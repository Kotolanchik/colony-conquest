using Unity.Burst;
using Unity.Mathematics;

namespace ColonyConquest.Settlers
{
    /// <summary>Формулы симуляции поселенцев по <c>spec/settler_simulation_system_spec.md</c>.</summary>
    [BurstCompile]
    public static class SettlerSimulationMath
    {
        public static float ClampNeed(float value)
        {
            return math.clamp(value, 0f, 100f);
        }

        /// <summary>Штраф к настроению от неудовлетворённых нужд (§4.1, §6.5).</summary>
        public static float ComputeNeedMoodPenalty(in NeedsState needs)
        {
            var penalty = 0f;
            if (needs.Hunger > 50f)
                penalty += ((needs.Hunger - 50f) / 10f) * 5f;
            if (needs.Thirst > 50f)
                penalty += ((needs.Thirst - 50f) / 10f) * 8f;
            if (needs.Rest > 70f)
                penalty += ((needs.Rest - 70f) / 10f) * 10f;
            if (needs.Comfort < 30f)
                penalty += 10f;
            if (needs.Space < 30f)
                penalty += (30f - needs.Space) * 0.2f;
            if (needs.TemperatureComfort < 70f)
                penalty += ((70f - needs.TemperatureComfort) / 10f) * 2f;
            return penalty;
        }

        /// <summary>Накопление стресса за сутки (§4.2).</summary>
        public static float ComputeStressAccumulation(
            in NeedsState needs,
            in PhysiologyState physio,
            in MentalConditions mental,
            bool inCombat,
            bool hasInjury,
            float phobiaPressure01)
        {
            var stressSources = 0f;
            stressSources += math.max(0f, needs.Hunger - 40f) * 0.05f;
            stressSources += math.max(0f, needs.Thirst - 35f) * 0.08f;
            stressSources += math.max(0f, needs.Rest - 45f) * 0.06f;
            stressSources += math.max(0f, 40f - needs.TemperatureComfort) * 0.03f;
            stressSources += math.max(0f, physio.Pain - 20f) * 0.04f;
            stressSources += mental.PTSDLevel * 0.015f;
            if (inCombat)
                stressSources += 8f;
            if (hasInjury)
                stressSources += 3f;
            stressSources += math.saturate(phobiaPressure01) * 5f;
            return stressSources;
        }

        /// <summary>Снятие стресса за сутки (§4.2).</summary>
        public static float ComputeStressRecovery(
            float recreationNeed,
            bool isSleeping,
            float socialSupport01,
            float entertainmentStressReduction01)
        {
            var recovery = 0.2f * 60f;
            var recreationRelief = math.saturate((100f - recreationNeed) / 100f);
            recovery += recreationRelief * 12f;
            if (isSleeping)
                recovery += 6f;
            recovery += math.saturate(socialSupport01) * 5f;
            recovery += math.saturate(entertainmentStressReduction01) * 10f;
            return recovery;
        }

        /// <summary>Порог срыва (§4.4).</summary>
        public static float ComputeBreakThreshold(float mood, byte ptsdLevel)
        {
            return math.clamp(70f - mood / 5f + ptsdLevel / 10f, 35f, 120f);
        }

        /// <summary>Категория риска срыва.</summary>
        public static byte ResolveBreakRisk(float stress, float threshold)
        {
            if (stress >= threshold + 25f)
                return 3;
            if (stress >= threshold + 10f)
                return 2;
            if (stress >= threshold - 5f)
                return 1;
            return 0;
        }

        /// <summary>Итоговая эффективность труда (§9.2, приложение A).</summary>
        public static float ComputeWorkEfficiency(
            in PhysiologyState physio,
            float mood,
            float stress,
            float passionBonus01,
            float leadershipBonus01)
        {
            var health01 = math.saturate(physio.Health / math.max(1f, physio.MaxHealth));
            var moodMult = 0.5f + (mood + 100f) / 200f;
            var painMult = 1f - math.saturate(physio.Pain / 200f);
            var stressMult = 1f - math.saturate(stress / 300f);
            var value = health01 * moodMult * (1f + math.max(0f, passionBonus01)) * painMult * stressMult *
                        (1f + math.max(0f, leadershipBonus01));
            return math.clamp(value, 0.05f, 2f);
        }

