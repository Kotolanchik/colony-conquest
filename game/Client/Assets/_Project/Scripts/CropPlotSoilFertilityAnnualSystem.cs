using ColonyConquest.Agriculture;
using ColonyConquest.Simulation;
using Unity.Entities;
using Unity.Mathematics;

namespace ColonyConquest.Core
{
    /// <summary>
    /// Раз в игровой год сдвигает <see cref="CropPlotRuntime.SoilFertility"/> по §2.3
    /// (<see cref="FertilizerSoilAnnualEcology"/> / <see cref="EcologySoilImpactRates"/>).
    /// </summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(ColonyCalendarAdvanceSystem))]
    [UpdateBefore(typeof(CropCareDailySystem))]
    public partial struct CropPlotSoilFertilityAnnualSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameCalendarState>();
        }

        public void OnUpdate(ref SystemState state)
        {
            uint day = SystemAPI.GetSingleton<GameCalendarState>().DayIndex;
            if (day == 0)
                return;

            uint y = day / GameCalendarTuning.DaysPerGameYear;
            if (y == 0)
                return;

            foreach (var plotRw in SystemAPI.Query<RefRW<CropPlotRuntime>>().WithAll<CropPlotTag>())
            {
                ref var p = ref plotRw.ValueRW;
                if (y <= p.LastSoilAnnualGameYearIndex)
                    continue;

                p.LastSoilAnnualGameYearIndex = y;
                float d = FertilizerSoilAnnualEcology.GetAnnualFertilityDelta01(p.ActiveFertilizer);
                p.SoilFertility = math.clamp(p.SoilFertility + d, 0.5f, 2f);
            }
        }
    }
}
