namespace ColonyConquest.Agriculture
{
    /// <summary>
    /// Культуры эпохи 1 (основание) — см. <c>spec/agriculture_mining_spec.md</c> §1.1.
    /// </summary>
    public enum CropKindId : byte
    {
        None = 0,
        Wheat = 1,
        Barley = 2,
        Oat = 3,
        Rye = 4,
        Corn = 5,
        Potato = 6,
        Vegetables = 7,
        Fruits = 8,
    }
}
