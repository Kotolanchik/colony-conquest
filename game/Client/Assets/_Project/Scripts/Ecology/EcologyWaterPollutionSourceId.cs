namespace ColonyConquest.Ecology
{
    /// <summary>Источники загрязнения воды — §2.2 <c>spec/ecology_spec.md</c>.</summary>
    public enum EcologyWaterPollutionSourceId : byte
    {
        None = 0,
        UntreatedSewage = 1,
        MineDrainage = 2,
        IndustrialEffluent = 3,
        OilSpill = 4,
        RadioactiveEffluent = 5,
    }
}
