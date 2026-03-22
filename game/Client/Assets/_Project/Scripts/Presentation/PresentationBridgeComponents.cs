using ColonyConquest.Core;
using ColonyConquest.Military;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace ColonyConquest.Presentation
{
    public enum PresentationIconKind : byte
    {
        Generic = 0,
        Unit = 1,
        Building = 2,
        Resource = 3,
        Notification = 4,
        Ability = 5
    }

    public enum PresentationVfxKind : byte
    {
        None = 0,
        UnitSpawn = 1,
        UnitDeath = 2,
        Hit = 3,
        Explosion = 4,
        ConstructionStart = 5,
        ConstructionComplete = 6,
        Repair = 7,
        NotificationPing = 8,
        WeatherRain = 9,
        WeatherSnow = 10
    }

    /// <summary>Маркер singleton презентационного bridge-слоя.</summary>
    public struct PresentationBridgeSingleton : IComponentData
    {
    }

    /// <summary>Сводная статистика обработки визуальных запросов.</summary>
    public struct PresentationBridgeState : IComponentData
    {
        public uint LastProcessedTick;
        public uint UnitRequestsTotal;
        public uint BuildingRequestsTotal;
        public uint IconRequestsTotal;
        public uint VfxRequestsTotal;
        public uint DroppedRequestsTotal;
        public uint ActiveVfxCount;
    }

    /// <summary>Запрос на визуализацию юнита/замену визуала.</summary>
    public struct UnitVisualRequestEntry : IBufferElementData
    {
        public uint RequestId;
        public uint RuntimeUnitId;
        public MilitaryUnitType UnitType;
        public float3 WorldPosition;
        public quaternion Rotation;
    }

    /// <summary>Запрос на визуализацию здания/чертежа.</summary>
    public struct BuildingVisualRequestEntry : IBufferElementData
    {
        public uint RequestId;
        public ConstructionBlueprintId BlueprintId;
        public uint RuntimeBuildingId;
        public float3 WorldPosition;
        public quaternion Rotation;
    }

    /// <summary>Запрос на вывод иконки в UI.</summary>
    public struct UiIconRequestEntry : IBufferElementData
    {
        public uint RequestId;
        public PresentationIconKind Kind;
        public FixedString64Bytes IconId;
        public float LifetimeSeconds;
        public byte Priority;
    }

    /// <summary>Запрос на спавн VFX.</summary>
    public struct VfxRequestEntry : IBufferElementData
    {
        public uint RequestId;
        public PresentationVfxKind Kind;
        public float3 WorldPosition;
        public float Intensity01;
        public float LifetimeSeconds;
    }
}
