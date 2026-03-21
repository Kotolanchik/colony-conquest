namespace ColonyConquest.Agriculture
{
    /// <summary>
    /// Типы удобрений — <c>spec/agriculture_mining_spec.md</c> §1.5 (эффекты в данных/балансе, не в формуле урожая напрямую).
    /// </summary>
    public enum FertilizerKindId : byte
    {
        None = 0,
        Manure = 1,
        Compost = 2,
        Lime = 3,
        Superphosphate = 4,
        NitrogenFertilizer = 5,
        Pesticides = 6,
        Herbicides = 7,
        BioFertilizer = 8,
    }
}
