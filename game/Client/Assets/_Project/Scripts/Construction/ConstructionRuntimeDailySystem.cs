using ColonyConquest.Analytics;
using ColonyConquest.Core;
using ColonyConquest.Economy;
using ColonyConquest.Simulation;
using ColonyConquest.Story;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace ColonyConquest.Construction
{
    /// <summary>Суточный прогон очереди строительства: материалы, этапы, завершение и телеметрия.</summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(ColonyCalendarAdvanceSystem))]
    public partial struct ConstructionRuntimeDailySystem : ISystem
    {
        private const uint EventConstructionCompleted = 0xE301;
        private const uint EventConstructionBlocked = 0xE302;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameCalendarState>();
            state.RequireForUpdate<SimulationRootState>();
            state.RequireForUpdate<ConstructionSimulationSingleton>();
            state.RequireForUpdate<ConstructionSimulationState>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var day = SystemAPI.GetSingleton<GameCalendarState>().DayIndex;
            ref var runtime = ref SystemAPI.GetSingletonRW<ConstructionSimulationState>().ValueRW;
            if (runtime.LastProcessedDay == day)
                return;
            runtime.LastProcessedDay = day;

            var queue = SystemAPI.GetSingletonBuffer<ConstructionProjectEntry>(ref state);
            DynamicBuffer<ResourceStockEntry> stock = default;
            var hasStockpile = SystemAPI.HasSingleton<ResourceStockpileSingleton>();
            if (hasStockpile)
                stock = SystemAPI.GetSingletonBuffer<ResourceStockEntry>(ref state);

            var blockedToday = 0u;
            var completedToday = 0u;
            var backlog = 0u;
            var activeProgressSum = 0f;
            var activeCount = 0u;
            var tick = SystemAPI.GetSingleton<SimulationRootState>().SimulationTick;

            for (var i = 0; i < queue.Length; i++)
            {
                var project = queue[i];
                if (project.IsCompleted != 0)
                    continue;

                EnsureProjectDefaults(ref project);

                if (project.MaterialsCommitted == 0)
                {
                    if (!hasStockpile || !TryCommitMaterials(ref stock, in project))
                    {
                        project.IsBlocked = 1;
                        blockedToday++;
                        queue[i] = project;
                        TryEnqueueStoryEvent(ref state, tick, EventConstructionBlocked, new FixedString64Bytes("build-blocked"));
                        continue;
                    }

                    project.MaterialsCommitted = 1;
                    project.IsBlocked = 0;
                    project.Stage = ConstructionStage.Preparation;
                }

                var dailyProgress = ConstructionRuntimeMath.GetDailyProgressDelta(
                    project.BaseWorkMinutes,
                    project.AssignedWorkers,
                    project.AverageBuilderSkill,
                    project.ToolQuality,
                    project.WeatherModifier,
                    project.HasLighting != 0,
                    project.Priority);

                project.Progress01 = math.saturate(project.Progress01 + dailyProgress);
                project.RemainingWorkMinutes = math.max(0f, project.BaseWorkMinutes * (1f - project.Progress01));
                project.Stage = ConstructionRuntimeMath.ResolveStage(project.Progress01);

                if (project.Progress01 >= 0.999f)
                {
                    project.Progress01 = 1f;
                    project.RemainingWorkMinutes = 0f;
                    project.IsCompleted = 1;
                    project.IsBlocked = 0;
                    project.Stage = ConstructionStage.Completed;
                    runtime.ProjectsCompletedTotal++;
                    completedToday++;
                    TryEnqueueStoryEvent(ref state, tick, EventConstructionCompleted, new FixedString64Bytes("build-complete"));
                }
                else
                {
                    backlog++;
                    activeCount++;
                    activeProgressSum += project.Progress01;
                }

                queue[i] = project;
            }

            runtime.ProjectsBlockedForResources += blockedToday;

            var averageProgress = activeCount == 0u ? 1f : activeProgressSum / activeCount;
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.ConstructionProjectsBacklog, backlog);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.ConstructionProjectsCompletedTotal,
                runtime.ProjectsCompletedTotal);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.ConstructionProjectsBlockedTotal,
                runtime.ProjectsBlockedForResources);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.ConstructionAverageProgress01,
                averageProgress);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.ConstructionCompletionsToday,
                completedToday);
        }

        private static void EnsureProjectDefaults(ref ConstructionProjectEntry project)
        {
            if (project.BaseWorkMinutes > 0f)
                return;
            if (!ConstructionRuntimeMath.TryGetBlueprintDefaults(project.BlueprintId, out var wood, out var stone, out var steel,
                    out var workMinutes, out var zone))
                return;

            project.RequiredWood = wood;
            project.RequiredStone = stone;
            project.RequiredSteel = steel;
            project.BaseWorkMinutes = workMinutes;
            project.RemainingWorkMinutes = workMinutes;
            project.ZoneKind = zone;
            if (project.AssignedWorkers == 0)
                project.AssignedWorkers = 2;
            if (project.AverageBuilderSkill <= 0f)
                project.AverageBuilderSkill = 1f;
            if (project.ToolQuality <= 0f)
                project.ToolQuality = 1f;
            if (project.WeatherModifier <= 0f)
                project.WeatherModifier = 1f;
            if (project.Priority > ConstructionPriority.Critical)
                project.Priority = ConstructionPriority.Normal;
            project.Stage = ConstructionStage.Planning;
        }

        private static bool TryCommitMaterials(ref DynamicBuffer<ResourceStockEntry> stock, in ConstructionProjectEntry project)
        {
            if (!ResourceStockpileOps.HasAtLeast(ref stock, ResourceId.Wood, project.RequiredWood))
                return false;
            if (!ResourceStockpileOps.HasAtLeast(ref stock, ResourceId.Stone, project.RequiredStone))
                return false;
            if (!ResourceStockpileOps.HasAtLeast(ref stock, ResourceId.SteelIndustrial, project.RequiredSteel))
                return false;

            if (!ResourceStockpileOps.TryConsume(ref stock, ResourceId.Wood, project.RequiredWood))
                return false;
            if (!ResourceStockpileOps.TryConsume(ref stock, ResourceId.Stone, project.RequiredStone))
                return false;
            if (!ResourceStockpileOps.TryConsume(ref stock, ResourceId.SteelIndustrial, project.RequiredSteel))
                return false;

            return true;
        }

        private static void TryEnqueueStoryEvent(ref SystemState state, uint tick, uint eventDefinitionId,
            in FixedString64Bytes label)
        {
            if (!SystemAPI.HasSingleton<StoryEventQueueSingleton>())
                return;
            var queue = SystemAPI.GetSingletonBuffer<GameEventQueueEntry>(ref state);
            queue.Add(new GameEventQueueEntry
            {
                Kind = StoryEventKind.Triggered,
                EventDefinitionId = eventDefinitionId,
                EnqueueSimulationTick = tick,
                DebugLabel = label
            });
        }
    }
}
