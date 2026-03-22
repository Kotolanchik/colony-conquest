using ColonyConquest.Analytics;
using ColonyConquest.Story;
using Unity.Collections;
using Unity.Entities;

namespace ColonyConquest.Core
{
    /// <summary>Legacy-fallback: тестовая запись/выборка очереди, когда full story runtime не активен.</summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(AiDirectorPolicyUpdateSystem))]
    public partial struct StoryEventPipelineSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            if (SystemAPI.HasSingleton<StorySimulationSingleton>())
                return;

            uint tick = SystemAPI.GetSingleton<SimulationRootState>().SimulationTick;
            ref var pipe = ref SystemAPI.GetSingletonRW<StoryEventPipelineState>().ValueRW;
            var buffer = SystemAPI.GetSingletonBuffer<GameEventQueueEntry>();

            if (pipe.BootstrapEnqueued == 0 && tick == 1)
            {
                buffer.Add(new GameEventQueueEntry
                {
                    Kind = StoryEventKind.Random,
                    EventDefinitionId = 1,
                    EnqueueSimulationTick = tick,
                    DebugLabel = new FixedString64Bytes("bootstrap")
                });
                pipe.BootstrapEnqueued = 1;
            }

            if (buffer.Length <= 0)
                return;

            var ev = buffer[0];
            buffer.RemoveAt(0);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement,
                AnalyticsMetricIds.IntegrationStoryEventBase + (uint)ev.Kind, ev.EventDefinitionId);
        }
    }
}
