using ColonyConquest.Economy;
using Unity.Collections;
using Unity.Entities;

namespace ColonyConquest.Manufacturing
{
    public enum ManufacturingPlantCategory : byte
    {
        Military = 0,
        Civilian = 1,
        HeavyIndustry = 2
    }

    public enum ManufacturingMobilizationPolicy : byte
    {
        PeaceEconomy = 0,
        PartialMobilization = 1,
        TotalMobilization = 2,
        ResourceSaving = 3
    }

    public enum ManufacturingPriority : byte
    {
        Critical = 1,
        High = 2,
        Normal = 3,
        Low = 4
    }

    public enum ManufacturingPlantKind : byte
    {
        Smithy = 1,
        PowderWorkshop = 2,
        CannonFoundry = 3,
        RiflePlant = 4,
        MachineGunPlant = 5,
        ArtilleryPlant = 6,
        CartridgePlant = 7,
        AssaultRiflePlant = 8,
        TankPlant = 9,
        AviationPlant = 10,
        DronePlant = 11,
        RocketPlant = 12,
        Mill = 13,
        Bakery = 14,
        CanningPlant = 15,
        TextileMill = 16,
        SewingFactory = 17,
        FurnitureFactory = 18,
        ElectronicsFactory = 19,
        ComputerFactory = 20,
        ChipFab = 21,
        BlastFurnace = 22,
        Converter = 23,
        ElectricFurnace = 24,
        RollingMill = 25,
        Refinery = 26,
        ChemicalPlant = 27,
        PlasticPlant = 28,
        PharmaPlant = 29
    }

    public enum ManufacturingProductKind : byte
    {
        Musket = 1,
        Pike = 2,
        GunpowderBatch = 3,
        BronzeCannon = 4,
        BoltActionRifle = 5,
        MachineGun = 6,
        Howitzer = 7,
        CartridgeBatch = 8,
        AssaultRifle = 9,
        MediumTank = 10,
        CombatDrone = 11,
        BallisticMissile = 12,
        BreadBatch = 13,
        CannedFoodBatch = 14,
        ClothBatch = 15,
        ClothingBatch = 16,
        FurnitureSet = 17,
        ConsumerElectronics = 18,
        Computer = 19,
        MicrochipBatch = 20,
        CastIronBatch = 21,
        SteelIndustrialBatch = 22,
        AlloySteelBatch = 23,
        SteelRolledBatch = 24,
        PetroleumProductsBatch = 25,
        PlasticBatch = 26,
        MedicineBatch = 27,
        ExplosiveBatch = 28
    }

    /// <summary>Маркер сущности runtime симуляции производственных заводов.</summary>
    public struct ManufacturingSimulationSingleton : IComponentData
    {
    }

    /// <summary>Сводное состояние домена manufacturing plants.</summary>
    public struct ManufacturingSimulationState : IComponentData
    {
        public uint LastProcessedDay;
        public uint LastOrderId;

        public ManufacturingMobilizationPolicy CurrentPolicy;
        public ManufacturingMobilizationPolicy DesiredPolicy;
        public byte SwitchDaysRemaining;
        public float RetoolingPenalty01;

        public uint OrdersCompletedTotal;
        public uint OrdersBlockedByResourcesTotal;
        public uint OrdersBlockedByEraTotal;

        public float MilitaryOutputToday;
        public float CivilianOutputToday;
        public float HeavyOutputToday;

        public float MilitaryOutputTotal;
        public float CivilianOutputTotal;
        public float HeavyOutputTotal;

        public float EnergyDemandKwToday;
        public float EnergySatisfied01;
    }

    /// <summary>Runtime-конфиг и состояние производственной площадки.</summary>
    public struct ManufacturingPlantRuntimeEntry : IBufferElementData
    {
        public uint PlantId;
        public ManufacturingPlantKind Kind;
        public ManufacturingPlantCategory Category;
        public GameEpoch MinEpoch;
        public ushort WorkerSlots;
        public ushort AssignedWorkers;
        public float Automation01;
        public float Condition01;
        public float ThroughputMultiplier;
        public float EnergyDemandKw;
        public FixedString64Bytes DebugName;
    }

    /// <summary>Заказ производства на заводе.</summary>
    public struct ManufacturingProductionOrderEntry : IBufferElementData
    {
        public uint OrderId;
        public uint PlantId;
        public ManufacturingProductKind Product;
        public ManufacturingPriority Priority;
        public float TargetUnits;
        public float ProducedUnits;
        public float BaseHoursPerUnit;
        public float AccumulatedHours;
        public byte IsMilitary;
        public byte IsCompleted;
        public byte IsBlockedByResources;
        public byte IsBlockedByEra;
        public FixedString64Bytes DebugName;
    }

    /// <summary>Склад произведённых товаров, не имеющих прямого эквивалента в ResourceId.</summary>
    public struct ManufacturingProductStockEntry : IBufferElementData
    {
        public ManufacturingProductKind Product;
        public float Amount;
    }

    /// <summary>Дефолт продукта для производственной математики.</summary>
    public struct ManufacturingProductDefinition
    {
        public ManufacturingPlantCategory Category;
        public GameEpoch MinEpoch;
        public ResourceId In0;
        public float Amount0;
        public ResourceId In1;
        public float Amount1;
        public ResourceId In2;
        public float Amount2;
        public ResourceId OutputResource;
        public float OutputAmount;
        public float BaseHoursPerUnit;
        public byte IsMilitary;
    }
}
