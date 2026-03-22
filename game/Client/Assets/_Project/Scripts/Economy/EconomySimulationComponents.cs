using Unity.Collections;
using Unity.Entities;

namespace ColonyConquest.Economy
{
    public enum EconomyCyclePhase : byte
    {
        Accumulation = 0,
        Expansion = 1,
        Preparation = 2,
        Warfare = 3,
        Recovery = 4
    }

    public enum EconomyFacilityKind : byte
    {
        Workshop = 0,
        Manufacture = 1,
        Factory = 2,
        Plant = 3,
        Complex = 4
    }

    public enum EconomyPowerGeneratorKind : byte
    {
        WaterWheel = 0,
        SteamEngine = 1,
        SteamTurbine = 2,
        DieselGenerator = 3,
        CoalPlant = 4,
        OilPlant = 5,
        HydroPlant = 6,
        NuclearReactor = 7,
        SolarFarm = 8,
        WindFarm = 9
    }

    public enum EconomyTransportKind : byte
    {
        Handcart = 0,
        HorseWagon = 1,
        Truck = 2,
        HeavyTruck = 3,
        CargoTrain = 4,
        CargoPlane = 5,
        CargoDrone = 6,
        Conveyor = 7
    }

    public enum EconomyMilitaryProductionMode : byte
    {
        Peace = 0,
        Mixed = 1,
        Military = 2
    }

    public enum EconomyProductionPriority : byte
    {
        Critical = 1,
        High = 2,
        Normal = 3,
        Low = 4,
        Minimal = 5
    }

    public enum EconomyBottleneckKind : byte
    {
        None = 0,
        ResourceExtraction = 1,
        EnergySupply = 2,
        Logistics = 3,
        Workforce = 4,
        Research = 5,
        MilitaryProduction = 6
    }

    /// <summary>Маркер сущности полной симуляции экономики.</summary>
    public struct EconomySimulationSingleton : IComponentData
    {
    }

    /// <summary>Макро-состояние экономики и текущей фазы цикла.</summary>
    public struct EconomySimulationState : IComponentData
    {
        public uint LastProcessedDay;
        public EconomyCyclePhase Phase;
        public byte DaysInPhase;

        public float Infrastructure01;
        public byte EconomicScaleLevel;

        public EconomyProductionPriority MilitaryPriority;
        public float MilitaryProductionShare01;

        public float LogisticsEfficiency01;
        public float ProductionEfficiency01;
        public EconomyBottleneckKind Bottleneck;

        public float InflationPercent;
        public float Unemployment01;
        public float ExportVolume;
        public float ImportVolume;
        public float TradeBalance;
    }

    /// <summary>Агрегаты энергосистемы экономики.</summary>
    public struct EconomyEnergyState : IComponentData
    {
        public float GeneratedKw;
        public float DemandKw;
        public float DeliveredKw;
        public float TransmissionDistanceKm;
        public float TransmissionLoss01;

        public float StorageCapacityKwh;
        public float StorageChargeKwh;
        public float StorageRoundTripEfficiency01;
    }

    /// <summary>Агрегаты логистики и транспорта.</summary>
    public struct EconomyLogisticsState : IComponentData
    {
        public float TotalTransportTonKmPerDay;
        public float RequiredTonKmPerDay;
        public float AverageRouteDistanceKm;
        public float ConveyorThroughputKgPerSecond;
        public float RouteComplexity01;
    }

    /// <summary>Агрегаты складской системы.</summary>
    public struct EconomyWarehouseState : IComponentData
    {
        public float TotalCapacityKg;
        public float UsedCapacityKg;
        public float Overload01;
        public float ProcessingSecondsPerTon;
        public float InventoryDriftPercentPerDay;
    }

    /// <summary>Состояние военного производства: civilian/military output и время переключения.</summary>
    public struct EconomyMilitaryIndustryState : IComponentData
    {
        public float CivilianOutputToday;
        public float MilitaryOutputToday;
        public float SwitchHoursRemaining;
    }

    /// <summary>Состояние снабжения армии по типам ресурсов.</summary>
    public struct EconomyArmySupplyState : IComponentData
    {
        public float ArmySupplyAdequacy01;
        public float ProvisionsNeedKgPerDay;
        public float FuelNeedLitersPerDay;
        public float AmmunitionNeedKgPerDay;
        public float SparePartsNeedKgPerDay;
        public float MedicalNeedKgPerDay;
    }

    /// <summary>Производственная площадка (обобщение мастерская/фабрика/завод/комплекс).</summary>
    public struct EconomyProductionFacilityEntry : IBufferElementData
    {
        public uint FacilityId;
        public FixedString64Bytes DebugName;
        public EconomyFacilityKind Kind;
        public GameEpoch Era;
        public ProductionRecipeId ActiveRecipe;

        public byte AssignedWorkers;
        public byte OptimalWorkers;
        public byte MasterCount;
        public byte UpgradeLevel;

        public float AverageSkill0To100;
        public float EnergyRequiredKw;
        public float BuildingWear01;
        public float ToolCondition01;
        public float BaseSpeedMultiplier;

        public EconomyMilitaryProductionMode MilitaryMode;
        public EconomyProductionPriority Priority;
        public float MilitaryShare01;
    }

    /// <summary>Источник генерации энергии.</summary>
    public struct EconomyPowerGeneratorEntry : IBufferElementData
    {
        public uint GeneratorId;
        public EconomyPowerGeneratorKind Kind;
        public ResourceId FuelResource;
        public float FuelPerDayAtFullLoad;
        public float OutputKw;
        public float Efficiency01;
        public float OutputScale01;
    }

    /// <summary>Маршрут доставки ресурсов/продукции.</summary>
    public struct EconomyTransportRouteEntry : IBufferElementData
    {
        public uint RouteId;
        public EconomyTransportKind Kind;
        public float PayloadKg;
        public float SpeedKmPerHour;
        public float DistanceKm;
        public float LoadCoefficient01;
        public float InfrastructureFactor01;
        public byte IsMilitaryRoute;
    }

    /// <summary>Складской блок.</summary>
    public struct EconomyWarehouseEntry : IBufferElementData
    {
        public uint WarehouseId;
        public float CapacityKg;
        public byte Workers;
        public float Automation01;
        public float Organization01;
        public float ReservedForMilitary01;
    }
}
