using ColonyConquest.Analytics;
using ColonyConquest.Audio;
using ColonyConquest.Presentation;
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
            PresentationBridgeBus.PostIcon(PresentationIconKind.Notification, ghost.Active != 0 ? "build-mode-on" : "build-mode-off",
                1.25f, 2);
            PresentationBridgeBus.PostVfx(ghost.Active != 0 ? PresentationVfxKind.ConstructionStart : PresentationVfxKind.NotificationPing,
                ghost.AnchorWorld, 0.7f, 1.1f);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.IntegrationConstructionModeActive,
                ghost.Active);
        }
    }
}
