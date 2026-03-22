using Unity.Collections;
using Unity.Entities;

namespace ColonyConquest.Defense
{
    /// <summary>Маркер сущности-симуляции оборонительных сооружений.</summary>
    public struct DefensiveSimulationSingleton : IComponentData
    {
    }

    /// <summary>Глобальные агрегаты оборонительного контура.</summary>
    public struct DefensiveSimulationState : IComponentData
    {
        public uint LastProcessedDay;
        public uint LastStructureId;
        public uint StructuresBuiltTotal;
        public uint StructuresDestroyedTotal;
        public ushort EngineersAssigned;
        public float EngineerSkillLevel;
        public byte UnderFireIntensity;
        public float IncomingDamagePressure;
        public float PowerReserveKw;
    }

    /// <summary>Заказ на строительство оборонительного объекта.</summary>
    public struct DefensiveConstructionOrderEntry : IBufferElementData
    {
        public uint OrderId;
        public DefensiveStructureKindId Kind;
        public float BaseBuildHours;
        public ushort EngineersAssigned;
        public byte UnderFireIntensity;
        public float Progress01;
        public byte IsCompleted;
        public FixedString64Bytes DebugName;
    }

    /// <summary>Runtime-данные активного оборонительного сооружения.</summary>
    public struct DefensiveStructureRuntimeEntry : IBufferElementData
    {
        public uint StructureId;
        public DefensiveStructureKindId Kind;
        public float CurrentHp;
        public float MaxHp;
        public float DefenseBonusPercent;
        public float SlowEffectPercent;
        public float ContactDamage;
        public float EnergyDemandKw;
        public byte IsOperational;
        public byte IsHighTech;
    }
}
