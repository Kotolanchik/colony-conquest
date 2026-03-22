using ColonyConquest.Analytics;
using ColonyConquest.Core;
using ColonyConquest.Justice;
using ColonyConquest.Simulation;
using ColonyConquest.Story;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace ColonyConquest.Entertainment
{
    /// <summary>Суточный апдейт досуга: настроение, продуктивность, стресс, праздничные бонусы и влияние на crime.</summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(ColonyCalendarAdvanceSystem))]
    public partial struct EntertainmentDailySystem : ISystem
    {
        private const uint EventHoliday = 0xC301;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameCalendarState>();
            state.RequireForUpdate<EntertainmentSimulationSingleton>();
            state.RequireForUpdate<EntertainmentSimulationState>();
            state.RequireForUpdate<EntertainmentFestivalState>();
            state.RequireForUpdate<SimulationRootState>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var day = SystemAPI.GetSingleton<GameCalendarState>().DayIndex;
            ref var sim = ref SystemAPI.GetSingletonRW<EntertainmentSimulationState>().ValueRW;
            if (sim.LastProcessedDay == day)
                return;
            sim.LastProcessedDay = day;

            ref var festivals = ref SystemAPI.GetSingletonRW<EntertainmentFestivalState>().ValueRW;
            festivals.HolidayMoodBonus = 0f;
            if (ShouldRunHoliday(day))
            {
                festivals.LastFestivalDay = day;
                festivals.HolidayMoodBonus = 12f;
                var tick = SystemAPI.GetSingleton<SimulationRootState>().SimulationTick;
                TryEnqueueStoryEvent(ref state, tick, EventHoliday, new FixedString64Bytes("holiday"));
            }

            var policyMultiplier = sim.GamblingPolicy switch
            {
                0 => 0.2f,
                2 => 0.7f,
                _ => 1f
            };

            sim.FinalMood = EntertainmentMath.ComputeFinalMood(
                sim.BaseMood,
                sim.Diversity,
                sim.Quality,
                sim.Availability,
                festivals.HolidayMoodBonus);
            sim.ProductivityModifier = EntertainmentMath.ComputeProductivityModifier(sim.FinalMood);
            sim.StressReduction = EntertainmentMath.ComputeStressReduction(sim.Availability / 100f, sim.Quality / 100f);
            sim.GamblingRisk = EntertainmentMath.ComputeGamblingRisk(policyMultiplier, sim.Quality / 100f,
                sim.Availability / 100f);

            // Интеграция с crime: доступность развлечений снижает преступность.
            if (SystemAPI.HasSingleton<CrimeJusticeState>())
            {
                ref var justice = ref SystemAPI.GetSingletonRW<CrimeJusticeState>().ValueRW;
                justice.EntertainmentAccess01 = math.saturate(sim.Availability / 100f);
            }

            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.EntertainmentMoodFinal,
                sim.FinalMood);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.EntertainmentProductivityModifier,
                sim.ProductivityModifier);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.EntertainmentStressReduction,
                sim.StressReduction);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.EntertainmentGamblingRisk,
                sim.GamblingRisk);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.EntertainmentAvailability,
                sim.Availability);
        }

        private static bool ShouldRunHoliday(uint day)
        {
            // Демо-праздники: каждые ~90 игровых дней.
            return day > 0 && day % 90u == 0;
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
