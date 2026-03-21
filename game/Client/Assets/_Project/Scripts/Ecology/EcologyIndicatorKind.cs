namespace ColonyConquest.Ecology
{
    /// <summary>Индикаторы экосистемы; <c>spec/ecology_spec.md</c> §1.2.</summary>
    public enum EcologyIndicatorKind : byte
    {
        AirQuality = 0,
        WaterQuality = 1,
        SoilFertility = 2,
        ForestCover = 3,
        Biodiversity = 4
    }
}
