namespace ColonyConquest.Ecology
{
    /// <summary>
    /// Годовые изменения плодородия почвы по типу воздействия — §2.3 <c>spec/ecology_spec.md</c>
    /// (доля 0–1; отрицательные — деградация за игровой год).
    /// </summary>
    public static class EcologySoilImpactRates
    {
        public static float GetAnnualFertilityDelta01(EcologySoilImpactSourceId id)
        {
            return id switch
            {
                EcologySoilImpactSourceId.Manure => 0.05f,
                EcologySoilImpactSourceId.ChemicalFertilizers => -0.10f,
                EcologySoilImpactSourceId.Pesticides => -0.15f,
                EcologySoilImpactSourceId.HeavyMetals => -0.30f,
                EcologySoilImpactSourceId.OilContamination => -0.50f,
                _ => 0f,
            };
        }
    }
}
