namespace ColonyConquest.Politics
{
    /// <summary>Формы правления из таблицы §4.2 `spec/political_system_spec.md`.</summary>
    public enum GovernmentFormId : byte
    {
        None = 0,
        AbsoluteMonarchy = 1,
        ConstitutionalMonarchy = 2,
        Dictatorship = 3,
        Oligarchy = 4,
        Republic = 5,
        Democracy = 6,
        DirectDemocracy = 7,
    }
}
