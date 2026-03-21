using ColonyConquest.Ecology;

namespace ColonyConquest.Agriculture
{
    /// <summary>
    /// Годовое изменение плодородия грядки по активному удобрению — <c>spec/ecology_spec.md</c> §2.3
    /// через <see cref="EcologySoilImpactRates"/>; варианты вне таблицы — согласованные доли.
    /// </summary>
    public static class FertilizerSoilAnnualEcology
    {
        public static float GetAnnualFertilityDelta01(FertilizerKindId f)
        {
            return f switch
            {
                FertilizerKindId.None => 0f,
                FertilizerKindId.Manure => EcologySoilImpactRates.GetAnnualFertilityDelta01(EcologySoilImpactSourceId.Manure),
                FertilizerKindId.Compost => 0.03f,
                FertilizerKindId.Lime => 0f,
                FertilizerKindId.Superphosphate => EcologySoilImpactRates.GetAnnualFertilityDelta01(EcologySoilImpactSourceId.ChemicalFertilizers),
                FertilizerKindId.NitrogenFertilizer => EcologySoilImpactRates.GetAnnualFertilityDelta01(EcologySoilImpactSourceId.ChemicalFertilizers),
                FertilizerKindId.Pesticides => EcologySoilImpactRates.GetAnnualFertilityDelta01(EcologySoilImpactSourceId.Pesticides),
                FertilizerKindId.Herbicides => -0.08f,
                FertilizerKindId.BioFertilizer => 0.04f,
                _ => 0f,
            };
        }
    }
}
