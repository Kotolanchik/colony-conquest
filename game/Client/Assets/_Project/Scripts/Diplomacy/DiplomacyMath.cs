using Unity.Mathematics;

namespace ColonyConquest.Diplomacy
{
    /// <summary>Формулы отношений, торговли и решения ИИ о войне.</summary>
    public static class DiplomacyMath
    {
        public static float ComputeRelationsDelta(float tradeDealsCount, float commonEnemyFactor, float ideologyAffinity,
            float borderTension)
        {
            // §2.2: торговля (+1), общий враг (+0.5/день), идеология ±0.2/день, граница -0.1/день.
            return tradeDealsCount * 1f + commonEnemyFactor * 0.5f + ideologyAffinity * 0.2f - borderTension * 0.1f;
        }

        public static float ComputeTradePrice(float basePrice, float demandSupplyFactor, float distanceKm, float relationScore,
            float traderSkillLevel)
        {
            var distancePenalty = 1f + (math.max(0f, distanceKm) / 100f) * 0.10f;
            var relationModifier = GetRelationPriceModifier(relationScore);
            var traderSkillModifier = 1f - math.clamp(traderSkillLevel, 0f, 10f) * 0.05f;
            return basePrice * math.max(0.5f, demandSupplyFactor) * distancePenalty * relationModifier * traderSkillModifier;
        }

        public static float ComputeAiWarChancePerYear(float baseChance, float militaryPowerRatio, float relationScore,
            byte personality)
        {
            var strengthModifier = militaryPowerRatio >= 1f ? 2f : 0.5f;
            var relationModifier = relationScore >= 25f ? 0.1f : relationScore <= -50f ? 3f : 1f;
            var personalityModifier = personality switch
            {
                0 => 2f, // агрессор
                3 => 0.5f, // изоляционист
                _ => 1f
            };

            return baseChance * strengthModifier * relationModifier * personalityModifier;
        }

        public static bool AreIdeologiesCompatible(FactionIdeologyId a, FactionIdeologyId b)
        {
            if (a == b)
                return true;
            if ((a == FactionIdeologyId.Militarism && b == FactionIdeologyId.Pacifism) ||
                (a == FactionIdeologyId.Pacifism && b == FactionIdeologyId.Militarism))
                return false;
            if ((a == FactionIdeologyId.Capitalism && b == FactionIdeologyId.Communism) ||
                (a == FactionIdeologyId.Communism && b == FactionIdeologyId.Capitalism))
                return false;
            if ((a == FactionIdeologyId.Theocracy && b == FactionIdeologyId.Atheism) ||
                (a == FactionIdeologyId.Atheism && b == FactionIdeologyId.Theocracy))
                return false;
            return true;
        }

        private static float GetRelationPriceModifier(float relationScore)
        {
            // Друзья -10%, враги +30%
            if (relationScore >= 50f)
                return 0.90f;
            if (relationScore <= -50f)
                return 1.30f;
            return 1f;
        }
    }
}
