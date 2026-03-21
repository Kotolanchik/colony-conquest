using ColonyConquest.Story;
using Unity.Collections;
using Unity.Entities;

namespace ColonyConquest.Core
{
    /// <summary>
    /// Оценка §2.3 шаг 1–2: при смене политики кладёт триггерное событие в очередь (тик &gt; 1, чтобы не мешать bootstrap-тесту).
    /// </summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(AiDirectorDimensionsUpdateSystem))]
    public partial struct AiDirectorPolicyUpdateSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<AiDirectorPolicyState>();
            state.RequireForUpdate<AiDirectorDimensionsState>();
            state.RequireForUpdate<SimulationRootState>();
        }

        public void OnUpdate(ref SystemState state)
        {
            uint tick = SystemAPI.GetSingleton<SimulationRootState>().SimulationTick;
            var ai = SystemAPI.GetSingleton<AiDirectorDimensionsState>();
            var next = EvaluatePolicy(ai);

            ref var pol = ref SystemAPI.GetSingletonRW<AiDirectorPolicyState>().ValueRW;
            if (next == pol.ActivePolicy)
                return;

            pol.ActivePolicy = next;
            pol.LastChangeTick = tick;

            if (next == AiDirectorPolicyKind.None || tick <= 1u)
                return;

            var buffer = SystemAPI.GetSingletonBuffer<GameEventQueueEntry>();
            buffer.Add(new GameEventQueueEntry
            {
                Kind = StoryEventKind.Triggered,
                EventDefinitionId = (uint)next,
                EnqueueSimulationTick = tick,
                DebugLabel = new FixedString64Bytes("director-policy")
            });
        }

        private static AiDirectorPolicyKind EvaluatePolicy(AiDirectorDimensionsState ai)
        {
            if (ai.Tension0to100 > 80f)
                return AiDirectorPolicyKind.Relief;
            if (ai.Wealth0to100 > 80f && ai.Tension0to100 < 30f)
                return AiDirectorPolicyKind.Challenge;
            if (ai.Stability0to100 < 30f)
                return AiDirectorPolicyKind.Stabilize;
            if (ai.Security0to100 < 30f)
                return AiDirectorPolicyKind.Military;
            return AiDirectorPolicyKind.None;
        }
    }
}
