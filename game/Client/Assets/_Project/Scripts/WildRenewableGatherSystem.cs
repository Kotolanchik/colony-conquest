using ColonyConquest.Agriculture;
using ColonyConquest.Economy;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ColonyConquest.Core
{
    /// <summary>
    /// Рыбалка и охота по §2.3: расход биомассы из <see cref="WildRenewableStockState"/> на склад
    /// (<see cref="ResourceId.FishCatch"/> / <see cref="ResourceId.LivestockMeat"/>).
    /// Узлы ручной добычи имеют приоритет — см. <see cref="ManualMiningGatherSystem"/>.
    /// </summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(ManualMiningGatherSystem))]
    public partial struct WildRenewableGatherSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<WildRenewableStockState>();
            state.RequireForUpdate<ResourceStockpileSingleton>();
        }

        public void OnUpdate(ref SystemState state)
        {
            if (SystemAPI.GetSingleton<InputCommandState>().InteractHeld == 0)
                return;

            float3 playerPos = float3.zero;
            var foundPlayer = false;
            foreach (var lt in SystemAPI.Query<RefRO<LocalTransform>>().WithAll<PlayerMoveTargetTag>())
            {
                playerPos = lt.ValueRO.Position;
                foundPlayer = true;
                break;
            }

            if (!foundPlayer)
                return;

            var radiusSq = WildRenewableGatherTuning.GatherRadius * WildRenewableGatherTuning.GatherRadius;
            var miningBlocks = false;
            foreach (var (dep, tr) in SystemAPI.Query<RefRO<MiningDepositRuntime>, RefRO<LocalTransform>>())
            {
                if (dep.ValueRO.AmountRemaining <= 0f)
                    continue;
                var d = tr.ValueRO.Position - playerPos;
                if (d.x * d.x + d.z * d.z <= radiusSq)
                {
                    miningBlocks = true;
                    break;
                }
            }

            if (miningBlocks)
                return;

            var bestDistSq = float.MaxValue;
            WildGatherKindId bestKind = WildGatherKindId.None;

            foreach (var (spot, tr) in SystemAPI
                         .Query<RefRO<WildGatherSpotRuntime>, RefRO<LocalTransform>>()
                         .WithAll<WildGatherSpotTag>())
            {
                var d = tr.ValueRO.Position - playerPos;
                var dsq = d.x * d.x + d.z * d.z;
                if (dsq > radiusSq || dsq >= bestDistSq)
                    continue;
                bestDistSq = dsq;
                bestKind = spot.ValueRO.Kind;
            }

            if (bestKind == WildGatherKindId.None)
                return;

            var dt = SystemAPI.Time.DeltaTime;
            ref var wild = ref SystemAPI.GetSingletonRW<WildRenewableStockState>().ValueRW;
            var stockBuf = SystemAPI.GetSingletonBuffer<ResourceStockEntry>();

            if (bestKind == WildGatherKindId.Fish)
            {
                if (wild.FishBiomass <= 0f)
                    return;
                var rate = WildRenewableGatherTuning.FishCatchPerGameHour / 3600f;
                var amount = math.min(rate * dt, wild.FishBiomass);
                if (amount <= 0f)
                    return;
                wild.FishBiomass -= amount;
                ResourceStockpileOps.Add(ref stockBuf, ResourceId.FishCatch, amount);
                return;
            }

            if (bestKind == WildGatherKindId.WildGame)
            {
                if (wild.WildGameBiomass <= 0f)
                    return;
                var rate = WildRenewableGatherTuning.WildGameMeatPerGameHour / 3600f;
                var amount = math.min(rate * dt, wild.WildGameBiomass);
                if (amount <= 0f)
                    return;
                wild.WildGameBiomass -= amount;
                ResourceStockpileOps.Add(ref stockBuf, ResourceId.LivestockMeat, amount);
            }
        }
    }
}
