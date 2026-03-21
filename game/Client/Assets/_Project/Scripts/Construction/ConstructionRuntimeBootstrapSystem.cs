using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using ColonyConquest.Core;

namespace ColonyConquest.Construction
{
    /// <summary>Создаёт singleton и демо-очередь проектов строительства.</summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct ConstructionRuntimeBootstrapSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            if (SystemAPI.HasSingleton<ConstructionSimulationSingleton>())
                return;

            var em = state.EntityManager;
            var entity = em.CreateEntity();
            em.AddComponent<ConstructionSimulationSingleton>(entity);
            em.AddComponent(entity, new ConstructionSimulationState
            {
                LastProcessedDay = uint.MaxValue,
                LastProjectId = 2,
                ProjectsCompletedTotal = 0,
                ProjectsBlockedForResources = 0
            });

            var queue = em.AddBuffer<ConstructionProjectEntry>(entity);
            queue.Add(BuildDemoProject(1, ConstructionBlueprintId.House, ConstructionPriority.Normal,
                new FixedString64Bytes("starter-house")));
            queue.Add(BuildDemoProject(2, ConstructionBlueprintId.WorkerTenement, ConstructionPriority.High,
                new FixedString64Bytes("worker-tenement")));

            if (!SystemAPI.HasSingleton<ConstructionModeState>())
            {
                em.CreateSingleton(new ConstructionModeState
                {
                    IsActive = 0,
                    BlueprintId = (byte)ConstructionBlueprintId.None,
                    GhostWorldPosition = float3.zero
                });
            }
        }

        private static ConstructionProjectEntry BuildDemoProject(
            uint id,
            ConstructionBlueprintId blueprintId,
            ConstructionPriority priority,
            in FixedString64Bytes debugName)
        {
            ConstructionRuntimeMath.TryGetBlueprintDefaults(blueprintId, out var wood, out var stone, out var steel,
                out var workMinutes, out var zone);
            return new ConstructionProjectEntry
            {
                ProjectId = id,
                BlueprintId = blueprintId,
                ZoneKind = zone,
                Priority = priority,
                Stage = ConstructionStage.Planning,
                BaseWorkMinutes = workMinutes,
                RemainingWorkMinutes = workMinutes,
                Progress01 = 0f,
                RequiredWood = wood,
                RequiredStone = stone,
                RequiredSteel = steel,
                AssignedWorkers = 3,
                AverageBuilderSkill = 1.2f,
                ToolQuality = 1f,
                WeatherModifier = 1f,
                HasLighting = 0,
                MaterialsCommitted = 0,
                IsCompleted = 0,
                IsBlocked = 0,
                DebugName = debugName
            };
        }
    }
}
