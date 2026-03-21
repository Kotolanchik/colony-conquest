namespace ColonyConquest.Agriculture
{
    /// <summary>
    /// Уровни кирки для ручной добычи — см. <c>spec/agriculture_mining_spec.md</c> §2.2 (ед/час и износ).
    /// </summary>
    public enum MiningPickaxeTierId : byte
    {
        None = 0,
        Stone = 1,
        Copper = 2,
        Bronze = 3,
        Iron = 4,
        Steel = 5,
    }
}
