namespace ColonyConquest.Agriculture
{
    /// <summary>Каталог культур эпохи 1; <c>spec/agriculture_mining_spec.md</c> §1.1.</summary>
    public static class CropCatalog
    {
        /// <summary>Длительность роста в тиках (кадрах) по умолчанию — настраивается для демо.</summary>
        public const uint DefaultGrowthDurationTicks = 4000;

        public static float GetBaseYieldPerTile(CropKindId kind) => Get(kind).BaseYieldPerTile;

        /// <summary>Множитель пищевой ценности относительно пшеницы (100%) — §1.1.</summary>
        public static float GetNutritionMultiplier(CropKindId kind)
        {
            var d = Get(kind);
            return d.Kind == CropKindId.None ? 1f : d.NutritionMultiplier;
        }

        public static CropDefinition Get(CropKindId kind)
        {
            switch (kind)
            {
                case CropKindId.Wheat:
                    return new CropDefinition(kind, 8f, DefaultGrowthDurationTicks, SeasonId.Summer, 1.0f);
                case CropKindId.Barley:
                    return new CropDefinition(kind, 10f, DefaultGrowthDurationTicks, SeasonId.Summer, 0.8f);
                case CropKindId.Oat:
                    return new CropDefinition(kind, 6f, DefaultGrowthDurationTicks, SeasonId.Summer, 0.7f);
                case CropKindId.Rye:
                    return new CropDefinition(kind, 7f, DefaultGrowthDurationTicks, SeasonId.Winter, 0.9f);
                case CropKindId.Corn:
                    return new CropDefinition(kind, 12f, DefaultGrowthDurationTicks, SeasonId.Summer, 0.85f);
                case CropKindId.Potato:
                    return new CropDefinition(kind, 15f, DefaultGrowthDurationTicks, SeasonId.Autumn, 1.1f);
                case CropKindId.Vegetables:
                    return new CropDefinition(kind, 5f, DefaultGrowthDurationTicks, SeasonId.Summer, 1.2f);
                case CropKindId.Fruits:
                    return new CropDefinition(kind, 4f, DefaultGrowthDurationTicks, SeasonId.Summer, 1.3f);
                default:
                    return default;
            }
        }
    }
}
