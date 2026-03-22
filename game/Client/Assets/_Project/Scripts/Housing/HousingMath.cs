using Unity.Mathematics;

namespace ColonyConquest.Housing
{
    /// <summary>Формулы жилья: score расселения, уют и уровни перенаселения.</summary>
    public static class HousingMath
    {
        public static float ComputeHousingScore(in HousingUnitRuntime unit, in HousingComfortSnapshot snapshot,
            in HousingAssignmentRequestEntry request)
        {
            var available = GetHardCapacity(unit) - unit.Residents;
            if (available <= 0)
                return float.MinValue;

            var distanceWeight = (1f - math.saturate(unit.DistanceToWork01)) * request.WorkProximityWeight * 100f;
            var comfortWeight = math.saturate(snapshot.ComfortScore / 100f) * request.ComfortNeedWeight * 100f;
            var familyFitWeight = math.saturate((float)available / math.max(1f, request.HouseholdSize)) *
                                  request.FamilyNeedWeight * 100f;
            var utilityReliability = (unit.PowerCoverage01 + unit.WaterCoverage01 + unit.SewageCoverage01 +
                                      unit.HeatingCoverage01) * 25f;
            var overcrowdingPenalty = math.max(0f, unit.Residents - unit.Capacity) * 6f;
            return distanceWeight + comfortWeight + familyFitWeight + utilityReliability - overcrowdingPenalty;
        }

        public static float ComputeComfortScore(in HousingUnitRuntime unit)
        {
            var housingQuality = math.saturate(unit.BaseComfort / 100f) * 100f;
            var utilityScore = (unit.PowerCoverage01 + unit.WaterCoverage01 + unit.SewageCoverage01 + unit.HeatingCoverage01) *
                               0.25f * 100f;
            var environmentScore = (unit.Cleanliness01 + (1f - unit.Noise01) + unit.NeighborhoodQuality01) * (100f / 3f);
            var decorScore = math.saturate(unit.DecorLevel01) * 100f;

            var cleanlinessModifier = math.lerp(0.75f, 1.10f, math.saturate(unit.Cleanliness01));
            var noiseModifier = math.lerp(0.75f, 1.05f, math.saturate(1f - unit.Noise01));

            var score = (housingQuality * 0.4f + utilityScore * 0.3f + environmentScore * 0.2f + decorScore * 0.1f) *
                        cleanlinessModifier * noiseModifier;

            if (unit.PowerCoverage01 < 0.2f || unit.WaterCoverage01 < 0.2f || unit.SewageCoverage01 < 0.2f)
                score = math.min(score, 45f);

            return math.clamp(score, 0f, 100f);
        }

        public static float ComputeConditionNext(float currentCondition01, float baseDecayPerDay, float overcrowdingDecayPerDay,
            float pollutionDecayPerDay, float maintenanceRepairPerDay)
        {
            return math.clamp(currentCondition01 - baseDecayPerDay - overcrowdingDecayPerDay - pollutionDecayPerDay +
                              maintenanceRepairPerDay, 0f, 1f);
        }

        public static byte GetOvercrowdingBand(in HousingUnitRuntime unit)
        {
            if (unit.Capacity <= 0)
                return 0;
            var ratio = unit.Residents / (float)unit.Capacity;
            if (ratio <= 1f)
                return 0;
            if (ratio <= 1.2f)
                return 1;
            if (ratio <= 1.5f)
                return 2;
            return 3;
        }

        public static int GetHardCapacity(in HousingUnitRuntime unit)
        {
            return math.max(1, unit.Capacity * 2);
        }

        public static void ComputeComfortEffects(float comfortScore, out float moodModifier, out float productivityModifier)
        {
            if (comfortScore <= 20f)
            {
                moodModifier = -20f;
                productivityModifier = -0.10f;
                return;
            }

            if (comfortScore <= 40f)
            {
                moodModifier = -10f;
                productivityModifier = -0.05f;
                return;
            }

            if (comfortScore <= 60f)
            {
                moodModifier = 0f;
                productivityModifier = 0f;
                return;
            }

            if (comfortScore <= 80f)
            {
                moodModifier = 10f;
                productivityModifier = 0.05f;
                return;
            }

            moodModifier = 20f;
            productivityModifier = 0.10f;
        }
    }
}
