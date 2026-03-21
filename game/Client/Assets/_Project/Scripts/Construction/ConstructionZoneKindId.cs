namespace ColonyConquest.Core
{
    /// <summary>Типы зон §3.1 <c>spec/construction_system_spec.md</c> (данные, без симуляции зонирования).</summary>
    public enum ConstructionZoneKindId : byte
    {
        None = 0,
        Residential = 1,
        Industrial = 2,
        Agricultural = 3,
        Military = 4,
        Public = 5,
    }
}
