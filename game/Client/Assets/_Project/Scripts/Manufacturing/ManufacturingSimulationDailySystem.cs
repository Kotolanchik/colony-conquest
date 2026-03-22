using ColonyConquest.Analytics;
using ColonyConquest.Core;
using ColonyConquest.Economy;
using ColonyConquest.Settlers;
using ColonyConquest.Simulation;
using ColonyConquest.Story;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace ColonyConquest.Manufacturing
{
    /// <summary>
    /// Суточный контур производственных заводов: заказы, ресурсы, переключение политики «мечи или плуги», выпуск продукции.
    /// </summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(ColonyCalendarAdvanceSystem))]
    [UpdateAfter(typeof(EconomySimulationDailySystem))]
    [UpdateAfter(typeof(SettlerSimulationDailySystem))]
    public partial struct ManufacturingSimulationDailySystem : ISystem
    {
        private const uint EventPolicySwitch = 0xEB01;
        private const uint EventOrderCompleted = 0xEB02;
        private const uint EventOrderBlocked = 0xEB03;
        private const uint EventOrderEraBlocked = 0xEB04;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameCalendarState>();
            state.RequireForUpdate<SimulationRootState>();
            state.RequireForUpdate<ResourceStockpileSingleton>();
            state.RequireForUpdate<ManufacturingSimulationSingleton>();
            state.RequireForUpdate<ManufacturingSimulationState>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var day = SystemAPI.GetSingleton<GameCalendarState>().DayIndex;
            var tick = SystemAPI.GetSingleton<SimulationRootState>().SimulationTick;
            ref var sim = ref SystemAPI.GetSingletonRW<ManufacturingSimulationState>().ValueRW;
            if (sim.LastProcessedDay == day)
                return;
            sim.LastProcessedDay = day;

            var stock = SystemAPI.GetSingletonBuffer<ResourceStockEntry>(ref state);
            var plants = SystemAPI.GetSingletonBuffer<ManufacturingPlantRuntimeEntry>(ref state);
            var orders = SystemAPI.GetSingletonBuffer<ManufacturingProductionOrderEntry>(ref state);
            var productStock = SystemAPI.GetSingletonBuffer<ManufacturingProductStockEntry>(ref state);

            var workerQuality01 = SystemAPI.HasSingleton<SettlerSimulationState>()
                ? math.clamp(SystemAPI.GetSingleton<SettlerSimulationState>().AverageWorkEfficiency01, 0.25f, 2f) * 0.5f
                : 0.75f;
            var energyRatio01 = 1f;
            if (SystemAPI.HasSingleton<EconomyEnergyState>())
            {
                var energy = SystemAPI.GetSingleton<EconomyEnergyState>();
                energyRatio01 = energy.DemandKw > 1e-3f ? math.saturate(energy.DeliveredKw / energy.DemandKw) : 1f;
            }

            var desiredPolicy = ResolveDesiredPolicy(ref state, energyRatio01);
            if (sim.DesiredPolicy != desiredPolicy)
                sim.DesiredPolicy = desiredPolicy;

            if (sim.CurrentPolicy != sim.DesiredPolicy && sim.SwitchDaysRemaining == 0)
            {
                sim.CurrentPolicy = sim.DesiredPolicy;
                sim.SwitchDaysRemaining = ManufacturingSimulationMath.RetoolingDays;
                TryEnqueueStoryEvent(ref state, tick, EventPolicySwitch, new FixedString64Bytes("manufacturing-policy-switch"));
            }

            if (sim.SwitchDaysRemaining > 0)
                sim.SwitchDaysRemaining--;

            sim.RetoolingPenalty01 = ManufacturingSimulationMath.ComputeRetoolingPenalty01(sim.SwitchDaysRemaining);
            ManufacturingSimulationMath.GetPolicyMultipliers(sim.CurrentPolicy, out var policyMilitaryMult, out var policyCivilianMult);

            var currentEpoch = ResolveCurrentEpoch(ref state);

            sim.MilitaryOutputToday = 0f;
            sim.CivilianOutputToday = 0f;
            sim.HeavyOutputToday = 0f;
            sim.EnergyDemandKwToday = 0f;
            var backlog = 0u;
            var blockedResourcesToday = 0u;
            var blockedEraToday = 0u;

            for (var i = 0; i < orders.Length; i++)
            {
                var order = orders[i];
                if (order.IsCompleted != 0)
                    continue;

                backlog++;
                var plantIndex = FindPlantIndex(ref plants, order.PlantId);
                if (plantIndex < 0)
                {
                    order.IsCompleted = 1;
                    orders[i] = order;
                    continue;
                }

                var plant = plants[plantIndex];
                if (!ManufacturingSimulationMath.TryGetProductDefinition(order.Product, out var productDef))
                {
                    order.IsCompleted = 1;
                    orders[i] = order;
                    continue;
                }

                order.BaseHoursPerUnit = math.max(0.02f, productDef.BaseHoursPerUnit);
                sim.EnergyDemandKwToday += plant.EnergyDemandKw;

                if (currentEpoch < productDef.MinEpoch || currentEpoch < plant.MinEpoch)
                {
                    order.IsBlockedByEra = 1;
                    order.IsBlockedByResources = 0;
                    blockedEraToday++;
                    orders[i] = order;
                    if (blockedEraToday <= 3)
                        TryEnqueueStoryEvent(ref state, tick, EventOrderEraBlocked, new FixedString64Bytes("manufacturing-era-blocked"));
                    continue;
                }

                var policyMult = order.IsMilitary != 0 ? policyMilitaryMult : policyCivilianMult;
                if (plant.Category == ManufacturingPlantCategory.HeavyIndustry)
                    policyMult = math.lerp(policyCivilianMult, policyMilitaryMult, 0.35f);

                var priorityMult = GetPriorityMultiplier(order.Priority);
                var plantEfficiency = ManufacturingSimulationMath.ComputePlantEfficiency(
                    plant.AssignedWorkers,
                    plant.WorkerSlots,
                    plant.Automation01,
                    plant.Condition01,
                    workerQuality01,
                    energyRatio01);
                var dailyHours = 24f * math.max(0.25f, plant.ThroughputMultiplier) * plantEfficiency * priorityMult *
                                 math.max(0.1f, policyMult) * sim.RetoolingPenalty01;
                order.AccumulatedHours += dailyHours;

                var unitsFromTime = math.floor(order.AccumulatedHours / order.BaseHoursPerUnit);
                var targetRemaining = math.max(0f, order.TargetUnits - order.ProducedUnits);
                if (unitsFromTime < 1f || targetRemaining <= 1e-4f)
                {
                    orders[i] = order;
                    continue;
                }

                var candidateUnits = math.min(unitsFromTime, targetRemaining);
                var unitsFromResources = ComputeUnitsByResources(ref stock, in productDef);
                var produceUnits = math.min(candidateUnits, unitsFromResources);
                if (produceUnits <= 1e-4f)
                {
                    order.IsBlockedByResources = 1;
                    order.IsBlockedByEra = 0;
                    blockedResourcesToday++;
                    orders[i] = order;
                    if (blockedResourcesToday <= 4)
                        TryEnqueueStoryEvent(ref state, tick, EventOrderBlocked, new FixedString64Bytes("manufacturing-resource-blocked"));
                    continue;
                }

                ConsumeInputs(ref stock, in productDef, produceUnits);
                order.AccumulatedHours = math.max(0f, order.AccumulatedHours - produceUnits * order.BaseHoursPerUnit);
                order.ProducedUnits += produceUnits;
                order.IsBlockedByResources = 0;
                order.IsBlockedByEra = 0;

                var outputUnits = produceUnits * math.max(0.01f, productDef.OutputAmount);
                if (productDef.OutputResource != ResourceId.None)
                    ResourceStockpileOps.Add(ref stock, productDef.OutputResource, outputUnits);
                AddProductStock(ref productStock, order.Product, outputUnits);

                var outputValue = EstimateOutputValue(productDef.OutputResource, outputUnits, order.Product);
                switch (productDef.Category)
                {
                    case ManufacturingPlantCategory.Military:
                        sim.MilitaryOutputToday += outputValue;
                        break;
                    case ManufacturingPlantCategory.Civilian:
                        sim.CivilianOutputToday += outputValue;
                        break;
                    default:
                        sim.HeavyOutputToday += outputValue;
                        break;
                }

                plant.Condition01 = math.clamp(plant.Condition01 - produceUnits * 0.0015f, 0.35f, 1f);
                plants[plantIndex] = plant;

                if (order.ProducedUnits + 1e-4f >= order.TargetUnits)
                {
                    order.IsCompleted = 1;
                    backlog = math.max(0u, backlog - 1u);
                    sim.OrdersCompletedTotal++;
                    TryEnqueueStoryEvent(ref state, tick, EventOrderCompleted, new FixedString64Bytes("manufacturing-order-completed"));
                }

                orders[i] = order;
            }

            sim.OrdersBlockedByResourcesTotal += blockedResourcesToday;
            sim.OrdersBlockedByEraTotal += blockedEraToday;
            sim.EnergySatisfied01 = energyRatio01;

            sim.MilitaryOutputTotal += sim.MilitaryOutputToday;
            sim.CivilianOutputTotal += sim.CivilianOutputToday;
            sim.HeavyOutputTotal += sim.HeavyOutputToday;

            if (SystemAPI.HasSingleton<EconomyMilitaryIndustryState>())
            {
                ref var militaryIndustry = ref SystemAPI.GetSingletonRW<EconomyMilitaryIndustryState>().ValueRW;
                militaryIndustry.MilitaryOutputToday += sim.MilitaryOutputToday;
                militaryIndustry.CivilianOutputToday += sim.CivilianOutputToday + sim.HeavyOutputToday * 0.5f;
            }

            if (SystemAPI.HasSingleton<EconomySimulationState>())
            {
                ref var economy = ref SystemAPI.GetSingletonRW<EconomySimulationState>().ValueRW;
                var militaryShare = (sim.MilitaryOutputToday + 1f) /
                                    (sim.MilitaryOutputToday + sim.CivilianOutputToday + sim.HeavyOutputToday + 1f);
                economy.MilitaryProductionShare01 = math.saturate(militaryShare);
            }

            var virtualStockUnits = 0f;
            for (var i = 0; i < productStock.Length; i++)
                virtualStockUnits += productStock[i].Amount;

            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.ManufacturingOrdersBacklog, backlog);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.ManufacturingOrdersCompletedTotal,
                sim.OrdersCompletedTotal);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.ManufacturingOrdersBlockedResourcesTotal,
                sim.OrdersBlockedByResourcesTotal);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.ManufacturingOrdersBlockedEraTotal,
                sim.OrdersBlockedByEraTotal);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.ManufacturingMilitaryOutputToday,
                sim.MilitaryOutputToday);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.ManufacturingCivilianOutputToday,
                sim.CivilianOutputToday);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.ManufacturingHeavyOutputToday,
                sim.HeavyOutputToday);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.ManufacturingEnergyDemandKw,
                sim.EnergyDemandKwToday);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.ManufacturingEnergySatisfied01,
                sim.EnergySatisfied01);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.ManufacturingPolicy,
                (byte)sim.CurrentPolicy);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.ManufacturingRetoolingPenalty01,
                sim.RetoolingPenalty01);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.ManufacturingVirtualStockUnits,
                virtualStockUnits);
        }

        private static ManufacturingMobilizationPolicy ResolveDesiredPolicy(ref SystemState state, float energyRatio01)
        {
            if (SystemAPI.HasSingleton<EconomySimulationState>())
            {
                var economy = SystemAPI.GetSingleton<EconomySimulationState>();
                if (economy.Phase == EconomyCyclePhase.Warfare)
                    return ManufacturingMobilizationPolicy.TotalMobilization;
                if (economy.Phase == EconomyCyclePhase.Preparation)
                    return ManufacturingMobilizationPolicy.PartialMobilization;
                if (energyRatio01 < 0.55f)
                    return ManufacturingMobilizationPolicy.ResourceSaving;
            }

            return ManufacturingMobilizationPolicy.PeaceEconomy;
        }

        private static GameEpoch ResolveCurrentEpoch(ref SystemState state)
        {
            if (!SystemAPI.HasSingleton<ColonyTechProgressState>())
                return GameEpoch.Epoch1_Foundation;
            var tech = SystemAPI.GetSingleton<ColonyTechProgressState>();
            return ManufacturingSimulationMath.ToGameEpoch(tech.CurrentEra);
        }

        private static int FindPlantIndex(ref DynamicBuffer<ManufacturingPlantRuntimeEntry> plants, uint plantId)
        {
            for (var i = 0; i < plants.Length; i++)
            {
                if (plants[i].PlantId == plantId)
                    return i;
            }
            return -1;
        }

        private static float GetPriorityMultiplier(ManufacturingPriority priority)
        {
            return priority switch
            {
                ManufacturingPriority.Critical => 1.25f,
                ManufacturingPriority.High => 1.10f,
                ManufacturingPriority.Low => 0.85f,
                _ => 1f
            };
        }

        private static float ComputeUnitsByResources(ref DynamicBuffer<ResourceStockEntry> stock,
            in ManufacturingProductDefinition product)
        {
            var by0 = GetByInput(ref stock, product.In0, product.Amount0);
            var by1 = GetByInput(ref stock, product.In1, product.Amount1);
            var by2 = GetByInput(ref stock, product.In2, product.Amount2);
            return math.min(by0, math.min(by1, by2));
        }

        private static float GetByInput(ref DynamicBuffer<ResourceStockEntry> stock, ResourceId id, float amountPerUnit)
        {
            if (id == ResourceId.None || amountPerUnit <= 1e-4f)
                return float.MaxValue;
            var available = ResourceStockpileOps.GetAmount(ref stock, id);
            return available / amountPerUnit;
        }

        private static void ConsumeInputs(ref DynamicBuffer<ResourceStockEntry> stock, in ManufacturingProductDefinition product,
            float units)
        {
            ConsumeInput(ref stock, product.In0, product.Amount0, units);
            ConsumeInput(ref stock, product.In1, product.Amount1, units);
            ConsumeInput(ref stock, product.In2, product.Amount2, units);
        }

        private static void ConsumeInput(ref DynamicBuffer<ResourceStockEntry> stock, ResourceId id, float amountPerUnit, float units)
        {
            if (id == ResourceId.None || amountPerUnit <= 0f || units <= 0f)
                return;
            ResourceStockpileOps.TryConsume(ref stock, id, amountPerUnit * units);
        }

        private static void AddProductStock(ref DynamicBuffer<ManufacturingProductStockEntry> stock,
            ManufacturingProductKind product, float amount)
        {
            if (amount <= 0f)
                return;
            for (var i = 0; i < stock.Length; i++)
            {
                if (stock[i].Product != product)
                    continue;
                var entry = stock[i];
                entry.Amount += amount;
                stock[i] = entry;
                return;
            }

            stock.Add(new ManufacturingProductStockEntry
            {
                Product = product,
                Amount = amount
            });
        }

        private static float EstimateOutputValue(ResourceId outputResource, float amount, ManufacturingProductKind product)
        {
            if (outputResource != ResourceId.None && ResourceCatalog.TryGet(outputResource, out var def))
                return amount * def.BasePrice;

            // Для виртуальных товаров (танки/дроны/мебель и т.д.) используем стабильную оценку по типу.
            var unitPrice = product switch
            {
                ManufacturingProductKind.MediumTank => 1200f,
                ManufacturingProductKind.CombatDrone => 900f,
                ManufacturingProductKind.BallisticMissile => 5000f,
                ManufacturingProductKind.MachineGun => 250f,
                ManufacturingProductKind.Howitzer => 600f,
                ManufacturingProductKind.ConsumerElectronics => 180f,
                ManufacturingProductKind.Computer => 1200f,
                ManufacturingProductKind.FurnitureSet => 200f,
                ManufacturingProductKind.CannedFoodBatch => 80f,
                _ => 100f
            };
            return amount * unitPrice;
        }

        private static void TryEnqueueStoryEvent(ref SystemState state, uint tick, uint eventDefinitionId,
            in FixedString64Bytes label)
        {
            if (!SystemAPI.HasSingleton<StoryEventQueueSingleton>())
                return;
            var queue = SystemAPI.GetSingletonBuffer<GameEventQueueEntry>(ref state);
            queue.Add(new GameEventQueueEntry
            {
                Kind = StoryEventKind.Triggered,
                EventDefinitionId = eventDefinitionId,
                EnqueueSimulationTick = tick,
                DebugLabel = label
            });
        }
    }
}
