namespace ColonyConquest.Agriculture
{
    /// <summary>Бонус урожайности (1 + value) из §1.5; 0.2 = +20%.</summary>
    public static class FertilizerTuning
    {
        public static float GetYieldBonus(FertilizerKindId id)
        {
            switch (id)
            {
                case FertilizerKindId.Manure: return 0.20f;
                case FertilizerKindId.Compost: return 0.15f;
                case FertilizerKindId.Lime: return 0.10f;
                case FertilizerKindId.Superphosphate: return 0.50f;
                case FertilizerKindId.NitrogenFertilizer: return 0.60f;
                case FertilizerKindId.Pesticides: return 0f;
                case FertilizerKindId.Herbicides: return 0f;
                case FertilizerKindId.BioFertilizer: return 0.40f;
                default: return 0f;
            }
        }
    }
}
