namespace ColonyConquest.Agriculture
{
    /// <summary>90 игровых дней на сезон, 360 дней в цикле (упрощение для прототипа).</summary>
    public static class SeasonCycle
    {
        public const uint DaysPerSeason = 90;

        public static SeasonId GetSeasonFromDayIndex(uint dayIndex)
        {
            uint d = dayIndex % (DaysPerSeason * 4);
            if (d < DaysPerSeason)
                return SeasonId.Spring;
            if (d < DaysPerSeason * 2)
                return SeasonId.Summer;
            if (d < DaysPerSeason * 3)
                return SeasonId.Autumn;
            return SeasonId.Winter;
        }
    }
}
