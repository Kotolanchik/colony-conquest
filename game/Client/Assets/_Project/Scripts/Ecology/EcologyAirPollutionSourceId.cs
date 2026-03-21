namespace ColonyConquest.Ecology
{
    /// <summary>Источники загрязнения воздуха — §2.1 <c>spec/ecology_spec.md</c>.</summary>
    public enum EcologyAirPollutionSourceId : byte
    {
        None = 0,
        CampfireOrFurnace = 1,
        BlastFurnace = 2,
        SteamEngine = 3,
        CoalFiredPowerPlant = 4,
        OilRefinery = 5,
        ChemicalPlant = 6,
        NuclearPowerPlant = 7,
        ThermonuclearReactor = 8,
    }
}
