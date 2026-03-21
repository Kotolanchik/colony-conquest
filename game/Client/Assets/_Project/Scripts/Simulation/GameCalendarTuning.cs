namespace ColonyConquest.Simulation
{
    /// <summary>Связь тика симуляции с игровыми минутами; настройка прототипа.</summary>
    public static class GameCalendarTuning
    {
        /// <summary>Сколько тиков <c>SimulationRootState.SimulationTick</c> соответствует одной игровой минуте.</summary>
        public const uint SimulationTicksPerGameMinute = 30;

        /// <summary>Дней в игровом году (календарь <see cref="GameCalendarState.DayIndex"/>); годовые эффекты экологии §2.3.</summary>
        public const uint DaysPerGameYear = 365;
    }
}
