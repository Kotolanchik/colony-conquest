using Unity.Entities;

namespace ColonyConquest.Simulation
{
    /// <summary>Календарь колонии: день и время суток, вывод из <see cref="SimulationRootState"/>.</summary>
    public struct GameCalendarState : IComponentData
    {
        public uint DayIndex;
        public byte HourOfDay;
        public byte MinuteOfHour;
    }
}
