using ColonyConquest.Simulation;
using Unity.Entities;

namespace ColonyConquest.Core
{
    /// <summary>Обновляет <see cref="GameCalendarState"/> из <see cref="SimulationRootState"/>.</summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(GameBootstrapSystem))]
    public partial struct ColonyCalendarAdvanceSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            uint tick = SystemAPI.GetSingleton<SimulationRootState>().SimulationTick;
            uint tpm = GameCalendarTuning.SimulationTicksPerGameMinute;
            if (tpm == 0)
                return;

            ulong minutesTotal = tick / (ulong)tpm;
            const ulong MinutesPerDay = 24 * 60;
            ulong minOfDay = minutesTotal % MinutesPerDay;

            ref var cal = ref SystemAPI.GetSingletonRW<GameCalendarState>().ValueRW;
            cal.DayIndex = (uint)(minutesTotal / MinutesPerDay);
            cal.HourOfDay = (byte)(minOfDay / 60);
            cal.MinuteOfHour = (byte)(minOfDay % 60);
        }
    }
}
