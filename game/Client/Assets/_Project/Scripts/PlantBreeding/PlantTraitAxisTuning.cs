namespace ColonyConquest.PlantBreeding
{
    /// <summary>Диапазоны характеристик в процентах от базы — <c>spec/plant_breeding_spec.md</c> §1.2.</summary>
    public static class PlantTraitAxisTuning
    {
        public static void GetPercentRange(PlantTraitAxisId axis, out float minPercent, out float maxPercent)
        {
            switch (axis)
            {
                case PlantTraitAxisId.Yield:
                    minPercent = 50f;
                    maxPercent = 200f;
                    break;
                case PlantTraitAxisId.GrowthSpeed:
                    minPercent = 50f;
                    maxPercent = 150f;
                    break;
                case PlantTraitAxisId.DroughtResistance:
                case PlantTraitAxisId.ColdResistance:
                case PlantTraitAxisId.PestResistance:
                    minPercent = 0f;
                    maxPercent = 100f;
                    break;
                case PlantTraitAxisId.NutritionalValue:
                    minPercent = 50f;
                    maxPercent = 150f;
                    break;
                case PlantTraitAxisId.Taste:
                    minPercent = 50f;
                    maxPercent = 150f;
                    break;
                default:
                    minPercent = 0f;
                    maxPercent = 100f;
                    break;
            }
        }

        /// <summary>Нормализация значения в 0…1 внутри диапазона оси.</summary>
        public static float NormalizeTo01(PlantTraitAxisId axis, float percentValue)
        {
            GetPercentRange(axis, out var minP, out var maxP);
            if (maxP <= minP)
                return 0f;
            return (percentValue - minP) / (maxP - minP);
        }
    }
}
