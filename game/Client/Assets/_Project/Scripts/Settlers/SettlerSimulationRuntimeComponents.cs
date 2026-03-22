using Unity.Collections;
using Unity.Entities;

namespace ColonyConquest.Settlers
{
    /// <summary>Маркер сущности runtime-симуляции поселенцев.</summary>
    public struct SettlerSimulationSingleton : IComponentData
    {
    }

    /// <summary>Сводное состояние контура симуляции поселенцев.</summary>
    public struct SettlerSimulationState : IComponentData
    {
        public uint LastProcessedDay;
        public uint LastSettlerId;
        public uint PopulationAlive;
        public uint BirthsTotal;
        public uint DeathsTotal;
        public uint MentalBreaksTotal;
        public uint InteractionEventsTotal;
        public float AverageMood;
        public float AverageStress;
        public float AverageHealth01;
        public float AverageWorkEfficiency01;
        public float ColonyMorale01;
        public float ResourceFoodDemandPerDay;
        public float ResourceFoodSatisfied01;
        public float WaterReserveUnits;
        public float EducationIndex01;
    }

    /// <summary>Глобальные правила автономии для колонии.</summary>
    public struct SettlerAutonomyPolicyState : IComponentData
    {
        public byte DefaultAutonomyLevel;
        public byte GlobalAlertLevel;
        public float SafetyOverrideHealthThreshold;
    }

    /// <summary>Технический стабильный ID поселенца в runtime.</summary>
    public struct SettlerRuntimeId : IComponentData
    {
        public uint Value;
    }

    /// <summary>Промежуточное состояние суточного процессинга поселенца.</summary>
    public struct SettlerRuntimeState : IComponentData
    {
        public uint BirthDay;
        public uint DaysSinceLastBreak;
        public uint LastSkillDecayDay;
        public float WorkContributionToday;
    }

    /// <summary>История использования навыков для деградации при простое (§3.2).</summary>
    public struct SkillUsageTracker : IComponentData
    {
        /// <summary>Счётчик дней с последнего использования навыка (индекс == SkillId).</summary>
        public FixedList32Bytes<byte> DaysSinceUse;

        /// <summary>Накопленный "впустую" XP при упоре в NaturalCap (индекс == SkillId).</summary>
        public FixedList64Bytes<ushort> WastedXp;
    }

    /// <summary>
    /// Ребро социального графа колонии (опциональный агрегат).
    /// Используется как быстрый срез отношений без обхода всех сущностей.
    /// </summary>
    public struct SettlerRelationshipEdge : IBufferElementData
    {
        public uint SourceSettlerId;
        public uint TargetSettlerId;
        public sbyte Value;
        public byte RelationshipType;
        public ushort DurationDays;
    }
}
