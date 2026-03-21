namespace ColonyConquest.Ecology
{
    /// <summary>Воздействие на почву — §2.3 <c>spec/ecology_spec.md</c>.</summary>
    public enum EcologySoilImpactSourceId : byte
    {
        None = 0,
        Manure = 1,
        ChemicalFertilizers = 2,
        Pesticides = 3,
        HeavyMetals = 4,
        OilContamination = 5,
    }
}
