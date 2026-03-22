using Unity.Collections;
using Unity.Entities;

namespace ColonyConquest.Economy
{
    /// <summary>Bootstrap полной экономической симуляции: здания, энергия, транспорт, склады.</summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct EconomySimulationBootstrapSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            if (SystemAPI.HasSingleton<EconomySimulationSingleton>())
                return;

            var em = state.EntityManager;
            var e = em.CreateEntity();
            em.AddComponent<EconomySimulationSingleton>(e);

            em.AddComponent(e, new EconomySimulationState
            {
                LastProcessedDay = uint.MaxValue,
                Phase = EconomyCyclePhase.Accumulation,
                DaysInPhase = 0,
                Infrastructure01 = 0.45f,
                EconomicScaleLevel = 2,
                MilitaryPriority = EconomyProductionPriority.Normal,
                MilitaryProductionShare01 = 0.10f,
                LogisticsEfficiency01 = 0.75f,
                ProductionEfficiency01 = 0.8f,
                Bottleneck = EconomyBottleneckKind.None,
                InflationPercent = 0f,
                Unemployment01 = 0.08f,
                ExportVolume = 0f,
                ImportVolume = 0f,
                TradeBalance = 0f
            });

            em.AddComponent(e, new EconomyEnergyState
            {
                GeneratedKw = 0f,
                DemandKw = 0f,
                DeliveredKw = 0f,
                TransmissionDistanceKm = 3f,
                TransmissionLoss01 = 0.10f,
                StorageCapacityKwh = 2500f,
                StorageChargeKwh = 1200f,
                StorageRoundTripEfficiency01 = 0.85f
            });

            em.AddComponent(e, new EconomyLogisticsState
            {
                TotalTransportTonKmPerDay = 0f,
                RequiredTonKmPerDay = 0f,
                AverageRouteDistanceKm = 0f,
                ConveyorThroughputKgPerSecond = 0f,
                RouteComplexity01 = 0.35f
            });

            em.AddComponent(e, new EconomyWarehouseState
            {
                TotalCapacityKg = 0f,
                UsedCapacityKg = 0f,
                Overload01 = 0f,
                ProcessingSecondsPerTon = 120f,
                InventoryDriftPercentPerDay = 0.5f
            });

            em.AddComponent(e, new EconomyMilitaryIndustryState
            {
                CivilianOutputToday = 0f,
                MilitaryOutputToday = 0f,
                SwitchHoursRemaining = 0f
            });

            em.AddComponent(e, new EconomyArmySupplyState
            {
                ArmySupplyAdequacy01 = 1f,
                ProvisionsNeedKgPerDay = 0f,
                FuelNeedLitersPerDay = 0f,
                AmmunitionNeedKgPerDay = 0f,
                SparePartsNeedKgPerDay = 0f,
                MedicalNeedKgPerDay = 0f
            });

