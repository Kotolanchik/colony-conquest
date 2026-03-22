using ColonyConquest.Analytics;
using ColonyConquest.Core;
using ColonyConquest.Economy;
using ColonyConquest.Politics;
using ColonyConquest.Simulation;
using ColonyConquest.Story;
using ColonyConquest.Technology;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace ColonyConquest.Ecology
{
    /// <summary>
    /// Суточный контур экологии: источники загрязнения, меры очистки, климат, восстановление природы и события.
    /// </summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(ColonyCalendarAdvanceSystem))]
    [UpdateAfter(typeof(AgrochemicalEcologyBridgeSystem))]
    [UpdateAfter(typeof(EconomySimulationDailySystem))]
    public partial struct EcologySimulationDailySystem : ISystem
    {
        private const uint EventEcologyCatastrophe = 0xEA01;
        private const uint EventEcologyProtest = 0xEA02;
        private const uint EventClimateAnomaly = 0xEA03;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameCalendarState>();
            state.RequireForUpdate<SimulationRootState>();
            state.RequireForUpdate<ColonyEcologyIndicatorsState>();
            state.RequireForUpdate<ColonyPollutionSummaryState>();
            state.RequireForUpdate<EcologySimulationSingleton>();
            state.RequireForUpdate<EcologySimulationState>();
            state.RequireForUpdate<EcologyMitigationState>();
            state.RequireForUpdate<EcologyDisasterState>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var day = SystemAPI.GetSingleton<GameCalendarState>().DayIndex;
            var tick = SystemAPI.GetSingleton<SimulationRootState>().SimulationTick;
            ref var ecoIndicators = ref SystemAPI.GetSingletonRW<ColonyEcologyIndicatorsState>().ValueRW;
            ref var summary = ref SystemAPI.GetSingletonRW<ColonyPollutionSummaryState>().ValueRW;
            ref var sim = ref SystemAPI.GetSingletonRW<EcologySimulationState>().ValueRW;
            ref var mitigation = ref SystemAPI.GetSingletonRW<EcologyMitigationState>().ValueRW;
            ref var disaster = ref SystemAPI.GetSingletonRW<EcologyDisasterState>().ValueRW;

            if (sim.LastProcessedDay == day)
                return;
            sim.LastProcessedDay = day;

            var techEra = SystemAPI.HasSingleton<ColonyTechProgressState>()
                ? SystemAPI.GetSingleton<ColonyTechProgressState>().CurrentEra
                : TechEraId.Era1_Foundation;

            if (techEra >= TechEraId.Era5_ModernFuture && mitigation.GeoengineeringEnabled == 0 && mitigation.CarbonCapture01 > 0.55f)
                mitigation.GeoengineeringEnabled = 1;

            // Политика влияет на чистоту: высокие гражданские права/низкий налог обычно коррелируют с зелёной повесткой.
            if (SystemAPI.HasSingleton<PoliticalSimulationState>() && SystemAPI.HasSingleton<PoliticalLawState>())
            {
                var politics = SystemAPI.GetSingleton<PoliticalSimulationState>();
                var laws = SystemAPI.GetSingleton<PoliticalLawState>();
                var policyBoost = math.saturate(
                    (laws.CivilRightsLevel == 2 ? 0.15f : 0f) +
                    (laws.TaxRate01 < 0.25f ? 0.06f : 0f) +
                    math.max(0f, politics.HappinessModifier) * 0.4f);
                mitigation.ReforestationIntensity01 = math.saturate(mitigation.ReforestationIntensity01 * 0.8f + policyBoost * 0.2f);
                mitigation.WildlifeProtection01 = math.saturate(mitigation.WildlifeProtection01 * 0.8f + policyBoost * 0.2f);
            }

            var airUnits = 0f;
            var waterUnits = 0f;
            var soilAnnualDelta01 = 0f;
            var restorationEffort01 = math.saturate((mitigation.ReforestationIntensity01 + mitigation.WildlifeProtection01 +
                                                     mitigation.CarbonCapture01) / 3f);

            var airSources = SystemAPI.GetSingletonBuffer<EcologyAirSourceEntry>(ref state);
            for (var i = 0; i < airSources.Length; i++)
            {
                var source = airSources[i];
                var rate = EcologyPollutionSourceRates.GetAirPollutionUnitsPerGameHour(source.SourceId);
                airUnits += rate * source.Count * math.max(0f, source.ActiveHoursPerDay) * math.saturate(source.Utilization01);
            }

            var waterSources = SystemAPI.GetSingletonBuffer<EcologyWaterSourceEntry>(ref state);
            for (var i = 0; i < waterSources.Length; i++)
            {
                var source = waterSources[i];
                var rate = EcologyPollutionSourceRates.GetWaterPollutionUnitsPerGameHour(source.SourceId);
                waterUnits += rate * source.Count * math.max(0f, source.ActiveHoursPerDay);

                var spillRate = EcologyPollutionSourceRates.GetWaterPollutionUnitsPerSpillEvent(source.SourceId);
                if (spillRate > 0f)
                {
                    var random = Random.CreateFromIndex(math.max(1u, day * 977u + (uint)(i + 1) * 181u));
                    if (random.NextFloat() < math.saturate(source.SpillChancePerDay01))
                        waterUnits += spillRate * source.Count;
                }
            }

            var soilSources = SystemAPI.GetSingletonBuffer<EcologySoilSourceEntry>(ref state);
            for (var i = 0; i < soilSources.Length; i++)
            {
                var source = soilSources[i];
                var annual = EcologySoilImpactRates.GetAnnualFertilityDelta01(source.SourceId);
                soilAnnualDelta01 += annual * math.saturate(source.Intensity01);
            }

            // Интеграция с экономикой: генерация энергии и интенсивное производство повышают загрязнение.
            float cleanEnergyShare01 = 0f;
            if (SystemAPI.HasSingleton<EconomySimulationSingleton>())
            {
                var generators = SystemAPI.GetSingletonBuffer<EconomyPowerGeneratorEntry>(ref state);
                var totalOutput = 0f;
                var cleanOutput = 0f;
                for (var i = 0; i < generators.Length; i++)
                {
                    var g = generators[i];
                    var output = g.OutputKw * math.saturate(g.OutputScale01);
                    totalOutput += output;

                    var airRate = EcologySimulationMath.GetAirRateByGeneratorKind((byte)g.Kind);
                    airUnits += airRate * 24f * math.saturate(g.OutputScale01) * math.max(0.4f, g.Efficiency01);

                    if (g.Kind is EconomyPowerGeneratorKind.HydroPlant
                        or EconomyPowerGeneratorKind.NuclearReactor
                        or EconomyPowerGeneratorKind.SolarFarm
                        or EconomyPowerGeneratorKind.WindFarm
                        or EconomyPowerGeneratorKind.WaterWheel)
                    {
                        cleanOutput += output;
                    }
                }

                cleanEnergyShare01 = totalOutput > 1e-3f ? cleanOutput / totalOutput : 0f;

                var facilities = SystemAPI.GetSingletonBuffer<EconomyProductionFacilityEntry>(ref state);
                var heavyIndustrialCount = 0f;
                for (var i = 0; i < facilities.Length; i++)
                {
                    var f = facilities[i];
                    var isHeavy = f.Kind is EconomyFacilityKind.Factory or EconomyFacilityKind.Plant or EconomyFacilityKind.Complex;
                    if (!isHeavy)
                        continue;
                    heavyIndustrialCount += math.max(0.2f, f.AssignedWorkers / 8f);
                    airUnits += f.BuildingWear01 * 5f;
                    waterUnits += f.BuildingWear01 * 4f;
                }

                waterUnits += heavyIndustrialCount * 6f;
                soilAnnualDelta01 -= heavyIndustrialCount * 0.0025f;
            }

            // Метан от животноводства (агрегированно через склад).
            if (SystemAPI.HasSingleton<ResourceStockpileSingleton>())
            {
                var stock = SystemAPI.GetSingletonBuffer<ResourceStockEntry>(ref state);
                var meatStock = ResourceStockpileOps.GetAmount(ref stock, ResourceId.LivestockMeat);
                airUnits += math.sqrt(math.max(0f, meatStock)) * 0.9f;
            }

            var eraAirMult = EcologySimulationMath.GetTechnologyAirPollutionMultiplier(techEra);
            var eraWaterMult = EcologySimulationMath.GetTechnologyWaterPollutionMultiplier(techEra);
            var eraSoilMult = EcologySimulationMath.GetTechnologySoilMultiplier(techEra);
            airUnits *= eraAirMult;
            waterUnits *= eraWaterMult;
            soilAnnualDelta01 *= eraSoilMult;

            var airCleanEff = EcologySimulationMath.GetAirCleanupEfficiency01(mitigation.AirCleanupLevel);
            var waterCleanEff = EcologySimulationMath.GetWaterCleanupEfficiency01(mitigation.WaterCleanupLevel);
            var soilRestoreEff = EcologySimulationMath.GetSoilRestorationDailyDelta01(mitigation.SoilRestorationLevel);

            airUnits *= 1f - airCleanEff * 0.92f;
            waterUnits *= 1f - waterCleanEff * 0.96f;

            sim.AirPollutionUnitsPerDay = airUnits;
            sim.WaterPollutionUnitsPerDay = waterUnits;
            sim.SoilContaminationUnitsPerDay = math.max(0f, -soilAnnualDelta01 * 365f * 100f);

            // Индикаторы качества (0..1): деградация + восстановление.
            var airDecay = EcologySimulationMath.ComputeIndicatorDecayFromPollution(airUnits, 0.00048f);
            var waterDecay = EcologySimulationMath.ComputeIndicatorDecayFromPollution(waterUnits, 0.00044f);
            var soilDailyDelta = soilAnnualDelta01 / 365f;

            ecoIndicators.AirQuality01 = math.clamp(
                ecoIndicators.AirQuality01 - airDecay + 0.0014f + mitigation.ReforestationIntensity01 * 0.0012f,
                0.02f, 1f);
            ecoIndicators.WaterQuality01 = math.clamp(
                ecoIndicators.WaterQuality01 - waterDecay + 0.0012f + mitigation.WildlifeProtection01 * 0.0007f,
                0.02f, 1f);
            ecoIndicators.SoilFertilityIndicator01 = math.clamp(
                ecoIndicators.SoilFertilityIndicator01 + soilDailyDelta + soilRestoreEff,
                0.02f, 1f);
            ecoIndicators.ForestCover01 = math.clamp(
                ecoIndicators.ForestCover01 - airDecay * 0.25f + EcologySimulationMath.ComputeForestRecoveryDelta01(
                    mitigation.ReforestationIntensity01, ecoIndicators.Biodiversity01),
                0.02f, 1f);
            ecoIndicators.Biodiversity01 = math.clamp(
                ecoIndicators.Biodiversity01 -
                (airDecay * 0.30f + waterDecay * 0.30f + math.max(0f, -soilDailyDelta) * 0.20f) +
                EcologySimulationMath.ComputeBiodiversityRecoveryDelta01(
                    mitigation.WildlifeProtection01, ecoIndicators.ForestCover01, ecoIndicators.WaterQuality01),
                0.02f, 1f);

            var pollutionPercent = EcologyPollutionMath.GetCombinedPollutionPercent0to100(ecoIndicators);
            summary.CombinedPollutionPercent0to100 = pollutionPercent;
            summary.Band = EcologyPollutionMath.GetPollutionLevelBand(pollutionPercent);

            // Климат.
            sim.GreenhouseGasIndex = math.clamp(
                sim.GreenhouseGasIndex + EcologySimulationMath.ComputeGreenhouseGasDelta(airUnits, mitigation.CarbonCapture01) -
                restorationEffort01 * 0.08f,
                0f, 1000f);
            sim.TemperatureAnomalyC = EcologySimulationMath.ComputeTemperatureAnomalyC(
                sim.GreenhouseGasIndex,
                mitigation.GeoengineeringEnabled != 0);
            sim.SeaLevelRiseMeters = EcologySimulationMath.ComputeSeaLevelRiseMeters(
                sim.SeaLevelRiseMeters,
                sim.TemperatureAnomalyC);
            sim.ExtremeWeatherRisk01 = EcologySimulationMath.ComputeExtremeWeatherRisk01(
                sim.TemperatureAnomalyC,
                pollutionPercent);

            sim.EcosystemHealth01 = math.saturate(
                (ecoIndicators.AirQuality01 + ecoIndicators.WaterQuality01 + ecoIndicators.SoilFertilityIndicator01 +
                 ecoIndicators.ForestCover01 + ecoIndicators.Biodiversity01) * 0.2f);
            sim.SustainableDevelopment01 = EcologySimulationMath.ComputeSustainableDevelopment01(
                sim.EcosystemHealth01,
                cleanEnergyShare01,
                restorationEffort01);

            var catastropheTriggered = false;
            var protestTriggered = false;
            var anomalyTriggered = false;
            var dayRandom = Random.CreateFromIndex(math.max(1u, day * 3571u + (uint)math.round(pollutionPercent * 10f)));

            if (summary.Band == PollutionLevelBand.Critical && dayRandom.NextFloat() < 0.18f)
            {
                catastropheTriggered = true;
                sim.CatastrophesTotal++;
                sim.EcologicalEventsTotal++;
                disaster.LastDisasterType = 1;
                disaster.LastDisasterDay = day;
                ApplyCatastrophePenalty(ref ecoIndicators, dayRandom.NextFloat(0.04f, 0.12f));
                TryEnqueueStoryEvent(ref state, tick, EventEcologyCatastrophe, new FixedString64Bytes("eco-catastrophe"));
            }

            if (!catastropheTriggered && summary.Band >= PollutionLevelBand.High && dayRandom.NextFloat() < 0.22f)
            {
                protestTriggered = true;
                sim.EcologicalEventsTotal++;
                disaster.LastDisasterType = 2;
                disaster.LastDisasterDay = day;
                TryEnqueueStoryEvent(ref state, tick, EventEcologyProtest, new FixedString64Bytes("eco-protest"));
            }

            if (sim.TemperatureAnomalyC >= 2.5f && dayRandom.NextFloat() < sim.ExtremeWeatherRisk01 * 0.3f)
            {
                anomalyTriggered = true;
                sim.EcologicalEventsTotal++;
                disaster.LastDisasterType = 3;
                disaster.LastDisasterDay = day;
                TryEnqueueStoryEvent(ref state, tick, EventClimateAnomaly, new FixedString64Bytes("climate-anomaly"));
            }

            // Повторно синхронизируем summary после потенциальных штрафов катастрофы.
            pollutionPercent = EcologyPollutionMath.GetCombinedPollutionPercent0to100(ecoIndicators);
            summary.CombinedPollutionPercent0to100 = pollutionPercent;
            summary.Band = EcologyPollutionMath.GetPollutionLevelBand(pollutionPercent);

            // Интеграция в аналитический snapshot.
            if (SystemAPI.HasSingleton<AnalyticsLocalSnapshot>())
            {
                ref var snap = ref SystemAPI.GetSingletonRW<AnalyticsLocalSnapshot>().ValueRW;
                snap.Social.Ecology01 = sim.EcosystemHealth01;
                var h = math.saturate((snap.Social.Education01 + snap.Social.Health01 + math.saturate(snap.Economy.GdpPerCapita / 2000f)) / 3f);
                snap.Social.HumanDevelopmentIndex = h;
            }

            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.EcologyAirQuality01,
                ecoIndicators.AirQuality01);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.EcologyWaterQuality01,
                ecoIndicators.WaterQuality01);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.EcologySoilFertility01,
                ecoIndicators.SoilFertilityIndicator01);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.EcologyForestCover01,
                ecoIndicators.ForestCover01);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.EcologyBiodiversity01,
                ecoIndicators.Biodiversity01);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.EcologyCombinedPollutionPercent,
                pollutionPercent);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.EcologyGreenhouseGasIndex,
                sim.GreenhouseGasIndex);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.EcologyTemperatureAnomalyC,
                sim.TemperatureAnomalyC);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.EcologyExtremeWeatherRisk01,
                sim.ExtremeWeatherRisk01);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.EcologySustainableDevelopment01,
                sim.SustainableDevelopment01);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.EcologyEventsToday,
                (catastropheTriggered ? 1 : 0) + (protestTriggered ? 1 : 0) + (anomalyTriggered ? 1 : 0));
        }

        private static void ApplyCatastrophePenalty(ref ColonyEcologyIndicatorsState eco, float penalty01)
        {
            eco.AirQuality01 = math.clamp(eco.AirQuality01 - penalty01 * 0.8f, 0.02f, 1f);
            eco.WaterQuality01 = math.clamp(eco.WaterQuality01 - penalty01 * 0.8f, 0.02f, 1f);
            eco.SoilFertilityIndicator01 = math.clamp(eco.SoilFertilityIndicator01 - penalty01 * 0.6f, 0.02f, 1f);
            eco.ForestCover01 = math.clamp(eco.ForestCover01 - penalty01 * 1.0f, 0.02f, 1f);
            eco.Biodiversity01 = math.clamp(eco.Biodiversity01 - penalty01 * 1.1f, 0.02f, 1f);
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
