using ColonyConquest.Core;
using ColonyConquest.Military;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace ColonyConquest.Presentation
{
    /// <summary>Публичная шина запросов к презентационному слою (prefab/icon/vfx).</summary>
    public static class PresentationBridgeBus
    {
        static uint _requestId;

        public static void PostUnitVisual(uint runtimeUnitId, MilitaryUnitType unitType, float3 worldPosition, quaternion rotation)
        {
            if (!TryGetService(out var em, out var singleton))
                return;
            var buffer = em.GetBuffer<UnitVisualRequestEntry>(singleton);
            buffer.Add(new UnitVisualRequestEntry
            {
                RequestId = ++_requestId,
                RuntimeUnitId = runtimeUnitId,
                UnitType = unitType,
                WorldPosition = worldPosition,
                Rotation = rotation
            });
        }

        public static void PostBuildingVisual(uint runtimeBuildingId, ConstructionBlueprintId blueprintId, float3 worldPosition,
            quaternion rotation)
        {
            if (!TryGetService(out var em, out var singleton))
                return;
            var buffer = em.GetBuffer<BuildingVisualRequestEntry>(singleton);
            buffer.Add(new BuildingVisualRequestEntry
            {
                RequestId = ++_requestId,
                BlueprintId = blueprintId,
                RuntimeBuildingId = runtimeBuildingId,
                WorldPosition = worldPosition,
                Rotation = rotation
            });
        }

        public static void PostIcon(PresentationIconKind kind, FixedString64Bytes iconId, float lifetimeSeconds = 1.5f, byte priority = 1)
        {
            if (!TryGetService(out var em, out var singleton))
                return;
            var buffer = em.GetBuffer<UiIconRequestEntry>(singleton);
            buffer.Add(new UiIconRequestEntry
            {
                RequestId = ++_requestId,
                Kind = kind,
                IconId = iconId,
                LifetimeSeconds = lifetimeSeconds,
                Priority = priority
            });
        }

        public static void PostIcon(PresentationIconKind kind, string iconId, float lifetimeSeconds = 1.5f, byte priority = 1)
        {
            PostIcon(kind, new FixedString64Bytes(iconId), lifetimeSeconds, priority);
        }

        public static void PostVfx(PresentationVfxKind kind, float3 worldPosition, float intensity01 = 1f, float lifetimeSeconds = 2f)
        {
            if (!TryGetService(out var em, out var singleton))
                return;
            var buffer = em.GetBuffer<VfxRequestEntry>(singleton);
            buffer.Add(new VfxRequestEntry
            {
                RequestId = ++_requestId,
                Kind = kind,
                WorldPosition = worldPosition,
                Intensity01 = math.saturate(intensity01),
                LifetimeSeconds = lifetimeSeconds
            });
        }

        static bool TryGetService(out EntityManager em, out Entity singleton)
        {
            em = default;
            singleton = Entity.Null;

            var world = World.DefaultGameObjectInjectionWorld;
            if (world == null || !world.IsCreated)
                return false;

            em = world.EntityManager;
            using var q = em.CreateEntityQuery(ComponentType.ReadOnly<PresentationBridgeSingleton>());
            if (q.CalculateEntityCount() == 0)
                return false;

            singleton = q.GetSingletonEntity();
            return true;
        }
    }
}
