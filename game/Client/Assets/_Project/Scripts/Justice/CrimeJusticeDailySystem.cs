using ColonyConquest.Analytics;
using ColonyConquest.Core;
using ColonyConquest.Entertainment;
using ColonyConquest.Religion;
using ColonyConquest.Simulation;
using ColonyConquest.Story;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace ColonyConquest.Justice
{
    /// <summary>Суточная симуляция: уровень преступности, инциденты, раскрываемость, наказания, рецидив.</summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(EntertainmentDailySystem))]
    [UpdateAfter(typeof(ColonyCalendarAdvanceSystem))]
    public partial struct CrimeJusticeDailySystem : ISystem
    {
        private const uint EventCrimeIncident = 0xC201;
        private const uint EventMurderCase = 0xC202;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameCalendarState>();
            state.RequireForUpdate<CrimeJusticeSingleton>();
            state.RequireForUpdate<CrimeJusticeState>();
            state.RequireForUpdate<PoliceForceState>();
            state.RequireForUpdate<JusticeCourtState>();
            state.RequireForUpdate<CrimeIncidentStatsState>();
            state.RequireForUpdate<SimulationRootState>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var day = SystemAPI.GetSingleton<GameCalendarState>().DayIndex;
            ref var justice = ref SystemAPI.GetSingletonRW<CrimeJusticeState>().ValueRW;
            if (justice.LastProcessedDay == day)
                return;
            justice.LastProcessedDay = day;

            ref var police = ref SystemAPI.GetSingletonRW<PoliceForceState>().ValueRW;
            ref var court = ref SystemAPI.GetSingletonRW<JusticeCourtState>().ValueRW;
            ref var stats = ref SystemAPI.GetSingletonRW<CrimeIncidentStatsState>().ValueRW;

            if (SystemAPI.HasSingleton<ReligionSimulationState>())
            {
                var faith = SystemAPI.GetSingleton<ReligionSimulationState>();
                justice.ReligiousPopulationPercent = math.clamp(faith.FaithLevelAvg, 0f, 100f);
            }

            stats.IncidentsToday = 0;
            stats.SolvedToday = 0;

            justice.CrimeLevelPercent = CrimeJusticeMath.ComputeCrimeLevelPercent(
                justice.PovertyPercent,
                justice.UnemploymentPercent,
                justice.InequalityGini,
                justice.IlliteracyPercent,
                justice.PoliceCoveragePercent,
                justice.CorruptionLevel,
                justice.OvercrowdingThousands,
                justice.ReligiousPopulationPercent,
                justice.EntertainmentAccess01,
                justice.PenaltySeverity);

            var policeEff = CrimeJusticeMath.ComputePoliceEfficiency(
                police.BaseEfficiency,
                police.SkillLevel,
                police.EquipmentLevel,
                police.OfficersPerPopulation);

            var tick = SystemAPI.GetSingleton<SimulationRootState>().SimulationTick;
            var rng = Random.CreateFromIndex(math.hash(new uint2(day, tick)));
            var incidentsToGenerate = (ushort)math.clamp((int)math.round(justice.CrimeLevelPercent / 20f), 0, 12);
            stats.IncidentsToday = incidentsToGenerate;

            for (var i = 0; i < incidentsToGenerate; i++)
            {
                var offense = RollOffense(ref rng, justice.CrimeLevelPercent);
                var solveChance = CrimeJusticeMath.ComputeSolveChance(offense, policeEff);
                var solved = rng.NextFloat() <= solveChance;
                if (solved)
                {
                    stats.SolvedToday++;
                    ApplyPunishment(offense, ref justice, ref court, ref stats, ref rng);
                }

                TryEnqueueStoryEvent(ref state, tick, EventCrimeIncident, new FixedString64Bytes("crime-incident"));
                if (offense == CrimeOffenseKindId.Murder)
                    TryEnqueueStoryEvent(ref state, tick, EventMurderCase, new FixedString64Bytes("murder-case"));
            }

            var recidivismChance = CrimeJusticeMath.ComputeRecidivismChance(
                justice.PenaltySeverity,
                court.PrisonConditions01,
                court.RehabPrograms01,
                court.OrganizedCrimePressure01);
            if (stats.InmatesCount > 0 && rng.NextFloat() <= recidivismChance * 0.1f)
                stats.RecidivistsCount++;

            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.CrimeLevelPercent,
                justice.CrimeLevelPercent);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.CrimeIncidentsDaily,
                stats.IncidentsToday);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.CrimeSolveRateDaily,
                stats.IncidentsToday > 0 ? stats.SolvedToday / (float)stats.IncidentsToday : 0f);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.CrimeInmatesCount,
                stats.InmatesCount);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.CrimeRecidivismCases,
                stats.RecidivistsCount);
        }

        private static CrimeOffenseKindId RollOffense(ref Random rng, float crimeLevel)
        {
            var r = rng.NextFloat();
            var severeBias = math.saturate((crimeLevel - 40f) / 60f);
            if (r < 0.50f - severeBias * 0.20f)
            {
                return rng.NextFloat() < 0.5f ? CrimeOffenseKindId.PettyTheft : CrimeOffenseKindId.FoodTheft;
            }

            if (r < 0.85f)
            {
                return rng.NextFloat() < 0.5f ? CrimeOffenseKindId.Robbery : CrimeOffenseKindId.AssaultGrievous;
            }

            return rng.NextFloat() < 0.5f ? CrimeOffenseKindId.Murder : CrimeOffenseKindId.Sabotage;
        }

        private static void ApplyPunishment(CrimeOffenseKindId offense, ref CrimeJusticeState justice, ref JusticeCourtState court,
            ref CrimeIncidentStatsState stats, ref Random rng)
        {
            // Упрощённый выбор наказаний из §5.2 и §7.2.
            if (offense == CrimeOffenseKindId.PettyTheft || offense == CrimeOffenseKindId.FoodTheft)
            {
                justice.CrimeLevelPercent = math.max(0f, justice.CrimeLevelPercent - 0.5f);
                return;
            }

            if (offense == CrimeOffenseKindId.Robbery || offense == CrimeOffenseKindId.AssaultGrievous)
            {
                stats.InmatesCount++;
                justice.CrimeLevelPercent = math.max(0f, justice.CrimeLevelPercent - 1.5f);
                return;
            }

            var deathPenalty = offense == CrimeOffenseKindId.Murder && justice.PenaltySeverity >= 0.8f &&
                               rng.NextFloat() < 0.2f * (1f - court.Corruption01);
            if (deathPenalty)
            {
                stats.DeathPenaltyCount++;
                justice.CrimeLevelPercent = math.max(0f, justice.CrimeLevelPercent - 2f);
            }
            else
            {
                stats.InmatesCount++;
                justice.CrimeLevelPercent = math.max(0f, justice.CrimeLevelPercent - 1.2f);
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
