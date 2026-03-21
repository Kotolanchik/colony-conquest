using Unity.Burst;
using Unity.Mathematics;

namespace ColonyConquest.Economy
{
    /// <summary>Формулы full-runtime экономики по <c>spec/economic_system_specification.md</c>.</summary>
    [BurstCompile]
    public static class EconomySimulationMath
    {
        public static byte GetPhaseDurationDays(EconomyCyclePhase phase)
        {
            return phase switch
            {
                EconomyCyclePhase.Accumulation => 4,
                EconomyCyclePhase.Expansion => 5,
                EconomyCyclePhase.Preparation => 2,
                EconomyCyclePhase.Warfare => 3,
                EconomyCyclePhase.Recovery => 4,
                _ => 3
            };
        }

        public static EconomyCyclePhase GetNextPhase(EconomyCyclePhase phase)
        {
            return phase switch
            {
                EconomyCyclePhase.Accumulation => EconomyCyclePhase.Expansion,
                EconomyCyclePhase.Expansion => EconomyCyclePhase.Preparation,
                EconomyCyclePhase.Preparation => EconomyCyclePhase.Warfare,
                EconomyCyclePhase.Warfare => EconomyCyclePhase.Recovery,
                _ => EconomyCyclePhase.Accumulation
            };
        }

        /// <summary>
        /// Эффективность здания (§3.3): база × workers × energy^0.8 × wear × skill × masters.
        /// Доп. штрафы при энергии &lt;50% / &lt;25%.
        /// </summary>
        public static float ComputeBuildingEfficiency(
            float baseSpeedMultiplier,
            float workersFact,
            float workersOptimal,
            float energyRatio01,
            float wear01,
            float averageSkill0To100,
            byte masterCount)
        {
            workersOptimal = math.max(workersOptimal, 1f);
            var workersMul = 0.5f + 0.5f * math.sqrt(math.max(workersFact / workersOptimal, 0f));
            var energyMul = math.pow(math.saturate(energyRatio01), 0.8f);
            var wearPenalty = math.saturate(wear01 / 2f);
            var wearMul = 1f - wearPenalty;
            var skillMul = 1f + math.saturate(averageSkill0To100) / 200f;
            var mastersMul = 1f + masterCount * 0.1f;

            var result = math.max(0f, baseSpeedMultiplier) * workersMul * energyMul * wearMul * skillMul * mastersMul;
            if (energyRatio01 < 0.25f)
                result *= 0.4f;
            else if (energyRatio01 < 0.5f)
                result *= 0.7f;
            return result;
        }

        public static float GetMilitaryPrioritySpeedMultiplier(EconomyProductionPriority priority)
        {
            return priority switch
            {
                EconomyProductionPriority.Critical => 1.5f,
                EconomyProductionPriority.High => 1.2f,
                EconomyProductionPriority.Low => 0.7f,
                EconomyProductionPriority.Minimal => 0.5f,
                _ => 1f
            };
        }

        public static float GetMilitaryPriorityEfficiencyMultiplier(EconomyProductionPriority priority)
        {
            return priority switch
            {
                EconomyProductionPriority.Critical => 0.8f,
                EconomyProductionPriority.High => 0.9f,
                EconomyProductionPriority.Low => 1.1f,
                EconomyProductionPriority.Minimal => 1.2f,
                _ => 1f
            };
        }

        /// <summary>Штраф смешанного военного режима: 100% - (X/10)% where X is military share 0..100.</summary>
        public static float GetMixedModeEfficiencyPenalty(float militaryShare01)
        {
            return 1f - math.saturate(militaryShare01) * 0.1f;
        }

        /// <summary>§5.3: L = Lbase × (1 - D/Dmax)^0.5 × (1 + 0.2*I)</summary>
        public static float ComputeLogisticsEfficiency(float baseEfficiency, float averageDistanceKm, float maxDistanceKm,
            float infrastructure01)
        {
            var distancePenalty = math.pow(math.saturate(1f - averageDistanceKm / math.max(1f, maxDistanceKm)), 0.5f);
            var infraMul = 1f + 0.2f * math.saturate(infrastructure01);
            return math.max(0.05f, baseEfficiency * distancePenalty * infraMul);
        }

        public static float ComputeTransportTonKmPerDay(float payloadKg, float speedKmPerHour, float loadCoefficient01)
        {
            var tons = math.max(0f, payloadKg) / 1000f;
            var speed = math.max(0f, speedKmPerHour);
            var coeff = math.saturate(loadCoefficient01);
            return tons * speed * 24f * coeff;
        }

        public static float ComputeTransmissionLoss01(float distanceKm)
        {
            if (distanceKm < 1f)
                return 0.05f;
            if (distanceKm < 5f)
                return 0.10f;
            if (distanceKm < 20f)
                return 0.15f;
            return 0.20f;
        }

        public static float ComputeWarehouseProcessingSecondsPerTon(
            byte workers,
            float automation01,
            float organization01,
            bool overloaded)
        {
            var baseSeconds = 60f + 90f + 120f + 60f;
            var workersMul = 1f / math.max(1f, workers * 0.5f);
            var autoMul = 1f - 0.7f * math.saturate(automation01);
            var orgMul = 1f - 0.3f * math.saturate(organization01);
            var overloadMul = overloaded ? 1.5f : 1f;
            return math.max(10f, baseSeconds * workersMul * autoMul * orgMul * overloadMul);
        }

        public static float ComputeResearchPointsFromEconomy(float productionValuePerDay, float scientistsCount,
            float tradeVolumePerDay, float funding01, float libraryLevel)
        {
            var productionPoints = productionValuePerDay / 100f;
            var educationPoints = scientistsCount * 10f;
            var tradePoints = tradeVolumePerDay / 1000f * 5f;
            var baseRate = productionPoints + educationPoints + tradePoints;
            return baseRate * (1f + 0.5f * math.saturate(funding01)) * (1f + 0.1f * math.max(0f, libraryLevel));
        }

        public static float GetFacilityWearPerHour(EconomyFacilityKind kind, GameEpoch era)
        {
            if (kind == EconomyFacilityKind.Workshop)
            {
                return era switch
                {
                    GameEpoch.Epoch1_Foundation => 0.9f,
                    GameEpoch.Epoch2_Industrialization => 1.5f,
                    _ => 1.2f
                };
            }

            return kind switch
            {
                EconomyFacilityKind.Manufacture => 1.8f,
                EconomyFacilityKind.Factory => 2.5f,
                EconomyFacilityKind.Plant => 3.8f,
                EconomyFacilityKind.Complex => 4.8f,
                _ => 1.5f
            };
        }

        public static float GetUpgradeProductionMultiplier(byte upgradeLevel)
        {
            return upgradeLevel switch
            {
                2 => 2f,
                3 => 4f,
                4 => 8f,
                5 => 16f,
                _ => 1f
            };
        }
    }
}
