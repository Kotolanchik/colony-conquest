using ColonyConquest.Analytics;
using ColonyConquest.Audio;
using ColonyConquest.Construction;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ColonyConquest.Core
{
    /// <summary>Переключение режима призрака по клавише B.</summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(InputGatherSystem))]
    public partial struct ConstructionToggleSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            if (SystemAPI.GetSingleton<InputCommandState>().ToggleBuildPressed == 0)
                return;

            ref var mode = ref SystemAPI.GetSingletonRW<ConstructionModeState>().ValueRW;
            mode.IsActive = (byte)(mode.IsActive == 0 ? 1 : 0);
            if (mode.IsActive != 0 && mode.BlueprintId == 0)
                mode.BlueprintId = 1;

                if (SystemAPI.TryGetSingletonRW<ConstructionGhostState>(out var ghostRw))
                {
                    ref var ghost = ref ghostRw.ValueRW;
                    ghost.Active = mode.IsActive;
                    ghost.BlueprintId = mode.BlueprintId != 0
                        ? (ConstructionBlueprintId)mode.BlueprintId
                        : ConstructionBlueprintId.None;
                    ghost.FootprintCells = ConstructionBlueprintFootprints.GetFootprintCells(ghost.BlueprintId);
                    ghost.AnchorWorld = mode.GhostWorldPosition;
                }

            AudioBusStub.Post(100u, AudioSfxCategory.Interface);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, 2u, mode.IsActive);
        }
    }

    /// <summary>Позиция призрака: смещение от игрока по направлению ввода.</summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(PlayerMoveFromInputSystem))]
    public partial struct ConstructionGhostCursorSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            ref var mode = ref SystemAPI.GetSingletonRW<ConstructionModeState>().ValueRW;
            if (mode.IsActive == 0)
                return;

            float2 move = SystemAPI.GetSingleton<InputCommandState>().Move;
            float3 dir = math.normalizesafe(new float3(move.x, 0f, move.y));
            if (math.lengthsq(dir) < 1e-6f)
                dir = new float3(0f, 0f, 1f);

            foreach (var lt in SystemAPI.Query<RefRO<LocalTransform>>().WithAll<PlayerMoveTargetTag>())
            {
                mode.GhostWorldPosition = lt.ValueRO.Position + dir * 3f;
                if (SystemAPI.TryGetSingletonRW<ConstructionGhostState>(out var ghostRw))
                {
                    ref var ghost = ref ghostRw.ValueRW;
                    ghost.FootprintCells = ConstructionBlueprintFootprints.GetFootprintCells(ghost.BlueprintId);
                    ghost.AnchorWorld = mode.GhostWorldPosition;
                    ghost.PlacementValid = 1;
                }

                return;
            }
        }
    }
}