        public static float ComputeLearningMultiplier(byte level)
        {
            return 2f / (1f + level * 0.1f);
        }

        public static float ComputeActionXp(
            float actionDifficulty,
            float qualityMultiplier,
            byte passionLevel,
            byte aptitudeLevel,
            byte level)
        {
            var baseXp = math.max(0f, actionDifficulty) * 10f * math.max(0.1f, qualityMultiplier);
            var passionMultiplier = 1f + math.clamp(passionLevel, (byte)0, (byte)3) * 0.5f;
            var aptitudeMultiplier = 1f + math.clamp(aptitudeLevel, (byte)0, (byte)5) * 0.2f;
            var learningMultiplier = ComputeLearningMultiplier(level);
            return baseXp * passionMultiplier * aptitudeMultiplier * learningMultiplier;
        }

        public static ushort ComputeExperienceToNextLevel(byte level)
        {
            return level switch
            {
                0 => 100,
                1 => 200,
                2 => 400,
                3 => 700,
                4 => 1100,
                5 => 1600,
                6 => 2200,
                7 => 2900,
                8 => 3700,
                9 => 4600,
                10 => 5600,
                11 => 6700,
                12 => 7900,
                13 => 9200,
                14 => 10600,
                _ => (ushort)math.min(ushort.MaxValue, 10600 + (level - 14) * 1500)
            };
        }

        public static float ComputeSkillDecayChance(byte daysSinceLastUse)
        {
            if (daysSinceLastUse <= 30)
                return 0f;
            return math.saturate((daysSinceLastUse - 30) * 0.01f);
        }

        public static ushort ComputeSkillDecayAmount(ushort experienceToNext)
        {
            return (ushort)math.max(1, (int)math.round(experienceToNext * 0.05f));
        }

        public static float GetAutonomyReactionTime(byte level)
        {
            return level switch
            {
                0 => 7.5f,
                1 => 15f,
                2 => 45f,
                3 => 3f,
                4 => 0f,
                _ => 12f
            };
        }

        public static float GetDecisionCooldown(byte level)
        {
            return level switch
            {
                0 => 7f,
                1 => 15f,
                2 => 45f,
                3 => 2f,
                4 => 0f,
                _ => 10f
            };
        }

        public static byte GetSkillCategory(byte skillId)
        {
            if (skillId <= 4)
                return 0;
            if (skillId <= 9)
                return 1;
            if (skillId <= 12)
                return 2;
            if (skillId <= 15)
                return 3;
            return 4;
        }

        public static bool TryGetTaskSkillId(byte taskType, out byte skillId)
        {
            switch (taskType)
            {
                case 1:
                    skillId = (byte)SettlerSkillId.Construction;
                    return true;
                case 2:
                    skillId = (byte)SettlerSkillId.Crafting;
                    return true;
                case 3:
                    skillId = (byte)SettlerSkillId.Research;
                    return true;
                case 4:
                    skillId = (byte)SettlerSkillId.Shooting;
                    return true;
                case 5:
                    skillId = (byte)SettlerSkillId.Melee;
                    return true;
                case 6:
                    skillId = (byte)SettlerSkillId.Mining;
                    return true;
                case 7:
                    skillId = (byte)SettlerSkillId.Growing;
                    return true;
                case 8:
                    skillId = (byte)SettlerSkillId.Medicine;
                    return true;
                case 9:
                    skillId = (byte)SettlerSkillId.Social;
                    return true;
                default:
                    skillId = (byte)SettlerSkillId.Survival;
                    return false;
            }
        }
    }
}
