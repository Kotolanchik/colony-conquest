namespace ColonyConquest.Agriculture
{
    /// <summary>Водный множитель урожая §1.3.</summary>
    public static class CropWaterTuning
    {
        public static float GetYieldMultiplier(WaterSupplyKind supply)
        {
            return supply switch
            {
                WaterSupplyKind.None => 0.5f,
                WaterSupplyKind.Normal => 1f,
                WaterSupplyKind.Irrigated => 1.2f,
                _ => 1f
            };
        }
    }
}
