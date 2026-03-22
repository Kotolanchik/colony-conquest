using ColonyConquest.Core;
using Unity.Entities;

namespace ColonyConquest.Presentation
{
    /// <summary>
    /// Базовый bridge-консьюмер: пока presentation-модуль не подключен, фиксирует объём запросов и очищает очереди.
    /// </summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(GameBootstrapSystem))]
    public partial struct PresentationBridgeDrainSystem : ISystem
    {
        const int MaxQueuedPerChannel = 512;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PresentationBridgeSingleton>();
            state.RequireForUpdate<SimulationRootState>();
        }

        public void OnUpdate(ref SystemState state)
        {
            // Если runtime resolver-сервис активен, очереди обрабатывает PresentationBridgeResolveSystem.
            if (PresentationRuntimeResolverService.HasActiveInstance)
                return;

            var tick = SystemAPI.GetSingleton<SimulationRootState>().SimulationTick;
            ref var bridge = ref SystemAPI.GetSingletonRW<PresentationBridgeState>().ValueRW;

            var unitRequests = SystemAPI.GetSingletonBuffer<UnitVisualRequestEntry>(ref state);
            var buildingRequests = SystemAPI.GetSingletonBuffer<BuildingVisualRequestEntry>(ref state);
            var iconRequests = SystemAPI.GetSingletonBuffer<UiIconRequestEntry>(ref state);
            var vfxRequests = SystemAPI.GetSingletonBuffer<VfxRequestEntry>(ref state);

            bridge.UnitRequestsTotal += (uint)unitRequests.Length;
            bridge.BuildingRequestsTotal += (uint)buildingRequests.Length;
            bridge.IconRequestsTotal += (uint)iconRequests.Length;
            bridge.VfxRequestsTotal += (uint)vfxRequests.Length;
            bridge.LastProcessedTick = tick;

            bridge.DroppedRequestsTotal += TrimOverflow(ref unitRequests);
            bridge.DroppedRequestsTotal += TrimOverflow(ref buildingRequests);
            bridge.DroppedRequestsTotal += TrimOverflow(ref iconRequests);
            bridge.DroppedRequestsTotal += TrimOverflow(ref vfxRequests);

            bridge.ActiveVfxCount = (uint)vfxRequests.Length;

            unitRequests.Clear();
            buildingRequests.Clear();
            iconRequests.Clear();
            vfxRequests.Clear();
            bridge.ActiveVfxCount = 0;
        }

        static uint TrimOverflow<T>(ref DynamicBuffer<T> buffer) where T : unmanaged, IBufferElementData
        {
            if (buffer.Length <= MaxQueuedPerChannel)
                return 0;
            var drop = (uint)(buffer.Length - MaxQueuedPerChannel);
            for (var i = 0; i < drop; i++)
                buffer.RemoveAt(0);
            return drop;
        }
    }
}
