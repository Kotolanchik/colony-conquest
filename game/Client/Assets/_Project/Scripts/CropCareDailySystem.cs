using ColonyConquest.Agriculture;
using ColonyConquest.Simulation;
using Unity.Entities;
using Unity.Mathematics;

namespace ColonyConquest.Core
{
    /// <summary>
    /// Ежедневный уход в фазе «Рост» — §1.2 (вредители, сорняки, вклад удобрений в экологию §1.5).
    /// </summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(ColonyCalendarAdvanceSystem))]
    [UpdateBefore(typeof(CropGrowthSimulationSystem))]
    public partial struct CropCareDailySystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameCalendarState>();
            state.RequireForUpdate<ColonyAgrochemicalLoadState>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var dayIndex = SystemAPI.GetSingleton<GameCalendarState>().DayIndex;
            var tick = SystemAPI.GetSingleton<SimulationRootState>().SimulationTick;
            ref var chem = ref SystemAPI.GetSingletonRW<ColonyAgrochemicalLoadState>().ValueRW;

            foreach (var plotRw in SystemAPI.Query<RefRW<CropPlotRuntime>>().WithAll<CropPlotTag>())
            {
                ref var p = ref plotRw.ValueRW;
                if (p.Phase != CropGrowthPhase.Growth)
                    continue;
                if (p.LastCareGameDayIndex == dayIndex)
                    continue;

                p.LastCareGameDayIndex = dayIndex;

                var rng = Random.CreateFromIndex(math.hash(new uint2(dayIndex, (uint)tick ^ (uint)(byte)p.Crop)));

                var delta = FertilizerEcologyTuning.GetDailyChemicalLoadDelta01(p.ActiveFertilizer);
                chem.ChemicalLoad01 = math.clamp(chem.ChemicalLoad01 + delta, 0f, 1f);

                if (p.ActiveFertilizer != FertilizerKindId.Pesticides)
                    p.PestDamage = math.min(0.5f, p.PestDamage + rng.NextFloat(0.004f, 0.018f));

                if (p.ActiveFertilizer == FertilizerKindId.Herbicides)
                    p.WeedPressure01 *= 0.1f;
                else
                    p.WeedPressure01 = math.min(0.5f, p.WeedPressure01 + rng.NextFloat(0.004f, 0.014f));
            }
        }
    }
}
