using ColonyConquest.Simulation;

namespace ColonyConquest.Agriculture
{
    /// <summary>Длительности этапов §1.2 в тиках симуляции (связь с <see cref="GameCalendarTuning"/>).</summary>
    public static class CropGrowthTuning
    {
        static ulong TicksPerGameMinute => GameCalendarTuning.SimulationTicksPerGameMinute;

        static ulong Minutes(ulong m) => m * TicksPerGameMinute;

        /// <summary>Подготовка — 1 день.</summary>
        public static ulong TicksPreparation => Minutes(24 * 60);

        /// <summary>Посев — 1 час.</summary>
        public static ulong TicksSowing => Minutes(60);

        /// <summary>Рост — 10 дней (из диапазона 5–30 §1.2).</summary>
        public static ulong TicksGrowth => Minutes(10 * 24 * 60);

        /// <summary>Урожай — 1 день.</summary>
        public static ulong TicksHarvest => Minutes(24 * 60);

        public static ulong TicksForPhase(CropGrowthPhase phase)
        {
            switch (phase)
            {
                case CropGrowthPhase.Preparation: return TicksPreparation;
                case CropGrowthPhase.Sowing: return TicksSowing;
                case CropGrowthPhase.Growth: return TicksGrowth;
                case CropGrowthPhase.Harvest: return TicksHarvest;
                default: return 0;
            }
        }
    }
}
