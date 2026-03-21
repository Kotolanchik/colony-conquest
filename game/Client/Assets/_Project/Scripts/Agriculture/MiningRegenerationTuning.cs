namespace ColonyConquest.Agriculture
{
    /// <summary>Параметры §2.3: восстановление леса 1% в год.</summary>
    public static class MiningRegenerationTuning
    {
        public const uint DaysPerGameYear = 365;
        public const float ForestRegrowthFractionOfCapPerYear = 0.01f;
        public const float FishRegrowthFractionOfCapPerYear = 0.10f;
        public const float WildGameRegrowthFractionOfCapPerYear = 0.05f;
    }
}
