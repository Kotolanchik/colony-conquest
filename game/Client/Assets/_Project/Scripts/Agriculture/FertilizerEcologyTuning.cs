namespace ColonyConquest.Agriculture
{
    /// <summary>Ежедневный вклад в нагрузку агрохимии §1.5 (0 = нейтрально; &gt;0 — рост химической нагрузки).</summary>
    public static class FertilizerEcologyTuning
    {
        /// <summary>Годовые эффекты из спеки приведены к одному игровому дню (упрощённо).</summary>
        public static float GetDailyChemicalLoadDelta01(FertilizerKindId id)
        {
            return id switch
            {
                FertilizerKindId.None => 0f,
                FertilizerKindId.Manure => 0f,
                FertilizerKindId.Compost => -0.05f / 365f,
                FertilizerKindId.Lime => 0f,
                FertilizerKindId.Superphosphate => 0.10f / 365f,
                FertilizerKindId.NitrogenFertilizer => 0.15f / 365f,
                FertilizerKindId.Pesticides => 0.20f / 365f,
                FertilizerKindId.Herbicides => 0.15f / 365f,
                FertilizerKindId.BioFertilizer => -0.10f / 365f,
                _ => 0f
            };
        }
    }
}
