namespace ColonyConquest.Analytics
{
    /// <summary>Категории достижений — <c>spec/statistics_analytics_spec.md</c> §8.1.</summary>
    public enum AchievementCategory : byte
    {
        None = 0,
        Population = 1,
        Economy = 2,
        Military = 3,
        Technology = 4,
        Social = 5,
        Special = 6,
    }
}
