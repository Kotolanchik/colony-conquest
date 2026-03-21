namespace ColonyConquest.Bioengineering
{
    /// <summary>Типы клонирования — §5.1 <c>spec/bioengineering_spec.md</c>.</summary>
    public enum CloningProcedureKindId : byte
    {
        None = 0,
        TherapeuticOrganGrowth = 1,
        ReproductiveAnimalCloning = 2,
        RegenerativeLimbCloning = 3,
    }
}
