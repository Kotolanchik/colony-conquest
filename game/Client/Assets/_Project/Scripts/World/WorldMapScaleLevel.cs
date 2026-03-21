namespace ColonyConquest.WorldMap
{
    /// <summary>Уровни масштаба карты — <c>spec/global_map_spec.md</c> §1.2.</summary>
    public enum WorldMapScaleLevel : byte
    {
        None = 0,
        Global = 1,
        Regional = 2,
        Local = 3,
        Tactical = 4
    }
}
