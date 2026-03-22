using ColonyConquest.Ecology;
using Unity.Entities;

namespace ColonyConquest.Core
{
    /// <summary>Пересчёт <see cref="ColonyPollutionSummaryState"/> из <see cref="ColonyEcologyIndicatorsState"/>.</summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(AgrochemicalEcologyBridgeSystem))]
    [UpdateAfter(typeof(EcologySimulationDailySystem))]
    public partial struct EcologyPollutionSummarySystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ColonyEcologyIndicatorsState>();
            state.RequireForUpdate<ColonyPollutionSummaryState>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var eco = SystemAPI.GetSingleton<ColonyEcologyIndicatorsState>();
            ref var sum = ref SystemAPI.GetSingletonRW<ColonyPollutionSummaryState>().ValueRW;
            sum.CombinedPollutionPercent0to100 = EcologyPollutionMath.GetCombinedPollutionPercent0to100(eco);
            sum.Band = EcologyPollutionMath.GetPollutionLevelBand(sum.CombinedPollutionPercent0to100);
        }
    }
}
