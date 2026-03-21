namespace ColonyConquest.Agriculture
{
    /// <summary>
    /// Виды животноводства — см. <c>spec/agriculture_mining_spec.md</c> §1.4 (эпоха в комментарии к значению).
    /// </summary>
    public enum LivestockKindId : byte
    {
        None = 0,
        Chickens = 1,
        Goats = 2,
        Sheep = 3,
        Pigs = 4,
        /// <summary>Эпоха 2.</summary>
        Cows = 5,
        /// <summary>Эпоха 2.</summary>
        Horses = 6,
        /// <summary>Эпоха 2 (пустыня).</summary>
        Camels = 7,
        /// <summary>Эпоха 4, фабричное скотоводство.</summary>
        FactoryLivestock = 8,
    }
}
