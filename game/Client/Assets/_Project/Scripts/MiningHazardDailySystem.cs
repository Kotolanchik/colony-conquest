using ColonyConquest.Agriculture;
using ColonyConquest.Analytics;
using ColonyConquest.Simulation;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace ColonyConquest.Core
{
    /// <summary>Ежедневные риски §2.4 при добыче рядом с узлом <see cref="MiningDepositRuntime"/>.</summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(ColonyCalendarAdvanceSystem))]
    public partial struct MiningHazardDailySystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<MiningHazardProcessState>();
            state.RequireForUpdate<GameCalendarState>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var day = SystemAPI.GetSingleton<GameCalendarState>().DayIndex;
            ref var proc = ref SystemAPI.GetSingletonRW<MiningHazardProcessState>().ValueRW;
            if (proc.LastProcessedGameDay == day)
                return;
            proc.LastProcessedGameDay = day;

            var tick = SystemAPI.GetSingleton<SimulationRootState>().SimulationTick;

            float3 playerPos = float3.zero;
            var hasPlayer = false;
            foreach (var lt in SystemAPI.Query<RefRO<LocalTransform>>().WithAll<PlayerMoveTargetTag>())
            {
                playerPos = lt.ValueRO.Position;
                hasPlayer = true;
                break;
            }

            if (!hasPlayer)
                return;

            var rng = Random.CreateFromIndex(math.hash(new uint2((uint)day, tick)));

            foreach (var (depositRo, transformRo) in SystemAPI.Query<RefRO<MiningDepositRuntime>, RefRO<LocalTransform>>())
            {
                if (depositRo.ValueRO.AmountRemaining <= 0f)
                    continue;

                var d = transformRo.ValueRO.Position - playerPos;
                var distSq = d.x * d.x + d.z * d.z;
                if (distSq > 16f)
                    continue;

                Roll(MiningHazardTuning.CaveInDaily, 1u, "cave_in", ref rng);
                Roll(MiningHazardTuning.SuffocationDaily, 2u, "suffocation", ref rng);
                Roll(MiningHazardTuning.GasExplosionDaily, 3u, "gas_explosion", ref rng);
                Roll(MiningHazardTuning.FloodingDaily, 4u, "flooding", ref rng);
                if (depositRo.ValueRO.Kind == MiningDepositKindId.Uranium)
                    Roll(MiningHazardTuning.RadiationExposureDaily, 5u, "radiation", ref rng);
            }
        }

        private static void Roll(float p, uint metricId, string label, ref Random rng)
        {
            if (rng.NextFloat() >= p)
                return;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"[Mining hazard] {label} (p={p})");
#endif
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, metricId, 1f);
        }
    }
}
