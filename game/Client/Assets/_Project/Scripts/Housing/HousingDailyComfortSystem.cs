using ColonyConquest.Analytics;
using ColonyConquest.Core;
using ColonyConquest.Simulation;
using ColonyConquest.Story;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace ColonyConquest.Housing
{
    /// <summary>Суточный пересчёт уюта жилья, износа и аварий инфраструктуры.</summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(HousingAssignmentSystem))]
    public partial struct HousingDailyComfortSystem : ISystem
    {
        private const uint EventHousingIncidentBase = 0xB701;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameCalendarState>();
            state.RequireForUpdate<HousingColonyState>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var day = SystemAPI.GetSingleton<GameCalendarState>().DayIndex;
            ref var colony = ref SystemAPI.GetSingletonRW<HousingColonyState>().ValueRW;
            if (colony.LastProcessedDay == day)
                return;
            colony.LastProcessedDay = day;

            var totalCapacity = 0;
            var totalResidents = 0;
            var overcrowded = 0;
            var incidents = 0;
            var comfortSum = 0f;
            var homes = 0;

            var isColdSeason = IsColdSeason(day);
            var tick = SystemAPI.GetSingleton<SimulationRootState>().SimulationTick;

            foreach (var (unitRw, comfortRw) in SystemAPI.Query<RefRW<HousingUnitRuntime>, RefRW<HousingComfortSnapshot>>())
            {
                ref var unit = ref unitRw.ValueRW;
                ref var comfort = ref comfortRw.ValueRW;

                comfort.ComfortScore = HousingMath.ComputeComfortScore(unit);
                comfort.OvercrowdingBand = HousingMath.GetOvercrowdingBand(unit);

                if (comfort.OvercrowdingBand == 1)
                    comfort.ComfortScore -= 6f;
                else if (comfort.OvercrowdingBand == 2)
                    comfort.ComfortScore -= 14f;
                else if (comfort.OvercrowdingBand == 3)
                    comfort.ComfortScore -= 24f;

                comfort.ComfortScore = math.clamp(comfort.ComfortScore, 0f, 100f);
                HousingMath.ComputeComfortEffects(comfort.ComfortScore, out comfort.MoodModifier,
                    out comfort.ProductivityModifier);

                var overcrowdingDecay = math.max(0f, unit.Residents - unit.Capacity) * 0.001f;
                var pollutionDecay = unit.Noise01 * 0.001f;
                unit.Condition01 = HousingMath.ComputeConditionNext(unit.Condition01, unit.BaseDecayPerDay, overcrowdingDecay,
                    pollutionDecay, unit.MaintenanceRepairPerDay);

                if (comfort.OvercrowdingBand > 0)
                    overcrowded++;

                if (isColdSeason && unit.HeatingCoverage01 < 0.2f)
                {
                    incidents++;
                    TryEnqueueStoryEvent(ref state, tick, EventHousingIncidentBase + 1u,
                        new FixedString64Bytes("heating-failure"));
                }

                if (unit.WaterCoverage01 < 0.3f)
                {
                    incidents++;
                    TryEnqueueStoryEvent(ref state, tick, EventHousingIncidentBase + 2u,
                        new FixedString64Bytes("water-shortage"));
                }

                if (unit.PowerCoverage01 < 0.25f)
                {
                    incidents++;
                    TryEnqueueStoryEvent(ref state, tick, EventHousingIncidentBase + 3u,
                        new FixedString64Bytes("power-overload"));
                }

                if (unit.SewageCoverage01 < 0.25f)
                {
                    incidents++;
                    TryEnqueueStoryEvent(ref state, tick, EventHousingIncidentBase + 4u,
                        new FixedString64Bytes("sanitary-crisis"));
                }

                totalCapacity += unit.Capacity;
                totalResidents += unit.Residents;
                comfortSum += comfort.ComfortScore;
                homes++;
            }

            colony.TotalCapacity = totalCapacity;
            colony.TotalResidents = totalResidents;
            colony.OvercrowdedUnits = overcrowded;
            colony.IncidentsToday = incidents;
            colony.AverageComfort = homes > 0 ? comfortSum / homes : 0f;

            var occupancyRatio = totalCapacity > 0 ? (float)totalResidents / totalCapacity : 0f;
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.HousingCapacityTotal, totalCapacity);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.HousingOccupancyRatio, occupancyRatio);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.HousingComfortScoreAvg,
                colony.AverageComfort);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.HousingIncidentsTotal, incidents);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.HousingOvercrowdingHouseholds,
                overcrowded);
        }

        private static bool IsColdSeason(uint day)
        {
            var dayOfYear = day % GameCalendarTuning.DaysPerGameYear;
            return dayOfYear >= 300 || dayOfYear <= 59;
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
