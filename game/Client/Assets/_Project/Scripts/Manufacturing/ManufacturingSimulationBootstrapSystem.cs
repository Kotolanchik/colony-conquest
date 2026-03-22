using ColonyConquest.Economy;
using Unity.Collections;
using Unity.Entities;

namespace ColonyConquest.Manufacturing
{
    /// <summary>Создаёт singleton manufacturing и демо-набор заводов/заказов.</summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct ManufacturingSimulationBootstrapSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            if (SystemAPI.HasSingleton<ManufacturingSimulationSingleton>())
                return;

            var em = state.EntityManager;
            var entity = em.CreateEntity();
            em.AddComponent<ManufacturingSimulationSingleton>(entity);
            em.AddComponent(entity, new ManufacturingSimulationState
            {
                LastProcessedDay = uint.MaxValue,
                LastOrderId = 8,
                CurrentPolicy = ManufacturingMobilizationPolicy.PeaceEconomy,
                DesiredPolicy = ManufacturingMobilizationPolicy.PeaceEconomy,
                SwitchDaysRemaining = 0,
                RetoolingPenalty01 = 1f,
                OrdersCompletedTotal = 0,
                OrdersBlockedByResourcesTotal = 0,
                OrdersBlockedByEraTotal = 0,
                MilitaryOutputToday = 0f,
                CivilianOutputToday = 0f,
                HeavyOutputToday = 0f,
                MilitaryOutputTotal = 0f,
                CivilianOutputTotal = 0f,
                HeavyOutputTotal = 0f,
                EnergyDemandKwToday = 0f,
                EnergySatisfied01 = 1f
            });

            var plants = em.AddBuffer<ManufacturingPlantRuntimeEntry>(entity);
            plants.Add(BuildPlant(1, ManufacturingPlantKind.Smithy, ManufacturingPlantCategory.Military, GameEpoch.Epoch1_Foundation,
                12, 10, 0.10f, 0.90f, 1f, 30f, "smithy"));
            plants.Add(BuildPlant(2, ManufacturingPlantKind.PowderWorkshop, ManufacturingPlantCategory.Military,
                GameEpoch.Epoch1_Foundation, 8, 7, 0.08f, 0.88f, 1f, 20f, "powder-workshop"));
            plants.Add(BuildPlant(3, ManufacturingPlantKind.RiflePlant, ManufacturingPlantCategory.Military,
                GameEpoch.Epoch2_Industrialization, 24, 20, 0.25f, 0.91f, 1.2f, 60f, "rifle-plant"));
            plants.Add(BuildPlant(4, ManufacturingPlantKind.TankPlant, ManufacturingPlantCategory.Military,
                GameEpoch.Epoch4_WorldWar2, 80, 60, 0.35f, 0.85f, 1.2f, 260f, "tank-plant"));
            plants.Add(BuildPlant(5, ManufacturingPlantKind.Mill, ManufacturingPlantCategory.Civilian, GameEpoch.Epoch1_Foundation,
                18, 14, 0.15f, 0.90f, 1f, 25f, "mill"));
            plants.Add(BuildPlant(6, ManufacturingPlantKind.CanningPlant, ManufacturingPlantCategory.Civilian,
                GameEpoch.Epoch2_Industrialization, 22, 18, 0.25f, 0.86f, 1.1f, 45f, "canning-plant"));
            plants.Add(BuildPlant(7, ManufacturingPlantKind.ChipFab, ManufacturingPlantCategory.Civilian, GameEpoch.Epoch5_Modern,
                40, 30, 0.85f, 0.93f, 1.5f, 420f, "chip-fab"));
            plants.Add(BuildPlant(8, ManufacturingPlantKind.BlastFurnace, ManufacturingPlantCategory.HeavyIndustry,
                GameEpoch.Epoch2_Industrialization, 30, 26, 0.20f, 0.84f, 1.2f, 180f, "blast-furnace"));
            plants.Add(BuildPlant(9, ManufacturingPlantKind.Converter, ManufacturingPlantCategory.HeavyIndustry,
                GameEpoch.Epoch2_Industrialization, 26, 22, 0.30f, 0.87f, 1.2f, 160f, "converter"));
            plants.Add(BuildPlant(10, ManufacturingPlantKind.Refinery, ManufacturingPlantCategory.HeavyIndustry,
                GameEpoch.Epoch3_WorldWar1, 34, 28, 0.42f, 0.82f, 1.3f, 280f, "refinery"));

