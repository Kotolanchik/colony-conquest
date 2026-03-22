using Unity.Collections;
using Unity.Entities;

namespace ColonyConquest.PlantBreeding
{
    /// <summary>Маркер сущности лаборатории селекции; на ней хранятся буферы сортов и заявок.</summary>
    public struct PlantBreedingLabSingleton : IComponentData
    {
    }

    /// <summary>Состояние лаборатории и планировщика селекции.</summary>
    public struct PlantBreedingLabState : IComponentData
    {
        public uint LastProcessedDay;
        public uint NextCultivarId;
        public byte LabTier;
        public float IsolationLevel01;
        public float DemoPlantationAreaFactor;
    }

    /// <summary>Набор генетических осей сорта (в процентах от базовой культуры).</summary>
    public struct PlantGenomeTraits
    {
        public float Yield;
        public float GrowthSpeed;
        public float DroughtResistance;
        public float ColdResistance;
        public float PestResistance;
        public float NutritionalValue;
        public float Taste;
    }

    /// <summary>Запись каталога сортов, полученная в результате селекции/регистрации.</summary>
    public struct PlantCultivarEntry : IBufferElementData
    {
        public uint CultivarId;
        public FixedString32Bytes DebugName;
        public PlantGenomeTraits Traits;
        public float StabilityScore;
        public float MutationLoad;
        public byte Generation;
        public byte IsGmo;
        public byte BioSafetyTier;
        public float EditDepth;
    }

    /// <summary>Заявка на цикл скрещивания.</summary>
    public struct PlantBreedingWorkOrderEntry : IBufferElementData
    {
        public uint ParentAId;
        public uint ParentBId;
        public float ParentWeightA;
        public float ParentWeightB;
        public short RemainingDays;
        public byte IsGmo;
        public byte BioSafetyTier;
        public float EditDepth;
    }
}
