namespace ColonyConquest.Bioengineering
{
    /// <summary>Применения генной терапии — таблица §4.1 <c>spec/bioengineering_spec.md</c>.</summary>
    public enum GeneTherapyApplicationKindId : byte
    {
        None = 0,
        HereditaryDiseaseTreatment = 1,
        LifespanExtension = 2,
        StatImprovement = 3,
        OrganRegeneration = 4,
    }
}
