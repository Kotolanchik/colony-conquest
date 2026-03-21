namespace ColonyConquest.Technology
{
    /// <summary>Модификатор эпохи §2.2 <c>spec/technology_tree_spec.md</c> (множитель к очкам/день).</summary>
    public static class TechResearchTuning
    {
        public static float GetEraResearchMultiplier(TechEraId era)
        {
            return era switch
            {
                TechEraId.Era1_Foundation => 1f,
                TechEraId.Era2_Industrialization => 1.5f,
                TechEraId.Era3_WorldWar1 => 2f,
                TechEraId.Era4_WorldWar2 => 3f,
                TechEraId.Era5_ModernFuture => 5f,
                _ => 1f,
            };
        }

        /// <summary>Доля института в бонусе к очкам (упрощённо относительно §2.1).</summary>
        public const float ResearchInstitutionBonusPerBuilding = 0.05f;

        /// <summary>Накоплено очков → списание и рост прогресса эпохи.</summary>
        public const float ResearchPointsPerEraProgressSlice = 1000f;

        public const float EraProgress01PerSlice = 0.002f;
    }
}