            var facilities = em.AddBuffer<EconomyProductionFacilityEntry>(e);
            facilities.Add(new EconomyProductionFacilityEntry
            {
                FacilityId = 1,
                DebugName = new FixedString64Bytes("forge-alpha"),
                Kind = EconomyFacilityKind.Workshop,
                Era = GameEpoch.Epoch1_Foundation,
                ActiveRecipe = ProductionRecipeId.ProduceSteelBasic,
                AssignedWorkers = 4,
                OptimalWorkers = 3,
                MasterCount = 1,
                UpgradeLevel = 1,
                AverageSkill0To100 = 52f,
                EnergyRequiredKw = 0f,
                BuildingWear01 = 0.08f,
                ToolCondition01 = 0.94f,
                BaseSpeedMultiplier = 1f,
                MilitaryMode = EconomyMilitaryProductionMode.Peace,
                Priority = EconomyProductionPriority.Normal,
                MilitaryShare01 = 0f
            });
            facilities.Add(new EconomyProductionFacilityEntry
            {
                FacilityId = 2,
                DebugName = new FixedString64Bytes("steel-mill"),
                Kind = EconomyFacilityKind.Manufacture,
                Era = GameEpoch.Epoch2_Industrialization,
                ActiveRecipe = ProductionRecipeId.RollingMillSteelPlate,
                AssignedWorkers = 8,
                OptimalWorkers = 8,
                MasterCount = 1,
                UpgradeLevel = 2,
                AverageSkill0To100 = 58f,
                EnergyRequiredKw = 50f,
                BuildingWear01 = 0.14f,
                ToolCondition01 = 0.90f,
                BaseSpeedMultiplier = 1.4f,
                MilitaryMode = EconomyMilitaryProductionMode.Mixed,
                Priority = EconomyProductionPriority.Normal,
                MilitaryShare01 = 0.15f
            });
            facilities.Add(new EconomyProductionFacilityEntry
            {
                FacilityId = 3,
                DebugName = new FixedString64Bytes("chem-plant"),
                Kind = EconomyFacilityKind.Factory,
                Era = GameEpoch.Epoch3_WorldWar1,
                ActiveRecipe = ProductionRecipeId.CraftDynamiteIndustrial,
                AssignedWorkers = 12,
                OptimalWorkers = 10,
                MasterCount = 1,
                UpgradeLevel = 2,
                AverageSkill0To100 = 61f,
                EnergyRequiredKw = 120f,
                BuildingWear01 = 0.10f,
                ToolCondition01 = 0.92f,
                BaseSpeedMultiplier = 1.8f,
                MilitaryMode = EconomyMilitaryProductionMode.Mixed,
                Priority = EconomyProductionPriority.High,
                MilitaryShare01 = 0.25f
            });
            facilities.Add(new EconomyProductionFacilityEntry
            {
                FacilityId = 4,
                DebugName = new FixedString64Bytes("war-arsenal"),
                Kind = EconomyFacilityKind.Plant,
                Era = GameEpoch.Epoch4_WorldWar2,
                ActiveRecipe = ProductionRecipeId.CraftExplosiveShellEpoch2,
                AssignedWorkers = 18,
                OptimalWorkers = 20,
                MasterCount = 2,
                UpgradeLevel = 3,
                AverageSkill0To100 = 64f,
                EnergyRequiredKw = 260f,
                BuildingWear01 = 0.12f,
                ToolCondition01 = 0.86f,
                BaseSpeedMultiplier = 2.2f,
                MilitaryMode = EconomyMilitaryProductionMode.Military,
                Priority = EconomyProductionPriority.High,
                MilitaryShare01 = 1f
            });
            facilities.Add(new EconomyProductionFacilityEntry
            {
                FacilityId = 5,
                DebugName = new FixedString64Bytes("future-materials"),
                Kind = EconomyFacilityKind.Complex,
                Era = GameEpoch.Epoch5_Modern,
                ActiveRecipe = ProductionRecipeId.CraftPlasticBakeliteEpoch4,
                AssignedWorkers = 25,
                OptimalWorkers = 25,
                MasterCount = 2,
                UpgradeLevel = 4,
                AverageSkill0To100 = 71f,
                EnergyRequiredKw = 420f,
                BuildingWear01 = 0.06f,
                ToolCondition01 = 0.94f,
                BaseSpeedMultiplier = 2.6f,
                MilitaryMode = EconomyMilitaryProductionMode.Mixed,
                Priority = EconomyProductionPriority.Normal,
                MilitaryShare01 = 0.20f
            });

