namespace ColonyConquest.Agriculture
{
    /// <summary>Статические параметры культуры эпохи 1; таблица §1.1.</summary>
    public readonly struct CropDefinition
    {
        public readonly CropKindId Kind;
        /// <summary>Базовый урожай «ед/плитка» из спеки.</summary>
        public readonly float BaseYieldPerTile;
        /// <summary>Ориентир длительности роста в тиках (для связи с тюнингом; этапы — <see cref="CropGrowthTuning"/>).</summary>
        public readonly uint GrowthDurationTicks;
        public readonly SeasonId PreferredSeason;
        /// <summary>Пищевая ценность относительно пшеницы (100% = 1.0) — §1.1.</summary>
        public readonly float NutritionMultiplier;

        public CropDefinition(
            CropKindId kind,
            float baseYieldPerTile,
            uint growthDurationTicks,
            SeasonId preferredSeason,
            float nutritionMultiplier)
        {
            Kind = kind;
            BaseYieldPerTile = baseYieldPerTile;
            GrowthDurationTicks = growthDurationTicks;
            PreferredSeason = preferredSeason;
            NutritionMultiplier = nutritionMultiplier;
        }
    }
}
