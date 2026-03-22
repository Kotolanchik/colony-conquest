using Unity.Mathematics;

namespace ColonyConquest.Entertainment
{
    /// <summary>Формулы настроения и продуктивности из развлекательной спеки.</summary>
    public static class EntertainmentMath
    {
        public static float ComputeFinalMood(float baseMood, float diversity, float quality, float availability,
            float holidayBonus)
        {
            return math.clamp(baseMood + diversity * 0.1f + quality * 0.2f + availability * 0.15f + holidayBonus, 0f, 100f);
        }

        public static float ComputeProductivityModifier(float finalMood)
        {
            if (finalMood > 80f)
                return 0.10f;
            if (finalMood >= 60f)
                return 0.05f;
            if (finalMood >= 40f)
                return 0f;
            if (finalMood >= 20f)
                return -0.05f;
            return -0.15f;
        }

        public static float ComputeStressReduction(float availability, float quality)
        {
            return math.saturate(availability * 0.6f + quality * 0.4f);
        }

        public static float ComputeGamblingRisk(float policyMultiplier, float quality, float availability)
        {
            return math.clamp(0.05f * policyMultiplier + quality * 0.02f + availability * 0.01f, 0f, 0.5f);
        }
    }
}