            var generators = em.AddBuffer<EconomyPowerGeneratorEntry>(e);
            generators.Add(new EconomyPowerGeneratorEntry
            {
                GeneratorId = 1,
                Kind = EconomyPowerGeneratorKind.SteamEngine,
                FuelResource = ResourceId.Coal,
                FuelPerDayAtFullLoad = 18f,
                OutputKw = 50f,
                Efficiency01 = 0.15f,
                OutputScale01 = 1f
            });
            generators.Add(new EconomyPowerGeneratorEntry
            {
                GeneratorId = 2,
                Kind = EconomyPowerGeneratorKind.SteamTurbine,
                FuelResource = ResourceId.CoalCoke,
                FuelPerDayAtFullLoad = 20f,
                OutputKw = 200f,
                Efficiency01 = 0.20f,
                OutputScale01 = 1f
            });
            generators.Add(new EconomyPowerGeneratorEntry
            {
                GeneratorId = 3,
                Kind = EconomyPowerGeneratorKind.DieselGenerator,
                FuelResource = ResourceId.PetroleumProducts,
                FuelPerDayAtFullLoad = 12f,
                OutputKw = 300f,
                Efficiency01 = 0.35f,
                OutputScale01 = 1f
            });
            generators.Add(new EconomyPowerGeneratorEntry
            {
                GeneratorId = 4,
                Kind = EconomyPowerGeneratorKind.HydroPlant,
                FuelResource = ResourceId.None,
                FuelPerDayAtFullLoad = 0f,
                OutputKw = 100f,
                Efficiency01 = 0.80f,
                OutputScale01 = 0.85f
            });

            var routes = em.AddBuffer<EconomyTransportRouteEntry>(e);
            routes.Add(new EconomyTransportRouteEntry
            {
                RouteId = 1,
                Kind = EconomyTransportKind.HorseWagon,
                PayloadKg = 500f,
                SpeedKmPerHour = 10f,
                DistanceKm = 18f,
                LoadCoefficient01 = 0.65f,
                InfrastructureFactor01 = 0.4f,
                IsMilitaryRoute = 0
            });
            routes.Add(new EconomyTransportRouteEntry
            {
                RouteId = 2,
                Kind = EconomyTransportKind.Truck,
                PayloadKg = 8000f,
                SpeedKmPerHour = 30f,
                DistanceKm = 55f,
                LoadCoefficient01 = 0.70f,
                InfrastructureFactor01 = 0.55f,
                IsMilitaryRoute = 1
            });
            routes.Add(new EconomyTransportRouteEntry
            {
                RouteId = 3,
                Kind = EconomyTransportKind.CargoTrain,
                PayloadKg = 500000f,
                SpeedKmPerHour = 50f,
                DistanceKm = 220f,
                LoadCoefficient01 = 0.85f,
                InfrastructureFactor01 = 0.75f,
                IsMilitaryRoute = 1
            });
            routes.Add(new EconomyTransportRouteEntry
            {
                RouteId = 4,
                Kind = EconomyTransportKind.Conveyor,
                PayloadKg = 200f,
                SpeedKmPerHour = 7.2f,
                DistanceKm = 2f,
                LoadCoefficient01 = 0.9f,
                InfrastructureFactor01 = 0.9f,
                IsMilitaryRoute = 0
            });

            var warehouses = em.AddBuffer<EconomyWarehouseEntry>(e);
            warehouses.Add(new EconomyWarehouseEntry
            {
                WarehouseId = 1,
                CapacityKg = 50000f,
                Workers = 4,
                Automation01 = 0.20f,
                Organization01 = 0.50f,
                ReservedForMilitary01 = 0.20f
            });
            warehouses.Add(new EconomyWarehouseEntry
            {
                WarehouseId = 2,
                CapacityKg = 30000f,
                Workers = 5,
                Automation01 = 0.35f,
                Organization01 = 0.60f,
                ReservedForMilitary01 = 0.35f
            });
            warehouses.Add(new EconomyWarehouseEntry
            {
                WarehouseId = 3,
                CapacityKg = 25000f,
                Workers = 6,
                Automation01 = 0.55f,
                Organization01 = 0.65f,
                ReservedForMilitary01 = 0.70f
            });
        }
    }
}
