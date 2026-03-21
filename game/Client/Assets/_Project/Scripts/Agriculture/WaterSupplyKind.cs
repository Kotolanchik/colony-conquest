namespace ColonyConquest.Agriculture
{
    /// <summary>Источник воды для грядки; множители §1.3.</summary>
    public enum WaterSupplyKind : byte
    {
        /// <summary>Нет полива — −50% к урожаю (множитель 0.5).</summary>
        None = 0,
        /// <summary>Обычный полив — 1.0.</summary>
        Normal = 1,
        /// <summary>Автополив — +20% (множитель 1.2).</summary>
        Irrigated = 2
    }
}
