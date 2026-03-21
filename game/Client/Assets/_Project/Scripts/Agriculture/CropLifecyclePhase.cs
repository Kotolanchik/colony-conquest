namespace ColonyConquest.Agriculture
{
    /// <summary>Этапы цикла выращивания — <c>spec/agriculture_mining_spec.md</c> §1.2.</summary>
    public enum CropLifecyclePhase : byte
    {
        None = 0,
        Preparation = 1,
        Sowing = 2,
        Growing = 3,
        Tending = 4,
        Harvest = 5,
        Storage = 6,
    }
}
