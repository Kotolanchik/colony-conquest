namespace ColonyConquest.Bioengineering
{
    /// <summary>Риски генной терапии — таблица §4.2 <c>spec/bioengineering_spec.md</c>.</summary>
    public enum GeneTherapyRiskKindId : byte
    {
        None = 0,
        Rejection = 1,
        Mutation = 2,
        Failure = 3,
        Cancer = 4,
    }
}
