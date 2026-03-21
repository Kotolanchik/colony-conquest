namespace ColonyConquest.Economy
{
    /// <summary>§1.1 <c>spec/economic_system_specification.md</c> — классификация ресурсов.</summary>
    public enum ResourceCategory : byte
    {
        None = 0,
        Raw = 1,
        Processed = 2,
        Material = 3,
        Component = 4,
        FinalProduct = 5,
    }

    /// <summary>Редкость на карте / в таблицах §1.2.</summary>
    public enum ResourceRarity : byte
    {
        NotApplicable = 0,
        Common = 1,
        Rare = 2,
        VeryRare = 3,
    }

    /// <summary>Игровые эпохи (§1.2): появление ресурса в дизайне.</summary>
    public enum GameEpoch : byte
    {
        None = 0,
        Epoch1_Foundation = 1,
        Epoch2_Industrialization = 2,
        Epoch3_WorldWar1 = 3,
        Epoch4_WorldWar2 = 4,
        Epoch5_Modern = 5,
    }
}
