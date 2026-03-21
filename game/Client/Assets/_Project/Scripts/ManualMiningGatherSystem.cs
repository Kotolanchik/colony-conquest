using ColonyConquest.Agriculture;
using ColonyConquest.Economy;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ColonyConquest.Core
{
    /// <summary>
    /// Ручная добыча по <c>spec/agriculture_mining_spec.md</c> §2.2–2.3: инструмент, навык, усталость, качество руды, исчерпание.
    /// Требует удерживать <b>Interact</b> (E) рядом с узлом.
    /// </summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(PlayerMoveFromInputSystem))]
    public partial struct ManualMiningGatherSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ResourceStockpileSingleton>();
            state.RequireForUpdate<ManualMiningToolState>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var cmd = SystemAPI.GetSingleton<InputCommandState>();
            ref var tool = ref SystemAPI.GetSingletonRW<ManualMiningToolState>().ValueRW;
            var dt = SystemAPI.Time.DeltaTime;

            if (cmd.InteractHeld == 0)
            {
                tool.SessionWorkHours = math.max(0f, tool.SessionWorkHours - dt * 0.25f);
                return;
            }

            float3 playerPos = float3.zero;
            var foundPlayer = false;
            foreach (var lt in SystemAPI.Query<RefRO<LocalTransform>>().WithAll<PlayerMoveTargetTag>())
            {
                playerPos = lt.ValueRO.Position;
                foundPlayer = true;
                break;
            }

            if (!foundPlayer)
            {
                tool.SessionWorkHours = math.max(0f, tool.SessionWorkHours - dt * 0.25f);
                return;
            }

            var radiusSq = ManualMiningTuning.GatherRadius * ManualMiningTuning.GatherRadius;
            var bestDistSq = float.MaxValue;
            var bestEntity = Entity.Null;

            foreach (var (dep, tr, ent) in SystemAPI
                         .Query<RefRO<MiningDepositRuntime>, RefRO<LocalTransform>>()
                         .WithEntityAccess())
            {
                if (dep.ValueRO.AmountRemaining <= 0f)
                    continue;
                var d = tr.ValueRO.Position - playerPos;
                var dsq = d.x * d.x + d.z * d.z;
                if (dsq > radiusSq || dsq >= bestDistSq)
                    continue;
                bestDistSq = dsq;
                bestEntity = ent;
            }

            if (bestEntity == Entity.Null)
            {
                tool.SessionWorkHours = math.max(0f, tool.SessionWorkHours - dt * 0.25f);
                return;
            }

            if (MiningManualFormulas.GetBaseUnitsPerGameHour(tool.Tier) <= 0f || tool.DurabilityRemaining <= 0f)
                return;

            var depositLookup = state.GetComponentLookup<MiningDepositRuntime>(false);
            depositLookup.Update(ref state);
            ref var deposit = ref depositLookup.GetRefRW(bestEntity).ValueRW;
            if (deposit.InitialAmount <= 0f && deposit.AmountRemaining > 0f)
                deposit.InitialAmount = deposit.AmountRemaining;

            var initial = math.max(deposit.InitialAmount, 1e-3f);
            var fractionExtracted = (initial - deposit.AmountRemaining) / initial;

            var ratePerSec = MiningManualFormulas.GetBaseUnitsPerGameHour(tool.Tier) / 3600f;
            ratePerSec *= MiningManualFormulas.GetManualGatherKindMultiplier(deposit.Kind);
            ratePerSec *= MiningManualFormulas.GetMinerSkillMultiplier(tool.MinerSkillLevel);
            ratePerSec *= MiningManualFormulas.GetFatigueMultiplier(tool.SessionWorkHours);
            var remFrac = deposit.AmountRemaining / initial;
            ratePerSec *= MiningManualFormulas.GetDepletionProductionMultiplier(remFrac);

            var physical = ratePerSec * dt;
            if (physical > deposit.AmountRemaining)
                physical = deposit.AmountRemaining;
            if (physical > tool.DurabilityRemaining)
                physical = tool.DurabilityRemaining;

            if (physical <= 0f)
                return;

            if (!MiningDepositPrimaryResource.TryGetPrimaryResource(deposit.Kind, out var resId))
                return;

            var oreMult = MiningManualFormulas.UsesOreQualityBands(deposit.Kind)
                ? MiningManualFormulas.GetOreContentAverageMultiplier(fractionExtracted)
                : 1f;
            var resourceAmount = physical * oreMult;

            deposit.AmountRemaining -= physical;
            tool.DurabilityRemaining -= physical;
            tool.SessionWorkHours += dt / 3600f;

            var stockBuf = SystemAPI.GetSingletonBuffer<ResourceStockEntry>();
            ResourceStockpileOps.Add(ref stockBuf, resId, resourceAmount);
        }
    }
}
