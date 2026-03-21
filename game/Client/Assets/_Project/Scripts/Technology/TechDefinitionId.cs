namespace ColonyConquest.Technology
{
    /// <summary>Идентификаторы технологий (репрезентативный каталог по эпохам из <c>technology_tree_spec.md</c>).</summary>
    public enum TechDefinitionId : ushort
    {
        None = 0,

        // Era 1
        ImprovedSmelting = 101,
        BrickProduction = 102,
        Steelworking = 103,
        Gunpowder = 104,
        PrintingPress = 105,

        // Era 2
        SteamEngine = 201,
        BlastFurnace = 202,
        Railway = 203,
        OilExtraction = 204,
        MassProduction = 205,

        // Era 3
        Electricity = 301,
        InternalCombustion = 302,
        TankMk1 = 303,
        Aviation = 304,
        Radio = 305,

        // Era 4
        Radar = 401,
        JetEngine = 402,
        NuclearPhysics = 403,
        Computer = 404,
        MediumTank = 405,

        // Era 5
        NuclearReactor = 501,
        Internet = 502,
        CompositeMaterials = 503,
        QuantumComputer = 504,
        WeakAi = 505,
    }
}
