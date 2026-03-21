using ColonyConquest.Agriculture;
using ColonyConquest.Economy;
using ColonyConquest.Simulation;
using Unity.Entities;

namespace ColonyConquest.Core
{
    /// <summary>Суточная продукция животноводства §1.4 <c>spec/agriculture_mining_spec.md</c>.</summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(ColonyCalendarAdvanceSystem))]
    public partial struct LivestockDailyProductionSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var day = SystemAPI.GetSingleton<GameCalendarState>().DayIndex;
            var stockBuf = SystemAPI.GetSingletonBuffer<ResourceStockEntry>();

            foreach (var penRw in SystemAPI.Query<RefRW<LivestockPenRuntime>>().WithAll<LivestockPenTag>())
            {
                ref var pen = ref penRw.ValueRW;
                if (pen.LastYieldDayIndex == day)
                    continue;

                if (LivestockDailyYield.TryGetPrimaryResource(pen.Kind, out var res, out var amt))
                    ResourceStockpileOps.Add(ref stockBuf, res, amt);

                pen.LastYieldDayIndex = day;
            }
        }
    }
}
