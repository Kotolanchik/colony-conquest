using Unity.Collections;
using Unity.Entities;
using ColonyConquest.Core;

namespace ColonyConquest.Construction
{
    /// <summary>Маркер сущности симуляции строительства.</summary>
    public struct ConstructionSimulationSingleton : IComponentData
    {
    }

    /// <summary>Агрегированное состояние очереди строительства.</summary>
    public struct ConstructionSimulationState : IComponentData
    {
        public uint LastProcessedDay;
        public uint LastProjectId;
        public uint ProjectsCompletedTotal;
        public uint ProjectsBlockedForResources;
    }

    public enum ConstructionPriority : byte
    {
        Low = 0,
        Normal = 1,
        High = 2,
        Critical = 3
    }

    public enum ConstructionStage : byte
    {
        Planning = 0,
        Preparation = 1,
        Foundation = 2,
        Framing = 3,
        Finishing = 4,
        Equipment = 5,
        Completed = 6
    }

    /// <summary>Запись проекта в очереди строительства.</summary>
    public struct ConstructionProjectEntry : IBufferElementData
    {
        public uint ProjectId;
        public ConstructionBlueprintId BlueprintId;
        public ConstructionZoneKindId ZoneKind;
        public ConstructionPriority Priority;
        public ConstructionStage Stage;

        public float BaseWorkMinutes;
        public float RemainingWorkMinutes;
        public float Progress01;

        public float RequiredWood;
        public float RequiredStone;
        public float RequiredSteel;

        public ushort AssignedWorkers;
        public float AverageBuilderSkill;
        public float ToolQuality;
        public float WeatherModifier;
        public byte HasLighting;

        public byte MaterialsCommitted;
        public byte IsCompleted;
        public byte IsBlocked;
        public FixedString64Bytes DebugName;
    }
}
