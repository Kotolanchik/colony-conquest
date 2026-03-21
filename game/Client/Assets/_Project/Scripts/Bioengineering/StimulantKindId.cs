namespace ColonyConquest.Bioengineering
{
    /// <summary>
    /// Типы стимуляторов (стимпаки) — см. <c>spec/bioengineering_spec.md</c> §3.1.
    /// </summary>
    public enum StimulantKindId : byte
    {
        None = 0,
        Stimpack = 1,
        MedX = 2,
        Psycho = 3,
        Buffout = 4,
        RadX = 5,
        Antidote = 6,
        Nanobots = 7,
    }
}
