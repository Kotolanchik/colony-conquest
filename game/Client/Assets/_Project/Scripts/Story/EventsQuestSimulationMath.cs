using ColonyConquest.Technology;
using Unity.Burst;
using Unity.Mathematics;

namespace ColonyConquest.Story
{
    /// <summary>Формулы выбора событий и генерации квестов для AI Director.</summary>
    [BurstCompile]
    public static class EventsQuestSimulationMath
    {
        public static bool IsEraAllowed(TechEraId currentEra, byte minEra, byte maxEra)
        {
            var era = (byte)currentEra;
            if (era == 0)
                era = 1;
            if (minEra > 0 && era < minEra)
                return false;
            if (maxEra > 0 && era > maxEra)
                return false;
            return true;
        }

        public static bool IsEligible(in StoryEventDefinitionEntry def, in AiDirectorDimensionsState dims, TechEraId era)
        {
            if (!IsEraAllowed(era, def.MinEra, def.MaxEra))
                return false;
            if (dims.Wealth0to100 < def.MinWealth)
                return false;
            if (dims.Security0to100 < def.MinSecurity)
                return false;
            if (dims.Stability0to100 < def.MinStability)
                return false;
            if (dims.Progress0to100 < def.MinProgress)
                return false;
            if (dims.Tension0to100 < def.MinTension)
                return false;
            if (def.MaxTension > 0f && dims.Tension0to100 > def.MaxTension)
                return false;
            return true;
        }

        public static float ComputeEventWeight(in StoryEventDefinitionEntry def, in AiDirectorDimensionsState dims,
            AiDirectorPolicyKind policy)
        {
            var weight = math.max(0.05f, def.BaseWeight);
            var normalizedTension = math.saturate(dims.Tension0to100 / 100f);
            var policyBoost = policy switch
            {
                AiDirectorPolicyKind.Relief => def.Category == StoryEventCategory.Social ? 1.9f : 0.9f,
                AiDirectorPolicyKind.Challenge => def.Category is StoryEventCategory.Military or StoryEventCategory.NaturalDisaster
                    ? 1.8f
                    : 0.95f,
                AiDirectorPolicyKind.Stabilize => def.Category == StoryEventCategory.Social ? 1.6f : 0.9f,
                AiDirectorPolicyKind.Military => def.Category == StoryEventCategory.Military ? 2f : 0.85f,
                _ => 1f
            };

            var tensionCurve = def.Category switch
            {
                StoryEventCategory.Social => math.lerp(1.3f, 0.8f, normalizedTension),
                StoryEventCategory.Global => math.lerp(0.8f, 1.2f, normalizedTension),
                StoryEventCategory.Military => math.lerp(0.9f, 1.5f, normalizedTension),
                StoryEventCategory.NaturalDisaster => math.lerp(0.85f, 1.25f, normalizedTension),
                _ => math.lerp(0.9f, 1.1f, normalizedTension)
            };

            return math.max(0.01f, weight * policyBoost * tensionCurve);
        }

        public static float ComputeSeverity01(float baseSeverity01, in AiDirectorDimensionsState dims, float climateRisk01,
            float militaryPressure01)
        {
            var severity = math.saturate(baseSeverity01);
            severity += math.saturate(dims.Tension0to100 / 100f) * 0.25f;
            severity += math.saturate((100f - dims.Stability0to100) / 100f) * 0.15f;
            severity += math.saturate(climateRisk01) * 0.10f;
            severity += math.saturate(militaryPressure01) * 0.10f;
            return math.saturate(severity);
        }

        public static QuestTemplateId PickQuestTemplate(StoryEventCategory category, float random01)
        {
            switch (category)
            {
                case StoryEventCategory.Military:
                    return random01 < 0.55f ? QuestTemplateId.Defend : QuestTemplateId.Eliminate;
                case StoryEventCategory.NaturalDisaster:
                    return random01 < 0.5f ? QuestTemplateId.Investigate : QuestTemplateId.Delivery;
                case StoryEventCategory.Economic:
                    return random01 < 0.5f ? QuestTemplateId.Delivery : QuestTemplateId.Find;
                case StoryEventCategory.Technology:
                    return random01 < 0.5f ? QuestTemplateId.Investigate : QuestTemplateId.Find;
                case StoryEventCategory.Social:
                    return random01 < 0.5f ? QuestTemplateId.Escort : QuestTemplateId.Investigate;
                default:
                    return random01 < 0.5f ? QuestTemplateId.Defend : QuestTemplateId.Investigate;
            }
        }

        public static byte ComputeQuestDifficulty(float severity01, float random01)
        {
            var score = math.saturate(severity01 * 0.8f + random01 * 0.4f);
            if (score >= 0.75f)
                return 3;
            if (score >= 0.45f)
                return 2;
            return 1;
        }

        public static uint ComputeQuestDurationDays(QuestTemplateId template, byte difficulty)
        {
            var baseDays = template switch
            {
                QuestTemplateId.Delivery => 2u,
                QuestTemplateId.Escort => 3u,
                QuestTemplateId.Eliminate => 4u,
                QuestTemplateId.Find => 3u,
                QuestTemplateId.Defend => 4u,
                _ => 3u
            };
            return baseDays + (uint)math.max(0, difficulty - 1);
        }

        public static float ComputeQuestReward(float severity01, byte difficulty, float economyScale01)
        {
            var baseReward = 40f + severity01 * 160f;
            var diffMult = 1f + (difficulty - 1) * 0.45f;
            var economyMult = 0.8f + math.saturate(economyScale01) * 0.6f;
            return math.max(10f, baseReward * diffMult * economyMult);
        }

        public static float ComputeQuestProgressDelta01(QuestTemplateId template, float security01, float stability01,
            float supplyAdequacy01, float militaryReadiness01, float random01)
        {
            var templateFactor = template switch
            {
                QuestTemplateId.Defend => (security01 * 0.45f + militaryReadiness01 * 0.45f + supplyAdequacy01 * 0.1f),
                QuestTemplateId.Eliminate => (militaryReadiness01 * 0.55f + security01 * 0.25f + supplyAdequacy01 * 0.2f),
                QuestTemplateId.Delivery => (supplyAdequacy01 * 0.55f + security01 * 0.2f + stability01 * 0.25f),
                QuestTemplateId.Escort => (security01 * 0.35f + stability01 * 0.35f + supplyAdequacy01 * 0.3f),
                QuestTemplateId.Find => (stability01 * 0.35f + security01 * 0.25f + random01 * 0.4f),
                _ => (stability01 * 0.45f + security01 * 0.25f + random01 * 0.3f)
            };
            return math.saturate(0.08f + templateFactor * 0.25f);
        }

        public static PersonalStoryArchetype ResolveArchetype(float morale01, float stress01, float health01, float random01)
        {
            if (morale01 > 0.75f && health01 > 0.65f)
                return random01 < 0.65f ? PersonalStoryArchetype.Hero : PersonalStoryArchetype.Sage;
            if (stress01 > 0.7f && morale01 < 0.4f)
                return random01 < 0.55f ? PersonalStoryArchetype.Victim : PersonalStoryArchetype.Rebirth;
            if (morale01 > 0.6f && stress01 < 0.45f)
                return PersonalStoryArchetype.Lovers;
            return random01 < 0.5f ? PersonalStoryArchetype.Rebirth : PersonalStoryArchetype.Villain;
        }
    }
}
