using Unity.Entities;

namespace ColonyConquest.Housing
{
    /// <summary>Упрощённые типы жилья для демо-симуляции комфорта.</summary>
    public enum HousingTypeId : byte
    {
        None = 0,
        Dugout = 1,
        Hut = 2,
        House = 3,
        Barracks = 4,
        Apartment = 5
    }

    /// <summary>Runtime-данные жилого блока.</summary>
    public struct HousingUnitRuntime : IComponentData
    {
        public HousingTypeId Type;
        public short Capacity;
        public short Residents;
        public float BaseComfort;

        public float PowerCoverage01;
        public float WaterCoverage01;
        public float SewageCoverage01;
        public float HeatingCoverage01;

        public float Cleanliness01;
        public float Noise01;
        public float NeighborhoodQuality01;
        public float DecorLevel01;

        public float Condition01;
        public float BaseDecayPerDay;
        public float MaintenanceRepairPerDay;

        public float DistanceToWork01;
        public byte IsBarracks;
    }

    /// <summary>Итоговая оценка уюта и модификаторы для поселенцев.</summary>
    public struct HousingComfortSnapshot : IComponentData
    {
        public float ComfortScore;
        public float MoodModifier;
        public float ProductivityModifier;
        public byte OvercrowdingBand;
    }

    /// <summary>Политика бараков для военного жилья.</summary>
    public struct BarracksPolicyData : IComponentData
    {
        public float MinSpacePerSoldier;
        public float DisciplineBonus;
        public float RestRecoveryRate;
    }

    /// <summary>Сводные показатели жилья колонии.</summary>
    public struct HousingColonyState : IComponentData
    {
        public uint LastProcessedDay;
        public int TotalCapacity;
        public int TotalResidents;
        public int OvercrowdedUnits;
        public int IncidentsToday;
        public int AssignmentBacklog;
        public float AverageComfort;
    }

    /// <summary>Синглтон с буфером запросов на расселение.</summary>
    public struct HousingAssignmentQueueSingleton : IComponentData
    {
    }

    /// <summary>Состояние процессора расселения для контроля частоты (раз в игровой день).</summary>
    public struct HousingAssignmentProcessState : IComponentData
    {
        public uint LastProcessedDay;
    }

    /// <summary>Запрос на расселение домохозяйства.</summary>
    public struct HousingAssignmentRequestEntry : IBufferElementData
    {
        public uint RequestId;
        public byte Priority;
        public short HouseholdSize;
        public float WorkProximityWeight;
        public float FamilyNeedWeight;
        public float ComfortNeedWeight;
    }
}
