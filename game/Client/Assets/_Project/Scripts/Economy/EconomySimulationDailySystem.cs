using ColonyConquest.Analytics;
using ColonyConquest.Core;
using ColonyConquest.Military;
using ColonyConquest.Politics;
using ColonyConquest.Simulation;
using ColonyConquest.Story;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace ColonyConquest.Economy
{
    /// <summary>
    /// Полный суточный цикл экономики: производство, энергия, логистика, склады, военные режимы и снабжение армии.
    /// </summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(ColonyCalendarAdvanceSystem))]
    [UpdateBefore(typeof(ColonyTechResearchDailySystem))]
    [UpdateBefore(typeof(PoliticalDailySystem))]
    public partial struct EconomySimulationDailySystem : ISystem
    {
        private const uint EventEconomyPhaseChanged = 0xE601;
        private const uint EventEconomyBottleneck = 0xE602;
        private const uint EventArmySupplyCritical = 0xE603;

        private EntityQuery _armyQuery;

        public void OnCreate(ref SystemState state)
        {
            _armyQuery = state.GetEntityQuery(ComponentType.ReadOnly<BattleUnitTag>());
            state.RequireForUpdate<GameCalendarState>();
            state.RequireForUpdate<SimulationRootState>();
            state.RequireForUpdate<ColonyDemographyState>();
            state.RequireForUpdate<ColonyTechProgressState>();
            state.RequireForUpdate<ResourceStockpileSingleton>();
            state.RequireForUpdate<EconomySimulationSingleton>();
            state.RequireForUpdate<EconomySimulationState>();
            state.RequireForUpdate<EconomyEnergyState>();
            state.RequireForUpdate<EconomyLogisticsState>();
            state.RequireForUpdate<EconomyWarehouseState>();
            state.RequireForUpdate<EconomyMilitaryIndustryState>();
            state.RequireForUpdate<EconomyArmySupplyState>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var day = SystemAPI.GetSingleton<GameCalendarState>().DayIndex;
            ref var sim = ref SystemAPI.GetSingletonRW<EconomySimulationState>().ValueRW;
            if (sim.LastProcessedDay == day)
                return;
            sim.LastProcessedDay = day;

            ref var energy = ref SystemAPI.GetSingletonRW<EconomyEnergyState>().ValueRW;
            ref var logistics = ref SystemAPI.GetSingletonRW<EconomyLogisticsState>().ValueRW;
            ref var warehouse = ref SystemAPI.GetSingletonRW<EconomyWarehouseState>().ValueRW;
            ref var militaryIndustry = ref SystemAPI.GetSingletonRW<EconomyMilitaryIndustryState>().ValueRW;
            ref var armySupply = ref SystemAPI.GetSingletonRW<EconomyArmySupplyState>().ValueRW;
            ref var demography = ref SystemAPI.GetSingletonRW<ColonyDemographyState>().ValueRW;
            ref var tech = ref SystemAPI.GetSingletonRW<ColonyTechProgressState>().ValueRW;

            var stock = SystemAPI.GetSingletonBuffer<ResourceStockEntry>(ref state);
            var facilities = SystemAPI.GetSingletonBuffer<EconomyProductionFacilityEntry>(ref state);
            var generators = SystemAPI.GetSingletonBuffer<EconomyPowerGeneratorEntry>(ref state);
            var routes = SystemAPI.GetSingletonBuffer<EconomyTransportRouteEntry>(ref state);
            var warehouses = SystemAPI.GetSingletonBuffer<EconomyWarehouseEntry>(ref state);

            var tick = SystemAPI.GetSingleton<SimulationRootState>().SimulationTick;
            var phaseBefore = sim.Phase;
            AdvanceEconomyPhase(ref sim);
            if (phaseBefore != sim.Phase)
                TryEnqueueStoryEvent(ref state, tick, EventEconomyPhaseChanged, new FixedString64Bytes("economy-phase"));

            UpdateMilitaryModeAndTargetShare(ref sim, ref militaryIndustry);

            UpdateLogisticsState(ref logistics, ref routes, sim.Infrastructure01, facilities.Length);
            sim.LogisticsEfficiency01 = EconomySimulationMath.ComputeLogisticsEfficiency(
                1f - logistics.RouteComplexity01 * 0.35f,
                logistics.AverageRouteDistanceKm,
                500f,
                sim.Infrastructure01);
            sim.LogisticsEfficiency01 *= math.saturate(
                logistics.TotalTransportTonKmPerDay / math.max(1f, logistics.RequiredTonKmPerDay));
            sim.LogisticsEfficiency01 = math.clamp(sim.LogisticsEfficiency01, 0.1f, 1.25f);

            UpdateWarehouseState(ref warehouse, ref warehouses, ref stock);
            var armyUnits = SystemAPI.HasSingleton<MilitarySimulationState>()
                ? (int)SystemAPI.GetSingleton<MilitarySimulationState>().ActiveArmyUnits
                : _armyQuery.CalculateEntityCount();
            UpdateArmySupplyState(ref armySupply, ref stock, sim.Phase, armyUnits);
            sim.MilitaryPriority = ResolveMilitaryPriority(sim.Phase, armySupply.ArmySupplyAdequacy01, armyUnits,
                population: demography.Population);

            var armyAdequacyBefore = sim.MilitaryProductionShare01;
            sim.MilitaryProductionShare01 = ResolveMilitaryProductionShare(sim.Phase, armySupply.ArmySupplyAdequacy01);
            if (math.abs(sim.MilitaryProductionShare01 - armyAdequacyBefore) > 1e-3f)
                militaryIndustry.SwitchHoursRemaining = math.abs(sim.MilitaryProductionShare01 - armyAdequacyBefore) * 10f;
            militaryIndustry.SwitchHoursRemaining = math.max(0f, militaryIndustry.SwitchHoursRemaining - 24f);
            ApplyGlobalMilitaryModeToFacilities(ref facilities, sim.MilitaryProductionShare01);

            if (armySupply.ArmySupplyAdequacy01 < 0.5f)
                TryEnqueueStoryEvent(ref state, tick, EventArmySupplyCritical, new FixedString64Bytes("army-supply"));

            var demandKw = 0f;
            for (var i = 0; i < facilities.Length; i++)
            {
                if (facilities[i].EnergyRequiredKw > 0f)
                    demandKw += facilities[i].EnergyRequiredKw;
            }

            UpdateEnergyState(ref energy, ref generators, ref stock, demandKw);
            var globalEnergyRatio = energy.DemandKw > 1e-3f ? math.saturate(energy.DeliveredKw / energy.DemandKw) : 1f;

            var totalOutputValue = 0f;
            var totalOutputMassKg = 0f;
            var primaryValue = 0f;
            var secondaryValue = 0f;
            var tertiaryValue = 0f;
            militaryIndustry.CivilianOutputToday = 0f;
            militaryIndustry.MilitaryOutputToday = 0f;
            var activeFacilities = 0u;
            var resourceBlockedFacilities = 0u;

            for (var i = 0; i < facilities.Length; i++)
            {
                var f = facilities[i];
                var producedValue = ProcessFacility(ref f, ref stock, sim.LogisticsEfficiency01, globalEnergyRatio,
                    sim.MilitaryPriority, sim.MilitaryProductionShare01, out var producedMassKg, out var militaryValue, out var canRun);
                if (canRun)
                    activeFacilities++;
                else
                    resourceBlockedFacilities++;

                totalOutputValue += producedValue;
                totalOutputMassKg += producedMassKg;
                militaryIndustry.MilitaryOutputToday += militaryValue;
                militaryIndustry.CivilianOutputToday += math.max(0f, producedValue - militaryValue);

                AccumulateSectorValue(f.ActiveRecipe, producedValue, ref primaryValue, ref secondaryValue, ref tertiaryValue);
                facilities[i] = f;
            }

            logistics.RequiredTonKmPerDay = math.max(100f, totalOutputMassKg / 1000f *
                math.max(5f, logistics.AverageRouteDistanceKm) * 0.7f);

            sim.ProductionEfficiency01 = activeFacilities == 0u
                ? 0f
                : math.saturate(activeFacilities / math.max(1f, (float)facilities.Length)) *
                  math.saturate(globalEnergyRatio) *
                  math.saturate(sim.LogisticsEfficiency01);

            var population = math.max(1f, demography.Population);
            var assignedWorkers = CountAssignedWorkers(ref facilities) + CountWarehouseWorkers(ref warehouses);
            var laborPool = math.max(1f, population * 0.45f);
            var unemployment01 = math.saturate(1f - assignedWorkers / laborPool);

            var gdp = ComputeCurrentStockValue(ref stock);
            var inflation = math.clamp(
                (1f - globalEnergyRatio) * 10f +
                (1f - sim.LogisticsEfficiency01) * 12f +
                warehouse.Overload01 * 8f +
                (sim.Phase == EconomyCyclePhase.Warfare ? 5f : 0f) +
                (1f - armySupply.ArmySupplyAdequacy01) * 10f,
                0f,
                50f);
            var exports = math.max(0f, logistics.TotalTransportTonKmPerDay - logistics.RequiredTonKmPerDay) * 20f;
            var imports = math.max(0f, logistics.RequiredTonKmPerDay - logistics.TotalTransportTonKmPerDay) * 15f;
            var tradeBalance = exports - imports;

            sim.InflationPercent = inflation;
            sim.Unemployment01 = unemployment01;
            sim.ExportVolume = exports;
            sim.ImportVolume = imports;
            sim.TradeBalance = tradeBalance;

            if (SystemAPI.HasSingleton<AnalyticsLocalSnapshot>())
            {
                ref var snap = ref SystemAPI.GetSingletonRW<AnalyticsLocalSnapshot>().ValueRW;
                var prevGdp = snap.Economy.Gdp;
                snap.Economy.GdpPrevious = prevGdp;
                snap.Economy.Gdp = gdp;
                snap.Economy.GdpPerCapita = gdp / population;
                snap.Economy.GdpGrowthPercent = prevGdp > 1e-3f ? (gdp - prevGdp) / prevGdp * 100f : 0f;
                snap.Economy.InflationPercent = inflation;
                snap.Economy.UnemploymentRate01 = unemployment01;
                snap.Economy.ExportVolume = exports;
                snap.Economy.ImportVolume = imports;
                snap.Economy.TradeBalance = tradeBalance;

                var sectorTotal = primaryValue + secondaryValue + tertiaryValue;
                if (sectorTotal > 1e-3f)
                {
                    snap.Economy.PrimarySectorShare01 = primaryValue / sectorTotal;
                    snap.Economy.SecondarySectorShare01 = secondaryValue / sectorTotal;
                    snap.Economy.TertiarySectorShare01 = tertiaryValue / sectorTotal;
                }
            }

            var researchFromEconomy = EconomySimulationMath.ComputeResearchPointsFromEconomy(
                totalOutputValue, tech.ScientistsCount, exports + imports, 0.55f, 1f);
            tech.ResearchPointsPerDay = math.max(1f, researchFromEconomy);

            sim.Bottleneck = ResolveBottleneck(globalEnergyRatio, sim.LogisticsEfficiency01,
                assignedWorkers / laborPool, resourceBlockedFacilities, armySupply.ArmySupplyAdequacy01);
            if (sim.Bottleneck != EconomyBottleneckKind.None)
                TryEnqueueStoryEvent(ref state, tick, EventEconomyBottleneck, new FixedString64Bytes("economy-bottleneck"));

            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.EconomyGdp, gdp);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.EconomyGdpPerCapita, gdp / population);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.EconomyInflation, inflation);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.EconomyUnemployment, unemployment01);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.EconomyExportVolume, exports);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.EconomyImportVolume, imports);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.EconomyTradeBalance, tradeBalance);

            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.EconomyPowerGeneratedKw, energy.GeneratedKw);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.EconomyPowerDemandKw, energy.DemandKw);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.EconomyPowerLossPercent,
                energy.TransmissionLoss01 * 100f);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.EconomyLogisticsCapacityTonKm,
                logistics.TotalTransportTonKmPerDay);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.EconomyLogisticsRequiredTonKm,
                logistics.RequiredTonKmPerDay);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.EconomyWarehouseUtilization01,
                warehouse.TotalCapacityKg > 1e-3f ? warehouse.UsedCapacityKg / warehouse.TotalCapacityKg : 0f);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.EconomyMilitaryProductionShare01,
                sim.MilitaryProductionShare01);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.EconomyArmySupplyAdequacy01,
                armySupply.ArmySupplyAdequacy01);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.EconomyCurrentCyclePhase, (byte)sim.Phase);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.EconomyResearchPointsFromEconomy,
                researchFromEconomy);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.EconomyActiveFacilities, activeFacilities);
        }

        private static void AdvanceEconomyPhase(ref EconomySimulationState sim)
        {
            sim.DaysInPhase++;
            var duration = EconomySimulationMath.GetPhaseDurationDays(sim.Phase);
            if (sim.DaysInPhase < duration)
                return;

            sim.Phase = EconomySimulationMath.GetNextPhase(sim.Phase);
            sim.DaysInPhase = 0;
        }

        private static void UpdateMilitaryModeAndTargetShare(ref EconomySimulationState sim,
            ref EconomyMilitaryIndustryState militaryIndustry)
        {
            var target = ResolveMilitaryProductionShare(sim.Phase, 1f);
            var delta = math.abs(target - sim.MilitaryProductionShare01);
            if (delta > 1e-4f)
                militaryIndustry.SwitchHoursRemaining = delta * 10f;
        }

        private static float ResolveMilitaryProductionShare(EconomyCyclePhase phase, float supplyAdequacy01)
        {
            var baseShare = phase switch
            {
                EconomyCyclePhase.Accumulation => 0.10f,
                EconomyCyclePhase.Expansion => 0.15f,
                EconomyCyclePhase.Preparation => 0.50f,
                EconomyCyclePhase.Warfare => 1f,
                EconomyCyclePhase.Recovery => 0.25f,
                _ => 0.2f
            };

            if (supplyAdequacy01 < 0.5f)
                baseShare *= 0.7f;
            return math.saturate(baseShare);
        }

        private static EconomyProductionPriority ResolveMilitaryPriority(EconomyCyclePhase phase, float supplyAdequacy01, int armyUnits,
            uint population)
        {
            var desiredArmy = math.max(1f, population * 0.02f);
            var lossesPercent = math.saturate((desiredArmy - armyUnits) / desiredArmy);
            if (supplyAdequacy01 < 0.45f)
                return EconomyProductionPriority.Low;
            if (lossesPercent > 0.2f)
                return EconomyProductionPriority.Critical;
            if (phase == EconomyCyclePhase.Preparation || phase == EconomyCyclePhase.Warfare)
                return EconomyProductionPriority.High;
            if (phase == EconomyCyclePhase.Recovery)
                return EconomyProductionPriority.Normal;
            return EconomyProductionPriority.Minimal;
        }

        private static void ApplyGlobalMilitaryModeToFacilities(ref DynamicBuffer<EconomyProductionFacilityEntry> facilities,
            float militaryShare01)
        {
            EconomyMilitaryProductionMode mode;
            if (militaryShare01 >= 0.95f)
                mode = EconomyMilitaryProductionMode.Military;
            else if (militaryShare01 <= 0.05f)
                mode = EconomyMilitaryProductionMode.Peace;
            else
                mode = EconomyMilitaryProductionMode.Mixed;

            for (var i = 0; i < facilities.Length; i++)
            {
                var f = facilities[i];
                f.MilitaryMode = mode;
                f.MilitaryShare01 = militaryShare01;
                facilities[i] = f;
            }
        }

        private static void UpdateLogisticsState(ref EconomyLogisticsState logistics, ref DynamicBuffer<EconomyTransportRouteEntry> routes,
            float infrastructure01, int facilityCount)
        {
            var capacity = 0f;
            var distanceWeighted = 0f;
            var distanceWeight = 0f;
            var conveyors = 0f;
            for (var i = 0; i < routes.Length; i++)
            {
                var r = routes[i];
                var infra = 0.8f + 0.4f * math.saturate((infrastructure01 + r.InfrastructureFactor01) * 0.5f);
                var routeTonKm = EconomySimulationMath.ComputeTransportTonKmPerDay(
                    r.PayloadKg,
                    r.SpeedKmPerHour * infra,
                    r.LoadCoefficient01);
                capacity += routeTonKm;
                distanceWeighted += r.DistanceKm * routeTonKm;
                distanceWeight += routeTonKm;
                if (r.Kind == EconomyTransportKind.Conveyor)
                    conveyors += (r.PayloadKg * r.SpeedKmPerHour * 1000f / 3600f) * math.saturate(r.LoadCoefficient01);
            }

            logistics.TotalTransportTonKmPerDay = capacity;
            logistics.AverageRouteDistanceKm = distanceWeight > 1e-3f ? distanceWeighted / distanceWeight : 0f;
            logistics.ConveyorThroughputKgPerSecond = conveyors;
            logistics.RequiredTonKmPerDay = math.max(100f, facilityCount * 250f);
        }

        private static void UpdateEnergyState(ref EconomyEnergyState energy, ref DynamicBuffer<EconomyPowerGeneratorEntry> generators,
            ref DynamicBuffer<ResourceStockEntry> stock, float demandKw)
        {
            var generatedKw = 0f;
            for (var i = 0; i < generators.Length; i++)
            {
                var g = generators[i];
                var outputScale = math.saturate(g.OutputScale01);
                var outputKw = g.OutputKw * outputScale;
                if (g.FuelResource != ResourceId.None && g.FuelPerDayAtFullLoad > 1e-3f)
                {
                    var fuelNeed = g.FuelPerDayAtFullLoad * outputScale;
                    var fuelAvailable = ResourceStockpileOps.GetAmount(ref stock, g.FuelResource);
                    var fuelRatio = math.saturate(fuelAvailable / fuelNeed);
                    if (fuelRatio <= 1e-4f)
                    {
                        generators[i] = g;
                        continue;
                    }

                    ResourceStockpileOps.TryConsume(ref stock, g.FuelResource, fuelNeed * fuelRatio);
                    outputKw *= fuelRatio;
                }

                generatedKw += outputKw;
                generators[i] = g;
            }

            var demand = math.max(0f, demandKw);
            var loss01 = EconomySimulationMath.ComputeTransmissionLoss01(energy.TransmissionDistanceKm);
            var generatedKwh = generatedKw * 24f;
            var deliveredKwh = generatedKwh * (1f - loss01);
            var demandKwh = demand * 24f;

            if (deliveredKwh > demandKwh)
            {
                var surplus = deliveredKwh - demandKwh;
                var chargeRoom = math.max(0f, energy.StorageCapacityKwh - energy.StorageChargeKwh);
                var charged = math.min(chargeRoom, surplus * math.saturate(energy.StorageRoundTripEfficiency01));
                energy.StorageChargeKwh += charged;
                deliveredKwh = demandKwh;
            }
            else if (deliveredKwh < demandKwh && energy.StorageChargeKwh > 0f)
            {
                var deficit = demandKwh - deliveredKwh;
                var discharge = math.min(energy.StorageChargeKwh, deficit);
                energy.StorageChargeKwh -= discharge;
                deliveredKwh += discharge;
            }

            energy.GeneratedKw = generatedKw;
            energy.DemandKw = demand;
            energy.DeliveredKw = deliveredKwh / 24f;
            energy.TransmissionLoss01 = loss01;
        }

        private static void UpdateWarehouseState(ref EconomyWarehouseState warehouse,
            ref DynamicBuffer<EconomyWarehouseEntry> entries, ref DynamicBuffer<ResourceStockEntry> stock)
        {
            var totalCapacity = 0f;
            var processingSeconds = 0f;
            var processingWeight = 0f;
            var totalUsed = 0f;
            for (var i = 0; i < entries.Length; i++)
            {
                var e = entries[i];
                totalCapacity += e.CapacityKg;
                var seconds = EconomySimulationMath.ComputeWarehouseProcessingSecondsPerTon(
                    e.Workers, e.Automation01, e.Organization01, false);
                processingSeconds += seconds * e.CapacityKg;
                processingWeight += e.CapacityKg;
            }

            for (var i = 0; i < stock.Length; i++)
            {
                totalUsed += GetEstimatedMassKg(stock[i].Id, stock[i].Amount);
            }

            warehouse.TotalCapacityKg = totalCapacity;
            warehouse.UsedCapacityKg = totalUsed;
            warehouse.Overload01 = totalCapacity > 1e-3f
                ? math.max(0f, (totalUsed - totalCapacity) / totalCapacity)
                : 0f;
            warehouse.ProcessingSecondsPerTon = processingWeight > 1e-3f ? processingSeconds / processingWeight : 120f;
            warehouse.InventoryDriftPercentPerDay = math.max(0.1f, 0.5f + warehouse.Overload01 * 1.5f);

            if (warehouse.Overload01 > 0f)
            {
                var spoilage = warehouse.Overload01 * 5f;
                ResourceStockpileOps.TryConsume(ref stock, ResourceId.CropWheat, spoilage);
                ResourceStockpileOps.TryConsume(ref stock, ResourceId.LivestockMeat, spoilage * 0.5f);
                ResourceStockpileOps.TryConsume(ref stock, ResourceId.FishCatch, spoilage * 0.4f);
            }
        }

        private static void UpdateArmySupplyState(ref EconomyArmySupplyState supply, ref DynamicBuffer<ResourceStockEntry> stock,
            EconomyCyclePhase phase, int armyUnits)
        {
            var thousand = math.max(0.2f, armyUnits / 10f);
            var battleFactor = phase == EconomyCyclePhase.Warfare ? 1.7f : 1f;
            var winterFactor = phase == EconomyCyclePhase.Recovery ? 1.2f : 1f;

            supply.ProvisionsNeedKgPerDay = 3000f * thousand * battleFactor * winterFactor;
            supply.FuelNeedLitersPerDay = 2000f * thousand * battleFactor;
            supply.AmmunitionNeedKgPerDay = (phase == EconomyCyclePhase.Warfare ? 5000f : 500f) * thousand;
            supply.SparePartsNeedKgPerDay = (phase == EconomyCyclePhase.Warfare ? 500f : 100f) * thousand;
            supply.MedicalNeedKgPerDay = (phase == EconomyCyclePhase.Warfare ? 200f : 50f) * thousand;

            var provisions = ConsumeFromPool(ref stock, supply.ProvisionsNeedKgPerDay,
                ResourceId.CropWheat, ResourceId.LivestockMeat, ResourceId.FishCatch);
            var fuel = ConsumeFromPool(ref stock, supply.FuelNeedLitersPerDay,
                ResourceId.PetroleumProducts, ResourceId.SyntheticFuel, ResourceId.HighOctaneGasoline);
            var ammo = ConsumeFromPool(ref stock, supply.AmmunitionNeedKgPerDay,
                ResourceId.Gunpowder, ResourceId.ArtilleryShellExplosiveEpoch2, ResourceId.HandGrenadeWW1,
                ResourceId.LandMineWW1, ResourceId.ChemicalArtilleryShellWW1);
            var spare = ConsumeFromPool(ref stock, supply.SparePartsNeedKgPerDay,
                ResourceId.SteelIndustrial, ResourceId.SteelAlloyed, ResourceId.SteelArmorPiercing);
            var med = ConsumeFromPool(ref stock, supply.MedicalNeedKgPerDay,
                ResourceId.ChemicalReagents, ResourceId.Cloth, ResourceId.Leather);

            var provAdeq = supply.ProvisionsNeedKgPerDay > 1e-3f ? provisions / supply.ProvisionsNeedKgPerDay : 1f;
            var fuelAdeq = supply.FuelNeedLitersPerDay > 1e-3f ? fuel / supply.FuelNeedLitersPerDay : 1f;
            var ammoAdeq = supply.AmmunitionNeedKgPerDay > 1e-3f ? ammo / supply.AmmunitionNeedKgPerDay : 1f;
            var spareAdeq = supply.SparePartsNeedKgPerDay > 1e-3f ? spare / supply.SparePartsNeedKgPerDay : 1f;
            var medAdeq = supply.MedicalNeedKgPerDay > 1e-3f ? med / supply.MedicalNeedKgPerDay : 1f;

            supply.ArmySupplyAdequacy01 = math.saturate(math.min(math.min(provAdeq, fuelAdeq), math.min(ammoAdeq, math.min(spareAdeq, medAdeq))));
        }

        private static float ProcessFacility(ref EconomyProductionFacilityEntry facility, ref DynamicBuffer<ResourceStockEntry> stock,
            float logisticsEfficiency01, float globalEnergyRatio, EconomyProductionPriority globalMilitaryPriority,
            float globalMilitaryShare01, out float producedMassKg, out float militaryValue, out bool canRun)
        {
            producedMassKg = 0f;
            militaryValue = 0f;
            canRun = false;

            var def = ProductionRecipeCatalog.Get(facility.ActiveRecipe);
            if (def.Id == ProductionRecipeId.None || def.DurationSeconds <= 0f)
            {
                IncreaseWearAndDegradeTools(ref facility, false);
                return 0f;
            }

            var recipeIsMilitary = IsMilitaryRecipe(def.Output);
            var militaryShare = facility.MilitaryMode switch
            {
                EconomyMilitaryProductionMode.Military => 1f,
                EconomyMilitaryProductionMode.Mixed => math.saturate(globalMilitaryShare01),
                _ => 0f
            };

            float modeShare;
            if (recipeIsMilitary)
                modeShare = facility.MilitaryMode == EconomyMilitaryProductionMode.Peace ? 0f : math.max(0.05f, militaryShare);
            else
                modeShare = facility.MilitaryMode == EconomyMilitaryProductionMode.Military ? 0f : 1f - militaryShare;

            if (modeShare <= 1e-4f)
            {
                IncreaseWearAndDegradeTools(ref facility, false);
                return 0f;
            }

            var energyRatio = facility.EnergyRequiredKw > 1e-3f ? globalEnergyRatio : 1f;
            var efficiency = EconomySimulationMath.ComputeBuildingEfficiency(
                facility.BaseSpeedMultiplier,
                facility.AssignedWorkers,
                facility.OptimalWorkers,
                energyRatio,
                facility.BuildingWear01,
                facility.AverageSkill0To100,
                facility.MasterCount);
            efficiency *= EconomySimulationMath.GetUpgradeProductionMultiplier(facility.UpgradeLevel);
            efficiency *= math.max(0.05f, logisticsEfficiency01);

            if (facility.MilitaryMode == EconomyMilitaryProductionMode.Mixed)
                efficiency *= EconomySimulationMath.GetMixedModeEfficiencyPenalty(militaryShare);

            if (recipeIsMilitary)
            {
                var effectivePriority = (EconomyProductionPriority)math.min((int)facility.Priority, (int)globalMilitaryPriority);
                var prioritySpeed = EconomySimulationMath.GetMilitaryPrioritySpeedMultiplier(effectivePriority);
                var priorityEff = EconomySimulationMath.GetMilitaryPriorityEfficiencyMultiplier(effectivePriority);
                efficiency *= prioritySpeed * priorityEff;
            }

            var cyclesPerDayBase = 86400f / def.DurationSeconds;
            var targetCycles = math.max(0f, cyclesPerDayBase * efficiency * modeShare);
            if (targetCycles <= 1e-4f)
            {
                IncreaseWearAndDegradeTools(ref facility, false);
                return 0f;
            }

            var byIn0 = GetCyclesByInput(ref stock, def.In0, def.Amount0);
            var byIn1 = GetCyclesByInput(ref stock, def.In1, def.Amount1);
            var byIn2 = GetCyclesByInput(ref stock, def.In2, def.Amount2);
            var actualCycles = math.min(targetCycles, math.min(byIn0, math.min(byIn1, byIn2)));
            if (actualCycles <= 1e-4f)
            {
                IncreaseWearAndDegradeTools(ref facility, false);
                return 0f;
            }

            canRun = true;
            ConsumeInputForCycles(ref stock, def.In0, def.Amount0, actualCycles);
            ConsumeInputForCycles(ref stock, def.In1, def.Amount1, actualCycles);
            ConsumeInputForCycles(ref stock, def.In2, def.Amount2, actualCycles);

            var outputAmount = def.OutputAmount * actualCycles;
            ResourceStockpileOps.Add(ref stock, def.Output, outputAmount);

            if (ResourceCatalog.TryGet(def.Output, out var outputDef))
            {
                var value = outputAmount * outputDef.BasePrice;
                producedMassKg = GetEstimatedMassKg(def.Output, outputAmount);
                militaryValue = recipeIsMilitary ? value : 0f;
                IncreaseWearAndDegradeTools(ref facility, true);
                TryRepairFacility(ref facility, ref stock);
                return value;
            }

            IncreaseWearAndDegradeTools(ref facility, true);
            TryRepairFacility(ref facility, ref stock);
            return 0f;
        }

        private static void IncreaseWearAndDegradeTools(ref EconomyProductionFacilityEntry facility, bool active)
        {
            var wearPerHour = EconomySimulationMath.GetFacilityWearPerHour(facility.Kind, facility.Era);
            var wearDelta01 = wearPerHour * 24f / 100f * (active ? 1f : 0.25f);
            facility.BuildingWear01 = math.saturate(facility.BuildingWear01 + wearDelta01);

            var toolDelta = active ? 0.03f : 0.005f;
            facility.ToolCondition01 = math.clamp(facility.ToolCondition01 - toolDelta, 0.2f, 1f);
        }

        private static void TryRepairFacility(ref EconomyProductionFacilityEntry facility, ref DynamicBuffer<ResourceStockEntry> stock)
        {
            if (facility.BuildingWear01 > 0.75f &&
                ResourceStockpileOps.HasAtLeast(ref stock, ResourceId.SteelIndustrial, 4f) &&
                ResourceStockpileOps.HasAtLeast(ref stock, ResourceId.Epoch1Tools, 1f))
            {
                ResourceStockpileOps.TryConsume(ref stock, ResourceId.SteelIndustrial, 4f);
                ResourceStockpileOps.TryConsume(ref stock, ResourceId.Epoch1Tools, 1f);
                facility.BuildingWear01 = math.max(0f, facility.BuildingWear01 - 0.25f);
            }

            if (facility.ToolCondition01 < 0.35f && ResourceStockpileOps.HasAtLeast(ref stock, ResourceId.Epoch1Tools, 1f))
            {
                ResourceStockpileOps.TryConsume(ref stock, ResourceId.Epoch1Tools, 1f);
                facility.ToolCondition01 = math.min(1f, facility.ToolCondition01 + 0.3f);
            }
        }

        private static float GetCyclesByInput(ref DynamicBuffer<ResourceStockEntry> stock, ResourceId id, float amountPerCycle)
        {
            if (id == ResourceId.None || amountPerCycle <= 0f)
                return float.MaxValue;
            var amount = ResourceStockpileOps.GetAmount(ref stock, id);
            return amount / amountPerCycle;
        }

        private static void ConsumeInputForCycles(ref DynamicBuffer<ResourceStockEntry> stock, ResourceId id, float amountPerCycle,
            float cycles)
        {
            if (id == ResourceId.None || amountPerCycle <= 0f || cycles <= 0f)
                return;
            ResourceStockpileOps.TryConsume(ref stock, id, amountPerCycle * cycles);
        }

        private static bool IsMilitaryRecipe(ResourceId output)
        {
            return output is ResourceId.MusketFirearm
                or ResourceId.PikeWeapon
                or ResourceId.BronzeCannonEpoch1
                or ResourceId.MilitaryRifleEpoch2
                or ResourceId.RevolverEpoch2
                or ResourceId.ArtilleryShellExplosiveEpoch2
                or ResourceId.HandGrenadeWW1
                or ResourceId.LandMineWW1
                or ResourceId.ChemicalArtilleryShellWW1
                or ResourceId.MilitaryRifleMagazineWW1
                or ResourceId.Gunpowder;
        }

        private static void AccumulateSectorValue(ProductionRecipeId recipeId, float value, ref float primary, ref float secondary,
            ref float tertiary)
        {
            var def = ProductionRecipeCatalog.Get(recipeId);
            if (def.Id == ProductionRecipeId.None || value <= 0f)
                return;
            if (!ResourceCatalog.TryGet(def.Output, out var outputDef))
                return;

            switch (outputDef.Category)
            {
                case ResourceCategory.Raw:
                case ResourceCategory.Material:
                    primary += value;
                    break;
                case ResourceCategory.Processed:
                case ResourceCategory.Component:
                    secondary += value;
                    break;
                case ResourceCategory.FinalProduct:
                    tertiary += value;
                    break;
            }
        }

        private static float ConsumeFromPool(ref DynamicBuffer<ResourceStockEntry> stock, float need,
            ResourceId a, ResourceId b = ResourceId.None, ResourceId c = ResourceId.None, ResourceId d = ResourceId.None,
            ResourceId e = ResourceId.None)
        {
            var remaining = math.max(0f, need);
            var consumed = 0f;
            ConsumeById(ref stock, a, ref remaining, ref consumed);
            ConsumeById(ref stock, b, ref remaining, ref consumed);
            ConsumeById(ref stock, c, ref remaining, ref consumed);
            ConsumeById(ref stock, d, ref remaining, ref consumed);
            ConsumeById(ref stock, e, ref remaining, ref consumed);
            return consumed;
        }

        private static void ConsumeById(ref DynamicBuffer<ResourceStockEntry> stock, ResourceId id, ref float remaining, ref float consumed)
        {
            if (id == ResourceId.None || remaining <= 1e-4f)
                return;
            var available = ResourceStockpileOps.GetAmount(ref stock, id);
            if (available <= 1e-4f)
                return;
            var take = math.min(available, remaining);
            if (take <= 1e-4f)
                return;
            ResourceStockpileOps.TryConsume(ref stock, id, take);
            consumed += take;
            remaining -= take;
        }

        private static float GetEstimatedMassKg(ResourceId id, float amount)
        {
            if (amount <= 0f)
                return 0f;
            if (!ResourceCatalog.TryGet(id, out var def))
                return amount;
            var unitMass = def.Category switch
            {
                ResourceCategory.Raw => 10f,
                ResourceCategory.Processed => 8f,
                ResourceCategory.Material => 6f,
                ResourceCategory.Component => 3f,
                ResourceCategory.FinalProduct => 2f,
                _ => 5f
            };
            return amount * unitMass;
        }

        private static float ComputeCurrentStockValue(ref DynamicBuffer<ResourceStockEntry> stock)
        {
            var total = 0f;
            for (var i = 0; i < stock.Length; i++)
            {
                var entry = stock[i];
                if (!ResourceCatalog.TryGet(entry.Id, out var def))
                    continue;
                total += entry.Amount * def.BasePrice;
            }

            return total;
        }

        private static float CountAssignedWorkers(ref DynamicBuffer<EconomyProductionFacilityEntry> facilities)
        {
            var workers = 0f;
            for (var i = 0; i < facilities.Length; i++)
                workers += facilities[i].AssignedWorkers;
            return workers;
        }

        private static float CountWarehouseWorkers(ref DynamicBuffer<EconomyWarehouseEntry> warehouses)
        {
            var workers = 0f;
            for (var i = 0; i < warehouses.Length; i++)
                workers += warehouses[i].Workers;
            return workers;
        }

        private static EconomyBottleneckKind ResolveBottleneck(float energyRatio01, float logisticsEfficiency01,
            float workforceLoad01, uint resourceBlockedFacilities, float armySupplyAdequacy01)
        {
            if (energyRatio01 < 0.6f)
                return EconomyBottleneckKind.EnergySupply;
            if (logisticsEfficiency01 < 0.6f)
                return EconomyBottleneckKind.Logistics;
            if (workforceLoad01 > 1.05f)
                return EconomyBottleneckKind.Workforce;
            if (resourceBlockedFacilities > 0)
                return EconomyBottleneckKind.ResourceExtraction;
            if (armySupplyAdequacy01 < 0.6f)
                return EconomyBottleneckKind.MilitaryProduction;
            return EconomyBottleneckKind.None;
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
