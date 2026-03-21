using ColonyConquest.Analytics;
using ColonyConquest.Core;
using ColonyConquest.Diplomacy;
using ColonyConquest.Justice;
using ColonyConquest.Simulation;
using ColonyConquest.Story;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace ColonyConquest.Politics
{
    /// <summary>Суточная политическая симуляция: доктринные модификаторы, законы и связь с другими доменами.</summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(ColonyCalendarAdvanceSystem))]
    public partial struct PoliticalDailySystem : ISystem
    {
        private const uint EventPolicyChanged = 0xD201;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameCalendarState>();
            state.RequireForUpdate<PoliticalSimulationSingleton>();
            state.RequireForUpdate<PoliticalSimulationState>();
            state.RequireForUpdate<PoliticalLawState>();
            state.RequireForUpdate<SimulationRootState>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var day = SystemAPI.GetSingleton<GameCalendarState>().DayIndex;
            ref var sim = ref SystemAPI.GetSingletonRW<PoliticalSimulationState>().ValueRW;
            if (sim.LastProcessedDay == day)
                return;
            sim.LastProcessedDay = day;

            ref var laws = ref SystemAPI.GetSingletonRW<PoliticalLawState>().ValueRW;
            var doctrine = PoliticalMath.GetDoctrineModifiers(sim.Doctrine);
            sim.EconomyModifier = PoliticalMath.ClampModifier(doctrine.Economy + (laws.TaxRate01 < 0.25f ? 0.05f : -0.05f));
            sim.HappinessModifier = PoliticalMath.ClampModifier(doctrine.Happiness + (laws.CivilRightsLevel - 1) * 0.05f);
            sim.ScienceModifier = PoliticalMath.ClampModifier(doctrine.Science + (laws.CivilRightsLevel == 2 ? 0.05f : -0.05f));
            sim.DefenseModifier = PoliticalMath.ClampModifier(doctrine.Defense + laws.MilitaryBudgetGdp01 * 0.5f);
            sim.CrimeModifier = PoliticalMath.ClampModifier(doctrine.Crime +
                                                            (laws.CivilRightsLevel == 0 ? -0.10f : 0.05f));
            sim.DemocracyLevel01 = PoliticalMath.GetDemocracyLevel01(sim.GovernmentForm);

            sim.Stability01 = math.saturate(0.55f + doctrine.Stability +
                                            (laws.CivilRightsLevel == 1 ? 0.05f : 0f) -
                                            math.abs(laws.TaxRate01 - 0.20f) * 0.5f);
            sim.DecisionEfficiency01 = math.saturate(0.5f + sim.Stability01 * 0.3f +
                                                     (1f - sim.DemocracyLevel01) * 0.2f);

            if (sim.DecisionCooldownDaysRemaining > 0)
            {
                sim.DecisionCooldownDaysRemaining--;
            }
            else
            {
                TryAutoPolicyShift(ref sim, ref laws, ref state);
            }

            // Интеграции:
            if (SystemAPI.HasSingleton<ColonyTechProgressState>())
            {
                ref var tech = ref SystemAPI.GetSingletonRW<ColonyTechProgressState>().ValueRW;
                tech.ResearchPointsPerDay = math.max(5f, tech.ResearchPointsPerDay * (1f + sim.ScienceModifier * 0.02f));
            }

            if (SystemAPI.HasSingleton<CrimeJusticeState>())
            {
                ref var justice = ref SystemAPI.GetSingletonRW<CrimeJusticeState>().ValueRW;
                justice.PenaltySeverity = math.saturate(0.5f + sim.DefenseModifier * 0.5f - sim.DemocracyLevel01 * 0.2f);
            }

            if (SystemAPI.HasSingleton<DiplomacySimulationState>())
            {
                ref var dip = ref SystemAPI.GetSingletonRW<DiplomacySimulationState>().ValueRW;
                dip.AverageRelations = math.clamp(dip.AverageRelations + sim.HappinessModifier * 0.5f, -100f, 100f);
            }

            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.PoliticsStability01, sim.Stability01);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.PoliticsEconomyModifier,
                sim.EconomyModifier);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.PoliticsHappinessModifier,
                sim.HappinessModifier);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.PoliticsScienceModifier,
                sim.ScienceModifier);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.PoliticsDefenseModifier,
                sim.DefenseModifier);
        }

        private static void TryAutoPolicyShift(ref PoliticalSimulationState sim, ref PoliticalLawState laws,
            ref SystemState state)
        {
            var changed = false;
            if (sim.Stability01 < 0.35f)
            {
                laws.MilitaryBudgetGdp01 = math.min(0.5f, laws.MilitaryBudgetGdp01 + 0.05f);
                laws.CivilRightsLevel = (byte)math.max(0, laws.CivilRightsLevel - 1);
                changed = true;
            }
            else if (sim.Stability01 > 0.75f)
            {
                laws.CivilRightsLevel = (byte)math.min(2, laws.CivilRightsLevel + 1);
                laws.ImmigrationPolicy = (byte)math.min(2, laws.ImmigrationPolicy + 1);
                changed = true;
            }

            if (!changed)
            {
                sim.DecisionCooldownDaysRemaining = PoliticalMath.GetDecisionCycleDays(sim.GovernmentForm);
                return;
            }

            sim.DecisionCooldownDaysRemaining = PoliticalMath.GetDecisionCycleDays(sim.GovernmentForm);
            var tick = SystemAPI.GetSingleton<SimulationRootState>().SimulationTick;
            TryEnqueueStoryEvent(ref state, tick, EventPolicyChanged, new FixedString64Bytes("policy-shift"));
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
