namespace ColonyConquest.Bioengineering
{
    /// <summary>Риски нейроинтерфейсов — §6.2 <c>spec/bioengineering_spec.md</c>.</summary>
    public enum NeuroInterfaceRiskKindId : byte
    {
        None = 0,
        Rejection = 1,
        BrainDamage = 2,
        Hack = 3,
        Dependency = 4,
    }
}
