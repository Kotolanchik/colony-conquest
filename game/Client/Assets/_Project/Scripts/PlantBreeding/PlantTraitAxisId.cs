namespace ColonyConquest.PlantBreeding
{
    /// <summary>
    /// Оси характеристик растений для селекции — <c>spec/plant_breeding_spec.md</c> §1.2.
    /// </summary>
    public enum PlantTraitAxisId : byte
    {
        None = 0,
        Yield = 1,
        GrowthSpeed = 2,
        DroughtResistance = 3,
        ColdResistance = 4,
        PestResistance = 5,
        NutritionalValue = 6,
        Taste = 7,
    }
}
