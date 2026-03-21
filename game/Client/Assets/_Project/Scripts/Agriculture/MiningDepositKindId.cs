namespace ColonyConquest.Agriculture
{
    /// <summary>
    /// Типы месторождений — см. <c>spec/agriculture_mining_spec.md</c> §2.1 (сжатый перечень для данных/карты).
    /// </summary>
    public enum MiningDepositKindId : byte
    {
        None = 0,
        Forest = 1,
        StoneQuarry = 2,
        Clay = 3,
        Sand = 4,
        IronOre = 5,
        Coal = 6,
        CopperOre = 7,
        TinOre = 8,
        LeadOre = 9,
        SilverOre = 10,
        GoldOre = 11,
        Oil = 12,
        Bauxite = 13,
        Nickel = 14,
        Chromium = 15,
        Tungsten = 16,
        Uranium = 17,
        Platinum = 18,
        RareEarths = 19,
        Diamonds = 20,
        Helium3 = 21,
        AntimatterLab = 22,
    }
}
