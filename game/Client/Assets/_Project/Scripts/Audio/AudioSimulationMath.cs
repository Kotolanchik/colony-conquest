using ColonyConquest.Technology;
using ColonyConquest.UI;
using ColonyConquest.WorldMap;
using Unity.Burst;
using Unity.Mathematics;

namespace ColonyConquest.Audio
{
    /// <summary>Формулы адаптивной музыки и параметров SFX runtime.</summary>
    [BurstCompile]
    public static class AudioSimulationMath
    {
        [BurstCompile]
        public static byte ToThemeEra(TechEraId era)
        {
            return era switch
            {
                TechEraId.Era1_Foundation => 1,
                TechEraId.Era2_Industrialization => 2,
                TechEraId.Era3_WorldWar1 => 3,
                TechEraId.Era4_WorldWar2 => 4,
                _ => 5
            };
        }

        [BurstCompile]
        public static float ComputeCombatIntensity01(float readiness01, uint activeUnits, float supplyAdequacy01, float tension01)
        {
            var unitPressure = math.saturate(activeUnits / 400f);
            var crisisFromSupply = 1f - math.saturate(supplyAdequacy01);
            return math.saturate(readiness01 * 0.3f + unitPressure * 0.3f + tension01 * 0.25f + crisisFromSupply * 0.15f);
        }

        [BurstCompile]
        public static float ComputeBaseCrisis01(float colonyMorale01, float foodSatisfied01, float economyEfficiency01,
            float ecology01)
        {
            var moralePenalty = 1f - math.saturate(colonyMorale01);
            var foodPenalty = 1f - math.saturate(foodSatisfied01);
            var economyPenalty = 1f - math.saturate(economyEfficiency01);
            var ecologyPenalty = 1f - math.saturate(ecology01);
            return math.saturate(moralePenalty * 0.3f + foodPenalty * 0.3f + economyPenalty * 0.2f + ecologyPenalty * 0.2f);
        }

        [BurstCompile]
        public static float ComputeMusicIntensity01(float combat01, float crisis01, byte hourOfDay, UiCameraLevel level)
        {
            var nightBoost = (hourOfDay >= 20 || hourOfDay <= 5) ? 0.1f : 0f;
            var scaleBoost = level == UiCameraLevel.Strategic ? 0.12f : level == UiCameraLevel.Operational ? 0.08f : 0.03f;
            return math.saturate(combat01 * 0.55f + crisis01 * 0.35f + nightBoost + scaleBoost);
        }

        [BurstCompile]
        public static AudioMusicIntensityLevel ToMusicLevel(float intensity01)
        {
            if (intensity01 < 0.25f)
                return AudioMusicIntensityLevel.Calm;
            if (intensity01 < 0.5f)
                return AudioMusicIntensityLevel.Tense;
            if (intensity01 < 0.75f)
                return AudioMusicIntensityLevel.Battle;
            return AudioMusicIntensityLevel.Epic;
        }

        [BurstCompile]
        public static float ComputeCrossfadeSeconds(AudioMusicIntensityLevel from, AudioMusicIntensityLevel to)
        {
            var delta = math.abs((int)to - (int)from);
            return math.clamp(2f + delta * 0.8f, 2f, 4f);
        }

        [BurstCompile]
        public static float ComputeCategoryPriority01(AudioSfxCategory category)
        {
            return category switch
            {
                AudioSfxCategory.Interface => 1f,
                AudioSfxCategory.Characters => 0.9f,
                AudioSfxCategory.Combat => 0.85f,
                AudioSfxCategory.Environment => 0.6f,
                AudioSfxCategory.Tech => 0.55f,
                _ => 0.4f
            };
        }

        [BurstCompile]
        public static float ComputeLifetimeSeconds(AudioSfxCategory category)
        {
            return category switch
            {
                AudioSfxCategory.Interface => 0.35f,
                AudioSfxCategory.Characters => 1.5f,
                AudioSfxCategory.Combat => 2.1f,
                AudioSfxCategory.Environment => 3.2f,
                AudioSfxCategory.Tech => 2.4f,
                _ => 2.8f
            };
        }

        [BurstCompile]
        public static float ComputeAttenuation01(float distanceMeters)
        {
            var t = math.saturate((distanceMeters - 1f) / 999f);
            return math.saturate(1f - t);
        }

        [BurstCompile]
        public static float ComputeOcclusion01(float weatherSeverity01, bool hasWorldPosition)
        {
            if (!hasWorldPosition)
                return 0f;
            return math.saturate(0.1f + weatherSeverity01 * 0.35f);
        }

        [BurstCompile]
        public static float ComputeBiomeReverbSeconds(WorldBiomeId biome)
        {
            return biome switch
            {
                WorldBiomeId.Mountains => 1.9f,
                WorldBiomeId.Swamp => 0.8f,
                WorldBiomeId.Coast => 0.4f,
                WorldBiomeId.Ocean => 0.25f,
                _ => 1.0f
            };
        }

        [BurstCompile]
        public static float EstimateMemoryMb(uint activeVoices, uint active3d)
        {
            return 24f + activeVoices * 1.6f + active3d * 2.4f;
        }

        [BurstCompile]
        public static float EstimateLatencyMs(uint activeVoices, uint voiceBudget)
        {
            var pressure = voiceBudget > 0 ? math.saturate(activeVoices / (float)voiceBudget) : 1f;
            return 18f + pressure * 28f;
        }
    }
}
