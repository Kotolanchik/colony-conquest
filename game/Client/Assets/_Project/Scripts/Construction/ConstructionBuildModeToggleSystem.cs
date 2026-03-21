using ColonyConquest.Analytics;
using ColonyConquest.Audio;
using Unity.Entities;

namespace ColonyConquest.Core
{
    /// <summary>
    /// По нажатию ToggleBuild (B) переключает <see cref="ConstructionGhostState.Active"/> на клиенте.
    /// </summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(InputGatherSystem))]
    public partial struct ConstructionBuildModeToggleSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<InputCommandState>();
            state.RequireForUpdate<ConstructionGhostState>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var cmd = SystemAPI.GetSingleton<InputCommandState>();
            if (cmd.ToggleBuildPressed == 0)
                return;

            ref var ghost = ref SystemAPI.GetSingletonRW<ConstructionGhostState>().ValueRW;
            if (ghost.Active == 0)
            {
                ghost.Active = 1;
                if (ghost.BlueprintId == ConstructionBlueprintId.None)
                    ghost.BlueprintId = ConstructionBlueprintId.EarthHut;
            }
            else
            {
                ghost.Active = 0;
            }

            AudioBusStub.Post(100u, AudioSfxCategory.Interface);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.IntegrationConstructionModeActive,
                ghost.Active);
        }
    }
}
