namespace ColonyConquest.Defense
{
    /// <summary>Типы оборонительных сооружений — <c>spec/defensive_structures_spec.md</c> §1.2.</summary>
    public enum DefensiveStructureKindId : byte
    {
        None = 0,

        // Полевые
        Trenches = 1,
        SandbagWall = 2,
        BarbedWire = 3,
        AntiTankHedgehogs = 4,
        Minefield = 5,

        // Укреплённые
        Pillbox = 10,
        Bunker = 11,
        ConcreteRedoubt = 12,
        FortifiedPosition = 13,

        // Эпоха 5+
        EnergyShield = 20,
        ForceField = 21,
        AutomatedTurret = 22
    }
}
