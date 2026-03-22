using ColonyConquest.Core;
using Unity.Entities;

namespace ColonyConquest.Presentation
{
    /// <summary>Обрабатывает bridge-запросы через runtime resolver service (prefab/icon/vfx).</summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(GameBootstrapSystem))]
    public partial struct PresentationBridgeResolveSystem : ISystem
    {
        const int MaxRequestsPerChannelPerFrame = 512;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PresentationBridgeSingleton>();
            state.RequireForUpdate<SimulationRootState>();
        }

        public void OnUpdate(ref SystemState state)
        {
            if (!PresentationRuntimeResolverService.HasActiveInstance)
                return;

            var service = PresentationRuntimeResolverService.Instance;
            if (service == null)
                return;

            ref var bridge = ref SystemAPI.GetSingletonRW<PresentationBridgeState>().ValueRW;
            var tick = SystemAPI.GetSingleton<SimulationRootState>().SimulationTick;

            var unitRequests = SystemAPI.GetSingletonBuffer<UnitVisualRequestEntry>(ref state);
            var buildingRequests = SystemAPI.GetSingletonBuffer<BuildingVisualRequestEntry>(ref state);
            var iconRequests = SystemAPI.GetSingletonBuffer<UiIconRequestEntry>(ref state);
            var vfxRequests = SystemAPI.GetSingletonBuffer<VfxRequestEntry>(ref state);

            bridge.UnitRequestsTotal += (uint)unitRequests.Length;
            bridge.BuildingRequestsTotal += (uint)buildingRequests.Length;
            bridge.IconRequestsTotal += (uint)iconRequests.Length;
            bridge.VfxRequestsTotal += (uint)vfxRequests.Length;
            bridge.LastProcessedTick = tick;

            bridge.DroppedRequestsTotal += ProcessUnits(service, ref unitRequests);
            bridge.DroppedRequestsTotal += ProcessBuildings(service, ref buildingRequests);
            bridge.DroppedRequestsTotal += ProcessIcons(service, ref iconRequests);
            bridge.DroppedRequestsTotal += ProcessVfx(service, ref vfxRequests);

            bridge.ActiveVfxCount = (uint)service.ActiveVfxCount;
        }

        static uint ProcessUnits(PresentationRuntimeResolverService service, ref DynamicBuffer<UnitVisualRequestEntry> requests)
        {
            var drop = 0u;
            var count = requests.Length > MaxRequestsPerChannelPerFrame ? MaxRequestsPerChannelPerFrame : requests.Length;
            for (var i = 0; i < count; i++)
            {
                var request = requests[i];
                if (!service.UpsertUnitVisual(request.RuntimeUnitId, request.UnitType, request.WorldPosition, request.Rotation))
                    drop++;
            }

            if (requests.Length > count)
                drop += (uint)(requests.Length - count);

            requests.Clear();
            return drop;
        }

        static uint ProcessBuildings(PresentationRuntimeResolverService service,
            ref DynamicBuffer<BuildingVisualRequestEntry> requests)
        {
            var drop = 0u;
            var count = requests.Length > MaxRequestsPerChannelPerFrame ? MaxRequestsPerChannelPerFrame : requests.Length;
            for (var i = 0; i < count; i++)
            {
                var request = requests[i];
                if (!service.UpsertBuildingVisual(request.RuntimeBuildingId, request.BlueprintId, request.WorldPosition, request.Rotation))
                    drop++;
            }

            if (requests.Length > count)
                drop += (uint)(requests.Length - count);

            requests.Clear();
            return drop;
        }

        static uint ProcessIcons(PresentationRuntimeResolverService service, ref DynamicBuffer<UiIconRequestEntry> requests)
        {
            var drop = 0u;
            var count = requests.Length > MaxRequestsPerChannelPerFrame ? MaxRequestsPerChannelPerFrame : requests.Length;
            for (var i = 0; i < count; i++)
            {
                if (!service.PushIcon(requests[i]))
                    drop++;
            }

            if (requests.Length > count)
                drop += (uint)(requests.Length - count);

            requests.Clear();
            return drop;
        }

        static uint ProcessVfx(PresentationRuntimeResolverService service, ref DynamicBuffer<VfxRequestEntry> requests)
        {
            var drop = 0u;
            var count = requests.Length > MaxRequestsPerChannelPerFrame ? MaxRequestsPerChannelPerFrame : requests.Length;
            for (var i = 0; i < count; i++)
            {
                var request = requests[i];
                if (!service.PlayVfx(request.Kind, request.WorldPosition, request.Intensity01, request.LifetimeSeconds))
                    drop++;
            }

            if (requests.Length > count)
                drop += (uint)(requests.Length - count);

            requests.Clear();
            return drop;
        }
    }
}
