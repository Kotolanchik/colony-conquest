namespace ColonyConquest.Agriculture
{
    /// <summary>
    /// Промышленные методы добычи — см. <c>spec/agriculture_mining_spec.md</c> §2.2 (таблица «Промышленная добыча»).
    /// </summary>
    public enum IndustrialMiningMethodId : byte
    {
        None = 0,
        Mine = 1,
        OpenQuarry = 2,
        Adit = 3,
        GoldDredge = 4,
        OilPumpjack = 5,
        OpenPit = 6,
        DeepMine = 7,
        AutomatedMine = 8,
        AsteroidMining = 9,
    }
}
