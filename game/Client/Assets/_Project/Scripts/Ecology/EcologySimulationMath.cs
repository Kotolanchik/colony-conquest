using ColonyConquest.Technology;
using Unity.Burst;
using Unity.Mathematics;

namespace ColonyConquest.Ecology
{
    /// <summary>Формулы runtime-симуляции экологии/климата/восстановления.</summary>
    [BurstCompile]
    public static class EcologySimulationMath
    {
        public static float GetAirCleanupEfficiency01(byte level)
        {
            return level switch
            {
                1 => 0.10f,
                2 => 0.50f,
                3 => 0.80f,
                4 => 1f,
                _ => 0f
            };
        }

        public static float GetWaterCleanupEfficiency01(byte level)
        {
            return level switch
            {
                1 => 0.50f,
                2 => 0.80f,
                3 => 0.90f,
                4 => 0.99f,
                _ => 0f
            };
        }

        public static float GetSoilRestorationDailyDelta01(byte level)
        {
            return level switch
            {
                1 => 0.05f / 365f,
                2 => 0.10f / 365f,
                3 => 0.20f / 365f,
                4 => 0.30f / 365f,
                _ => 0f
            };
        }

        public static float GetTechnologyAirPollutionMultiplier(TechEraId era)
        {
            return era switch
            {
                TechEraId.Era1_Foundation => 1f,
                TechEraId.Era2_Industrialization => 1.2f,
                TechEraId.Era3_WorldWar1 => 1.35f,
                TechEraId.Era4_WorldWar2 => 1.15f,
                TechEraId.Era5_ModernFuture => 0.70f,
                _ => 1f
            };
        }

        public static float GetTechnologyWaterPollutionMultiplier(TechEraId era)
        {
            return era switch
            {
                TechEraId.Era1_Foundation => 1f,
                TechEraId.Era2_Industrialization => 1.25f,
                TechEraId.Era3_WorldWar1 => 1.30f,
                TechEraId.Era4_WorldWar2 => 1.10f,
                TechEraId.Era5_ModernFuture => 0.75f,
                _ => 1f
            };
        }

        public static float GetTechnologySoilMultiplier(TechEraId era)
        {
            return era switch
            {
                TechEraId.Era1_Foundation => 1f,
                TechEraId.Era2_Industrialization => 1.1f,
                TechEraId.Era3_WorldWar1 => 1.15f,
                TechEraId.Era4_WorldWar2 => 1.05f,
                TechEraId.Era5_ModernFuture => 0.75f,
                _ => 1f
            };
        }

        public static float ComputeIndicatorDecayFromPollution(float unitsPerDay, float scale)
        {
            return math.max(0f, unitsPerDay) * math.max(0f, scale);
        }

        public static float ComputeForestRecoveryDelta01(float reforestationIntensity01, float biodiversity01)
        {
            var baseRecovery = 0.0008f;
            var effortRecovery = math.saturate(reforestationIntensity01) * 0.0045f;
            var biodiversityAssist = math.saturate(biodiversity01) * 0.0012f;
            return baseRecovery + effortRecovery + biodiversityAssist;
        }

        public static float ComputeBiodiversityRecoveryDelta01(float protection01, float forestCover01, float waterQuality01)
        {
            var baseRecovery = 0.0007f;
            var policyRecovery = math.saturate(protection01) * 0.0038f;
            var ecosystemRecovery = (math.saturate(forestCover01) + math.saturate(waterQuality01)) * 0.001f;
            return baseRecovery + policyRecovery + ecosystemRecovery;
        }

        public static float ComputeGreenhouseGasDelta(float airPollutionUnits, float carbonCapture01)
        {
            var produced = math.max(0f, airPollutionUnits) * 0.006f;
            var captured = produced * math.saturate(carbonCapture01) * 0.95f;
            return produced - captured;
        }

        public static float ComputeTemperatureAnomalyC(float greenhouseGasIndex, bool geoengineeringEnabled)
        {
            var anomaly = math.clamp(greenhouseGasIndex * 0.015f, 0f, 5f);
            if (geoengineeringEnabled)
                anomaly = math.max(0f, anomaly - 0.6f);
            return anomaly;
        }

        public static float ComputeSeaLevelRiseMeters(float currentRiseMeters, float temperatureAnomalyC)
        {
            return currentRiseMeters + math.max(0f, temperatureAnomalyC) * 0.000035f;
        }

        public static float ComputeExtremeWeatherRisk01(float temperatureAnomalyC, float combinedPollutionPercent0to100)
        {
            var tempFactor = math.saturate(temperatureAnomalyC / 5f);
            var pollutionFactor = math.saturate(combinedPollutionPercent0to100 / 100f);
            return math.saturate(tempFactor * 0.55f + pollutionFactor * 0.45f);
        }

        public static float ComputeSustainableDevelopment01(
            float ecosystemHealth01,
            float cleanEnergyShare01,
            float restorationEffort01)
        {
            return math.saturate(ecosystemHealth01 * 0.6f + cleanEnergyShare01 * 0.25f + restorationEffort01 * 0.15f);
        }

        public static float GetAirRateByGeneratorKind(byte generatorKind)
        {
            return generatorKind switch
            {
                1 => EcologyPollutionSourceRates.GetAirPollutionUnitsPerGameHour(EcologyAirPollutionSourceId.SteamEngine),
                2 => EcologyPollutionSourceRates.GetAirPollutionUnitsPerGameHour(EcologyAirPollutionSourceId.SteamEngine),
                3 => EcologyPollutionSourceRates.GetAirPollutionUnitsPerGameHour(EcologyAirPollutionSourceId.SteamEngine) * 0.7f,
                4 => EcologyPollutionSourceRates.GetAirPollutionUnitsPerGameHour(EcologyAirPollutionSourceId.CoalFiredPowerPlant),
                5 => EcologyPollutionSourceRates.GetAirPollutionUnitsPerGameHour(EcologyAirPollutionSourceId.OilRefinery),
                7 => 0f,
                _ => 0f
            };
        }
    }
}
