using ColonyConquest.Analytics;
using ColonyConquest.Core;
using ColonyConquest.Simulation;
using ColonyConquest.Story;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace ColonyConquest.Religion
{
    /// <summary>Суточный религиозный апдейт: вера, конверсия, напряжение, культы и фазы священной войны.</summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(ColonyCalendarAdvanceSystem))]
    public partial struct ReligionDailySimulationSystem : ISystem
    {
        private const uint EventHolyWarDeclared = 0xB601;
        private const uint EventHolyWarActive = 0xB602;
        private const uint EventHolyWarResolved = 0xB603;
        private const uint EventCultRadicalization = 0xB604;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ReligionSimulationState>();
            state.RequireForUpdate<ReligiousConflictState>();
            state.RequireForUpdate<CultActivityState>();
            state.RequireForUpdate<HolyWarState>();
            state.RequireForUpdate<GameCalendarState>();
            state.RequireForUpdate<SimulationRootState>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var day = SystemAPI.GetSingleton<GameCalendarState>().DayIndex;
            ref var sim = ref SystemAPI.GetSingletonRW<ReligionSimulationState>().ValueRW;
            if (sim.LastProcessedDay == day)
                return;
            sim.LastProcessedDay = day;

            var tick = SystemAPI.GetSingleton<SimulationRootState>().SimulationTick;
            var rng = Random.CreateFromIndex(math.hash(new uint2(day, tick)));

            UpdateFaith(ref sim);
            var conversions = SimulateConversion(ref sim, ref rng);

            ref var conflict = ref SystemAPI.GetSingletonRW<ReligiousConflictState>().ValueRW;
            UpdateTension(ref conflict);

            ref var cult = ref SystemAPI.GetSingletonRW<CultActivityState>().ValueRW;
            UpdateCultActivity(ref cult, conflict.TensionScore);

            ref var holyWar = ref SystemAPI.GetSingletonRW<HolyWarState>().ValueRW;
            AdvanceHolyWarState(ref holyWar, conflict.TensionScore, tick, ref state);

            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.ReligionFaithLevelAvg,
                sim.FaithLevelAvg);
            AnalyticsHooks.RecordCounter(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.ReligionConversionEventsTotal,
                conversions);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.ReligionTensionAvg,
                conflict.TensionScore);

            if (cult.RadicalizationRisk >= 70f)
            {
                AnalyticsHooks.RecordCounter(AnalyticsDomain.LocalSettlement,
                    AnalyticsMetricIds.ReligionCultRadicalizationCases, 1);
                TryEnqueueStoryEvent(ref state, tick, EventCultRadicalization,
                    new FixedString64Bytes("cult-radicalization"));
            }
        }

        private static void UpdateFaith(ref ReligionSimulationState sim)
        {
            var delta = sim.TempleVisit * 1.0f +
                        sim.SermonExposure * 0.8f +
                        sim.PersonalCrisis * 0.6f +
                        sim.CommunityCohesion * 0.4f -
                        sim.EducationPressure * 0.5f -
                        sim.ProsperitySecularPull * 0.3f;

            sim.FaithLevelAvg = math.clamp(sim.FaithLevelAvg + delta, 0f, 100f);
            sim.DoubtAvg = math.clamp(sim.DoubtAvg + sim.EducationPressure * 0.7f - sim.CommunityCohesion * 0.4f, 0f, 100f);
            sim.FanaticismAvg = math.clamp(sim.FanaticismAvg + sim.SermonExposure * 0.5f + sim.PersonalCrisis * 0.4f -
                                           sim.ProsperitySecularPull * 0.2f, 0f, 100f);
        }

        private static int SimulateConversion(ref ReligionSimulationState sim, ref Random rng)
        {
            var influencePower = sim.TempleVisit * 30f + sim.SermonExposure * 25f + sim.CommunityCohesion * 20f;
            var socialPressure = sim.FanaticismAvg * 0.25f;
            var benefitExpectation = sim.PersonalCrisis * 20f;
            var resistance = sim.DoubtAvg * 0.30f + sim.EducationPressure * 20f + sim.ProsperitySecularPull * 15f;

            var chance = math.clamp((influencePower + socialPressure + benefitExpectation - resistance) / 100f, 0.01f, 0.85f);
            var roll = rng.NextFloat();
            if (roll > chance)
                return 0;

            var converted = math.max(1, (int)math.round(chance * 5f));
            sim.FaithLevelAvg = math.clamp(sim.FaithLevelAvg + converted * 0.2f, 0f, 100f);
            return converted;
        }

        private static void UpdateTension(ref ReligiousConflictState conflict)
        {
            var score = conflict.IncidentWeight +
                        conflict.IdeologyDistance * 0.6f +
                        conflict.ResourceCompetition * 0.5f +
                        conflict.PropagandaPressure * 0.4f -
                        conflict.LawEnforcement * 0.7f -
                        conflict.DialoguePrograms * 0.5f;
            conflict.TensionScore = math.clamp(score, 0f, 100f);
        }

        private static void UpdateCultActivity(ref CultActivityState cult, float tension)
        {
            var tensionPressure = math.max(0f, tension - 30f) * 0.25f;
            cult.RadicalizationRisk = math.clamp(cult.RadicalizationRisk +
                                                 cult.RecruitmentRate * 3f +
                                                 cult.HiddenCells * 2f +
                                                 tensionPressure * 0.1f, 0f, 100f);
        }

        private static void AdvanceHolyWarState(ref HolyWarState holyWar, float tension, uint tick, ref SystemState state)
        {
            if (holyWar.Phase == HolyWarPhase.None)
            {
                if (tension >= 81f && holyWar.Preparedness >= 50f)
                {
                    holyWar.Phase = HolyWarPhase.TriggerDetected;
                    holyWar.DayInPhase = 0;
                }

                return;
            }

            holyWar.DayInPhase++;
            switch (holyWar.Phase)
            {
                case HolyWarPhase.TriggerDetected when holyWar.DayInPhase >= 1:
                    holyWar.Phase = HolyWarPhase.DoctrineApproval;
                    holyWar.DayInPhase = 0;
                    break;
                case HolyWarPhase.DoctrineApproval when holyWar.DayInPhase >= 1:
                    holyWar.Phase = HolyWarPhase.UltimatumPhase;
                    holyWar.DayInPhase = 0;
                    break;
                case HolyWarPhase.UltimatumPhase when holyWar.DayInPhase >= 2:
                    holyWar.Phase = HolyWarPhase.Mobilization;
                    holyWar.DayInPhase = 0;
                    break;
                case HolyWarPhase.Mobilization when holyWar.DayInPhase >= 2:
                    holyWar.Phase = HolyWarPhase.HolyWarActive;
                    holyWar.DayInPhase = 0;
                    holyWar.ActiveDaysRemaining = 5;
                    AnalyticsHooks.RecordCounter(AnalyticsDomain.LocalSettlement,
                        AnalyticsMetricIds.ReligionHolyWarsStartedTotal, 1);
                    TryEnqueueStoryEvent(ref state, tick, EventHolyWarDeclared,
                        new FixedString64Bytes("holy-war-declared"));
                    TryEnqueueStoryEvent(ref state, tick, EventHolyWarActive,
                        new FixedString64Bytes("holy-war-active"));
                    break;
                case HolyWarPhase.HolyWarActive:
                    if (holyWar.ActiveDaysRemaining > 0)
                        holyWar.ActiveDaysRemaining--;
                    if (holyWar.ActiveDaysRemaining == 0 || tension < 40f)
                    {
                        holyWar.Phase = HolyWarPhase.Resolution;
                        holyWar.DayInPhase = 0;
                    }

                    break;
                case HolyWarPhase.Resolution when holyWar.DayInPhase >= 1:
                    holyWar.Phase = HolyWarPhase.None;
                    holyWar.DayInPhase = 0;
                    holyWar.ActiveDaysRemaining = 0;
                    TryEnqueueStoryEvent(ref state, tick, EventHolyWarResolved,
                        new FixedString64Bytes("holy-war-resolved"));
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
