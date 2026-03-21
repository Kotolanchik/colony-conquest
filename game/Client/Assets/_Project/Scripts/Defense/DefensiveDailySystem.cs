using ColonyConquest.Analytics;
using ColonyConquest.Core;
using ColonyConquest.Simulation;
using ColonyConquest.Story;
using ColonyConquest.Technology;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace ColonyConquest.Defense
{
    /// <summary>Суточная симуляция укреплений: стройка под огнём, урон сооружениям, питание high-tech.</summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(ColonyCalendarAdvanceSystem))]
    public partial struct DefensiveDailySystem : ISystem
    {
        private const uint EventDefensiveBuilt = 0xE401;
        private const uint EventDefensiveDestroyed = 0xE402;
        private const uint EventDefensiveBlockedByEra = 0xE403;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameCalendarState>();
            state.RequireForUpdate<SimulationRootState>();
            state.RequireForUpdate<DefensiveSimulationSingleton>();
            state.RequireForUpdate<DefensiveSimulationState>();
            state.RequireForUpdate<ColonyTechProgressState>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var day = SystemAPI.GetSingleton<GameCalendarState>().DayIndex;
            ref var sim = ref SystemAPI.GetSingletonRW<DefensiveSimulationState>().ValueRW;
            if (sim.LastProcessedDay == day)
                return;
            sim.LastProcessedDay = day;

            var tick = SystemAPI.GetSingleton<SimulationRootState>().SimulationTick;
            var techEra = SystemAPI.GetSingleton<ColonyTechProgressState>().CurrentEra;
            var orders = SystemAPI.GetSingletonBuffer<DefensiveConstructionOrderEntry>(ref state);
            var structures = SystemAPI.GetSingletonBuffer<DefensiveStructureRuntimeEntry>(ref state);

            var builtToday = 0u;
            var destroyedToday = 0u;
            var blockedByEraToday = 0u;

            for (var i = 0; i < orders.Length; i++)
            {
                var order = orders[i];
                if (order.IsCompleted != 0)
                    continue;

                if (order.BaseBuildHours <= 0f)
                    DefensiveSimulationMath.TryGetBaseBuildHours(order.Kind, out order.BaseBuildHours);
                if (order.EngineersAssigned == 0)
                    order.EngineersAssigned = sim.EngineersAssigned == 0 ? (ushort)1 : sim.EngineersAssigned;

                if (!DefensiveSimulationMath.IsTechEraAllowed(order.Kind, techEra))
                {
                    blockedByEraToday++;
                    orders[i] = order;
                    TryEnqueueStoryEvent(ref state, tick, EventDefensiveBlockedByEra, new FixedString64Bytes("defense-era-lock"));
                    continue;
                }

                var fireIntensity = order.UnderFireIntensity > sim.UnderFireIntensity
                    ? order.UnderFireIntensity
                    : sim.UnderFireIntensity;
                var dailyProgress = DefensiveSimulationMath.ComputeBuildProgressPerDay(order.BaseBuildHours,
                    sim.EngineerSkillLevel, order.EngineersAssigned, fireIntensity);
                order.Progress01 = math.saturate(order.Progress01 + dailyProgress);

                if (order.Progress01 >= 0.999f)
                {
                    order.Progress01 = 1f;
                    order.IsCompleted = 1;
                    sim.StructuresBuiltTotal++;
                    sim.LastStructureId++;
                    builtToday++;
                    structures.Add(BuildRuntime(sim.LastStructureId, order.Kind));
                    TryEnqueueStoryEvent(ref state, tick, EventDefensiveBuilt, new FixedString64Bytes("defense-built"));
                }

                orders[i] = order;
            }

            var availablePower = sim.PowerReserveKw;
            var hpRatioSum = 0f;
            var activeCount = 0u;
            for (var i = structures.Length - 1; i >= 0; i--)
            {
                var s = structures[i];
                var hasPower = s.EnergyDemandKw <= 0f || availablePower >= s.EnergyDemandKw;
                if (s.EnergyDemandKw > 0f && hasPower)
                    availablePower -= s.EnergyDemandKw;
                s.IsOperational = (byte)(hasPower ? 1 : 0);

                var damage = DefensiveSimulationMath.ComputeDailyDamage(sim.IncomingDamagePressure, s.DefenseBonusPercent,
                    s.IsOperational != 0);
                var fireScale = 1f + sim.UnderFireIntensity * 0.2f;
                s.CurrentHp -= damage * fireScale;
                if (s.CurrentHp <= 0f)
                {
                    sim.StructuresDestroyedTotal++;
                    destroyedToday++;
                    structures.RemoveAt(i);
                    TryEnqueueStoryEvent(ref state, tick, EventDefensiveDestroyed, new FixedString64Bytes("defense-destroyed"));
                    continue;
                }

                hpRatioSum += s.CurrentHp / math.max(1f, s.MaxHp);
                activeCount++;
                structures[i] = s;
            }

            var avgHpRatio = activeCount == 0u ? 1f : hpRatioSum / activeCount;
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.DefenseActiveStructuresCount,
                structures.Length);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.DefenseAverageStructureHp01, avgHpRatio);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.DefenseStructuresBuiltTotal,
                sim.StructuresBuiltTotal);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.DefenseStructuresDestroyedTotal,
                sim.StructuresDestroyedTotal);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.DefenseOrdersBlockedByEraTotal,
                blockedByEraToday);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.DefenseBuildsCompletedToday, builtToday);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.DefenseDestroyedToday, destroyedToday);
        }

        private static DefensiveStructureRuntimeEntry BuildRuntime(uint id, DefensiveStructureKindId kind)
        {
            var hp = DefensiveSimulationMath.GetMaxHp(kind);
            return new DefensiveStructureRuntimeEntry
            {
                StructureId = id,
                Kind = kind,
                CurrentHp = hp,
                MaxHp = hp,
                DefenseBonusPercent = DefensiveSimulationMath.GetDefenseBonusPercent(kind),
                SlowEffectPercent = DefensiveSimulationMath.GetSlowEffectPercent(kind),
                ContactDamage = DefensiveSimulationMath.GetContactDamage(kind),
                EnergyDemandKw = DefensiveSimulationMath.GetEnergyDemandKw(kind),
                IsOperational = 1,
                IsHighTech = (byte)(DefensiveSimulationMath.IsHighTech(kind) ? 1 : 0)
            };
        }

        private static void TryEnqueueStoryEvent(ref SystemState state, uint tick, uint eventDefinitionId,
            in FixedString64Bytes label)
        {
            if (!SystemAPI.HasSingleton<StoryEventQueueSingleton>())
                return;

            var queue = SystemAPI.GetSingletonBuffer<GameEventQueueEntry>(ref state);
            queue.Add(new GameEventQueueEntry
            {
                Kind = StoryEventKind.Triggered,
                EventDefinitionId = eventDefinitionId,
                EnqueueSimulationTick = tick,
                DebugLabel = label
            });
        }
    }
}
