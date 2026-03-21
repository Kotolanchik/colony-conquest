namespace ColonyConquest.Agriculture
{
    /// <summary>Бонус к урожайности от удобрения (доля для <see cref="CropYieldFormulas.ComputeYield"/> §1.3) — §1.5.</summary>
    public static class FertilizerYieldBonuses
    {
        public static float GetYieldBonus(FertilizerKindId kind)
        {
            return kind switch
            {
                FertilizerKindId.Manure => 0.2f,
                FertilizerKindId.Compost => 0.15f,
                FertilizerKindId.Lime => 0.1f,
                FertilizerKindId.Superphosphate => 0.5f,
                FertilizerKindId.NitrogenFertilizer => 0.6f,
                FertilizerKindId.Pesticides => 0f,
                FertilizerKindId.Herbicides => 0f,
                FertilizerKindId.BioFertilizer => 0.4f,
                _ => 0f
            };
        }
    }
}