            var orders = em.AddBuffer<ManufacturingProductionOrderEntry>(entity);
            orders.Add(BuildOrder(1, 1, ManufacturingProductKind.Musket, 120f, ManufacturingPriority.High, true, "muskets-batch"));
            orders.Add(BuildOrder(2, 2, ManufacturingProductKind.GunpowderBatch, 80f, ManufacturingPriority.High, true,
                "gunpowder-batch"));
            orders.Add(BuildOrder(3, 3, ManufacturingProductKind.BoltActionRifle, 200f, ManufacturingPriority.Normal, true,
                "rifle-production"));
            orders.Add(BuildOrder(4, 4, ManufacturingProductKind.MediumTank, 6f, ManufacturingPriority.Critical, true, "tank-run"));
            orders.Add(BuildOrder(5, 5, ManufacturingProductKind.BreadBatch, 150f, ManufacturingPriority.Normal, false, "bread-run"));
            orders.Add(BuildOrder(6, 6, ManufacturingProductKind.CannedFoodBatch, 100f, ManufacturingPriority.Normal, false,
                "canned-food"));
            orders.Add(BuildOrder(7, 8, ManufacturingProductKind.CastIronBatch, 90f, ManufacturingPriority.Normal, false,
                "cast-iron"));
            orders.Add(BuildOrder(8, 10, ManufacturingProductKind.PetroleumProductsBatch, 60f, ManufacturingPriority.High, false,
                "petroleum-batch"));

            em.AddBuffer<ManufacturingProductStockEntry>(entity);
        }

        private static ManufacturingPlantRuntimeEntry BuildPlant(
            uint id,
            ManufacturingPlantKind kind,
            ManufacturingPlantCategory category,
            GameEpoch minEpoch,
            ushort workerSlots,
            ushort assignedWorkers,
            float automation01,
            float condition01,
            float throughputMultiplier,
            float energyDemandKw,
            in FixedString64Bytes name)
        {
            return new ManufacturingPlantRuntimeEntry
            {
                PlantId = id,
                Kind = kind,
                Category = category,
                MinEpoch = minEpoch,
                WorkerSlots = workerSlots,
                AssignedWorkers = assignedWorkers,
                Automation01 = automation01,
                Condition01 = condition01,
                ThroughputMultiplier = throughputMultiplier,
                EnergyDemandKw = energyDemandKw,
                DebugName = name
            };
        }

        private static ManufacturingProductionOrderEntry BuildOrder(
            uint id,
            uint plantId,
            ManufacturingProductKind product,
            float target,
            ManufacturingPriority priority,
            bool isMilitary,
            in FixedString64Bytes debugName)
        {
            ManufacturingSimulationMath.TryGetProductDefinition(product, out var def);
            return new ManufacturingProductionOrderEntry
            {
                OrderId = id,
                PlantId = plantId,
                Product = product,
                Priority = priority,
                TargetUnits = target,
                ProducedUnits = 0f,
                BaseHoursPerUnit = def.BaseHoursPerUnit > 0f ? def.BaseHoursPerUnit : 1f,
                AccumulatedHours = 0f,
                IsMilitary = isMilitary ? (byte)1 : (byte)0,
                IsCompleted = 0,
                IsBlockedByResources = 0,
                IsBlockedByEra = 0,
                DebugName = debugName
            };
        }
    }
}
