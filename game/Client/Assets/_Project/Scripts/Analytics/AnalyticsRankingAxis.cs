namespace ColonyConquest.Analytics
{
    /// <summary>Оси глобального рейтинга фракций — <c>spec/statistics_analytics_spec.md</c> §7.1.</summary>
    public enum AnalyticsRankingAxis : byte
    {
        None = 0,
        ByPopulation = 1,
        ByGdp = 2,
        ByMilitaryPower = 3,
        ByTechnology = 4,
    }
}
