using ColonyConquest.Analytics;
using ColonyConquest.Core;
using ColonyConquest.Simulation;
using ColonyConquest.Story;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace ColonyConquest.Technology
{
    /// <summary>Суточная симуляция дерева технологий: активное исследование, разблокировки и переходы эпох.</summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(ColonyTechResearchDailySystem))]
    public partial struct TechTreeDailySystem : ISystem
    {
        private const uint EventTechnologyUnlocked = 0xD101;
        private const uint EventEraTransition = 0xD102;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameCalendarState>();
            state.RequireForUpdate<ColonyTechProgressState>();
            state.RequireForUpdate<TechTreeSimulationSingleton>();
            state.RequireForUpdate<TechTreeSimulationState>();
            state.RequireForUpdate<SimulationRootState>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var day = SystemAPI.GetSingleton<GameCalendarState>().DayIndex;
            ref var runtime = ref SystemAPI.GetSingletonRW<TechTreeSimulationState>().ValueRW;
            var previousDay = runtime.LastProcessedDay;
            if (previousDay == day)
                return;
            runtime.LastProcessedDay = day;

            ref var tech = ref SystemAPI.GetSingletonRW<ColonyTechProgressState>().ValueRW;
            if (tech.CurrentEra == TechEraId.None)
                tech.CurrentEra = TechEraId.Era1_Foundation;

            var days = previousDay == uint.MaxValue ? 1u : math.max(1u, day - previousDay);
            runtime.ResearchPoolPoints += GetDailyResearchIncome(tech) * days;

            var unlocked = SystemAPI.GetSingletonBuffer<TechUnlockedEntry>(ref state);
            EnsureActiveResearch(ref runtime, ref unlocked, tech.CurrentEra);
            ProcessActiveResearch(ref runtime, ref unlocked, ref tech, ref state);
            UpdateEraProgressAndTransitions(ref runtime, ref unlocked, ref tech, ref state);

            tech.ResearchPointsAccumulated = runtime.ResearchPoolPoints;

            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.TechActiveResearchId,
                (ushort)runtime.ActiveResearch);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.TechResearchPoolPoints,
                runtime.ResearchPoolPoints);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.TechEraTransitionsTotal,
                runtime.EraTransitionsTotal);
        }

        private static float GetDailyResearchIncome(in ColonyTechProgressState tech)
        {
            var scientistMult = 1f + tech.ScientistsCount * 0.2f;
            var instMult = 1f + tech.ResearchInstitutions * TechResearchTuning.ResearchInstitutionBonusPerBuilding;
            var eraMult = TechResearchTuning.GetEraResearchMultiplier(tech.CurrentEra);
            return tech.ResearchPointsPerDay * scientistMult * instMult * eraMult;
        }

        private static void EnsureActiveResearch(ref TechTreeSimulationState runtime, ref DynamicBuffer<TechUnlockedEntry> unlocked,
            TechEraId currentEra)
        {
            if (runtime.ActiveResearch != TechDefinitionId.None && TechTreeCatalog.TryGet(runtime.ActiveResearch, out _))
                return;

            using var defs = TechTreeCatalog.GetAll(Allocator.Temp);
            for (var i = 0; i < defs.Length; i++)
            {
                var d = defs[i];
                if (d.Era > currentEra)
                    continue;
                if (IsUnlocked(ref unlocked, d.Id))
                    continue;
                if (d.Prerequisite != TechDefinitionId.None && !IsUnlocked(ref unlocked, d.Prerequisite))
                    continue;

                runtime.ActiveResearch = d.Id;
                runtime.ActiveResearchProgressPoints = 0f;
                return;
            }

            runtime.ActiveResearch = TechDefinitionId.None;
            runtime.ActiveResearchProgressPoints = 0f;
        }

        private static void ProcessActiveResearch(ref TechTreeSimulationState runtime, ref DynamicBuffer<TechUnlockedEntry> unlocked,
            ref ColonyTechProgressState tech, ref SystemState state)
        {
            if (runtime.ActiveResearch == TechDefinitionId.None)
                return;
            if (!TechTreeCatalog.TryGet(runtime.ActiveResearch, out var activeDef))
            {
                runtime.ActiveResearch = TechDefinitionId.None;
                runtime.ActiveResearchProgressPoints = 0f;
                return;
            }

            if (runtime.ResearchPoolPoints <= 0f)
                return;

            var need = math.max(0f, activeDef.CostPoints - runtime.ActiveResearchProgressPoints);
            var spend = math.min(need, runtime.ResearchPoolPoints);
            runtime.ResearchPoolPoints -= spend;
            runtime.ActiveResearchProgressPoints += spend;

            if (runtime.ActiveResearchProgressPoints + 1e-3f < activeDef.CostPoints)
                return;

            unlocked.Add(new TechUnlockedEntry { Id = activeDef.Id });
            tech.TechnologiesUnlocked++;
            IncrementEraUnlockCounter(ref runtime, activeDef.Era);

            var tick = SystemAPI.GetSingleton<SimulationRootState>().SimulationTick;
            TryEnqueueStoryEvent(ref state, tick, EventTechnologyUnlocked, new FixedString64Bytes("tech-unlocked"));

            runtime.ActiveResearch = TechDefinitionId.None;
            runtime.ActiveResearchProgressPoints = 0f;
        }

        private static void UpdateEraProgressAndTransitions(ref TechTreeSimulationState runtime,
            ref DynamicBuffer<TechUnlockedEntry> unlocked, ref ColonyTechProgressState tech, ref SystemState state)
        {
            var unlockedCurrentEra = CountUnlockedInEra(ref unlocked, tech.CurrentEra);
            var totalCurrentEra = math.max(1, TechTreeCatalog.CountEraTechs(tech.CurrentEra));
            tech.CurrentEraProgress01 = math.saturate(unlockedCurrentEra / (float)totalCurrentEra);

            var threshold = GetEraTransitionThreshold(tech.CurrentEra);
            if (tech.CurrentEra >= TechEraId.Era5_ModernFuture || tech.CurrentEraProgress01 < threshold)
                return;

            tech.CurrentEra = (TechEraId)((byte)tech.CurrentEra + 1);
            tech.CurrentEraProgress01 = 0f;
            runtime.EraTransitionsTotal++;

            var tick = SystemAPI.GetSingleton<SimulationRootState>().SimulationTick;
            TryEnqueueStoryEvent(ref state, tick, EventEraTransition, new FixedString64Bytes("era-transition"));
        }

        private static float GetEraTransitionThreshold(TechEraId era)
        {
            return era switch
            {
                TechEraId.Era1_Foundation => 0.50f,
                TechEraId.Era2_Industrialization => 0.60f,
                TechEraId.Era3_WorldWar1 => 0.70f,
                TechEraId.Era4_WorldWar2 => 0.80f,
                _ => 1f
            };
        }

        private static int CountUnlockedInEra(ref DynamicBuffer<TechUnlockedEntry> unlocked, TechEraId era)
        {
            var count = 0;
            for (var i = 0; i < unlocked.Length; i++)
            {
                if (!TechTreeCatalog.TryGet(unlocked[i].Id, out var d))
                    continue;
                if (d.Era == era)
                    count++;
            }

            return count;
        }

        private static bool IsUnlocked(ref DynamicBuffer<TechUnlockedEntry> unlocked, TechDefinitionId id)
        {
            for (var i = 0; i < unlocked.Length; i++)
            {
                if (unlocked[i].Id == id)
                    return true;
            }

            return false;
        }

        private static void IncrementEraUnlockCounter(ref TechTreeSimulationState runtime, TechEraId era)
        {
            switch (era)
            {
                case TechEraId.Era1_Foundation:
                    runtime.UnlocksEra1++;
                    break;
                case TechEraId.Era2_Industrialization:
                    runtime.UnlocksEra2++;
                    break;
                case TechEraId.Era3_WorldWar1:
                    runtime.UnlocksEra3++;
                    break;
                case TechEraId.Era4_WorldWar2:
                    runtime.UnlocksEra4++;
                    break;
                case TechEraId.Era5_ModernFuture:
                    runtime.UnlocksEra5++;
                    break;
            }
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
