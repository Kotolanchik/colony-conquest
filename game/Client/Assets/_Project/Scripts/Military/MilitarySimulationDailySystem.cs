using ColonyConquest.Analytics;
using ColonyConquest.Core;
using ColonyConquest.Defense;
using ColonyConquest.Ecology;
using ColonyConquest.Economy;
using ColonyConquest.Manufacturing;
using ColonyConquest.Settlers;
using ColonyConquest.Simulation;
using ColonyConquest.Story;
using ColonyConquest.WorldMap;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ColonyConquest.Military
{
    /// <summary>Суточная military-симуляция: погода, приказы, боестолкновения, потери, снабжение, мета-юниты.</summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(ColonyCalendarAdvanceSystem))]
    [UpdateAfter(typeof(EconomySimulationDailySystem))]
    [UpdateAfter(typeof(ManufacturingSimulationDailySystem))]
    [UpdateBefore(typeof(AnalyticsSnapshotUpdateSystem))]
    public partial struct MilitarySimulationDailySystem : ISystem
    {
        private const uint EventBattleStarted = 0xEA01;
        private const uint EventHeavyCasualties = 0xEA02;
        private const uint EventSupplyCollapse = 0xEA03;
        private const uint EventMoralePanic = 0xEA04;
        private const uint EventOrderDelivered = 0xEA05;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameCalendarState>();
            state.RequireForUpdate<SimulationRootState>();
            state.RequireForUpdate<ResourceStockpileSingleton>();
            state.RequireForUpdate<MilitarySimulationSingleton>();
            state.RequireForUpdate<MilitarySimulationState>();
            state.RequireForUpdate<MilitaryEnvironmentState>();
            state.RequireForUpdate<MilitaryCommandRelayState>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var day = SystemAPI.GetSingleton<GameCalendarState>().DayIndex;
            var tick = SystemAPI.GetSingleton<SimulationRootState>().SimulationTick;
            ref var sim = ref SystemAPI.GetSingletonRW<MilitarySimulationState>().ValueRW;
            if (sim.LastProcessedDay == day)
                return;
            sim.LastProcessedDay = day;

            ref var env = ref SystemAPI.GetSingletonRW<MilitaryEnvironmentState>().ValueRW;
            ref var relay = ref SystemAPI.GetSingletonRW<MilitaryCommandRelayState>().ValueRW;
            var stock = SystemAPI.GetSingletonBuffer<ResourceStockEntry>(ref state);
            var formations = SystemAPI.GetSingletonBuffer<MilitaryFormationEntry>(ref state);
            var orders = SystemAPI.GetSingletonBuffer<MilitaryOperationOrderEntry>(ref state);
            var meta = SystemAPI.GetSingletonBuffer<MilitaryMetaUnitEntry>(ref state);

            var supplyAdequacy = SystemAPI.HasSingleton<EconomyArmySupplyState>()
                ? math.saturate(SystemAPI.GetSingleton<EconomyArmySupplyState>().ArmySupplyAdequacy01)
                : 0.8f;
            sim.SupplyAdequacy01 = supplyAdequacy;

            var operationHour = (byte)((day * 7u + 6u) % 24u);
            var isNight = operationHour >= 20 || operationHour < 6;
            var climateRisk = 0f;
            if (SystemAPI.HasSingleton<EcologySimulationState>())
                climateRisk = math.saturate(SystemAPI.GetSingleton<EcologySimulationState>().ExtremeWeatherRisk01);

            var dayRng = Random.CreateFromIndex(day + 0xC0FFEEu);
            var weather = ResolveWeather(dayRng.NextFloat(), climateRisk);
            MilitarySimulationMath.GetWeatherProfile(weather, out var visibility01, out var weatherAccuracy,
                out var movementMult, out var vehicleMult, out var commPenalty, out var suppressionMult, out var severity01);

            env.Weather = weather;
            env.OperationHour = operationHour;
            env.IsNightOperation = isNight ? (byte)1 : (byte)0;
            env.Visibility01 = visibility01;
            env.AccuracyMultiplier = weatherAccuracy;
            env.MovementMultiplier = movementMult;
            env.VehicleMobilityMultiplier = vehicleMult;
            env.CommunicationPenalty01 = commPenalty;
            env.NightPenalty01 = isNight ? 0.6f : 0f;
            env.SuppressionModifier = suppressionMult;
            env.WeatherSeverity01 = severity01;

            relay.RadioInterference01 = math.saturate(0.1f + commPenalty * 0.9f + climateRisk * 0.2f);
            if (relay.CommanderLossPenaltyMinutes > 0f)
                relay.CommanderLossPenaltyMinutes = math.max(0f, relay.CommanderLossPenaltyMinutes - 4f);

            var offensivePressure = ResolveOffensivePressure(ref state, in sim, supplyAdequacy);
            var ordersInTransit = 0u;
            for (var i = 0; i < orders.Length; i++)
            {
                var order = orders[i];
                if (order.IsExecuted != 0 || order.IsFailed != 0)
                    continue;

                var formation = FindFormation(ref formations, order.FormationId);
                var reaction = formation.ReactionTimeSeconds <= 0f ? 30f : formation.ReactionTimeSeconds;
                var hasRadio = formation.HasRadio != 0;
                if (order.DelayMinutesRemaining <= 0f)
                {
                    order.DelayMinutesRemaining = MilitarySimulationMath.ComputeOrderDelayMinutes(
                        order.DistanceKm,
                        reaction,
                        hasRadio,
                        relay.RadioInterference01,
                        env.CommunicationPenalty01,
                        env.NightPenalty01,
                        relay.CommanderLossPenaltyMinutes) + relay.BaseDelayMinutes;
                    order.IsAcknowledged = 1;
                }

                var progression = 180f * math.max(0.2f, 1f - env.CommunicationPenalty01 * 0.6f);
                order.DelayMinutesRemaining -= progression;
                order.ExpireAfterMinutes -= progression;
                if (order.ExpireAfterMinutes <= 0f)
                {
                    order.IsFailed = 1;
                }
                else if (order.DelayMinutesRemaining <= 0f)
                {
                    order.IsExecuted = 1;
                    TryEnqueueStoryEvent(ref state, tick, EventOrderDelivered, new FixedString64Bytes("mil-order-delivered"));
                    if (order.Type == OrderType.Attack)
                        offensivePressure = math.min(1.3f, offensivePressure + 0.1f);
                }
                else
                {
                    ordersInTransit++;
                }

                orders[i] = order;
            }

            var defensiveCoverBonus = 0f;
            if (SystemAPI.HasSingleton<DefensiveSimulationSingleton>())
            {
                var structures = SystemAPI.GetSingletonBuffer<DefensiveStructureRuntimeEntry>(ref state);
                var supportCount = 0f;
                for (var i = 0; i < structures.Length; i++)
                {
                    if (structures[i].IsOperational == 0)
                        continue;
                    supportCount += 1f;
                    defensiveCoverBonus += structures[i].DefenseBonusPercent * 0.0015f;
                }

                if (supportCount > 1e-3f)
                    defensiveCoverBonus /= supportCount;
            }

            var colonyMorale01 = 0.6f;
            if (SystemAPI.HasSingleton<SettlerSimulationState>())
                colonyMorale01 = math.saturate(SystemAPI.GetSingleton<SettlerSimulationState>().ColonyMorale01);

            var medicsAlive = 0u;
            foreach (var runtime in SystemAPI.Query<RefRO<MilitaryUnitRuntimeState>>().WithAll<BattleUnitTag>())
            {
                if (runtime.ValueRO.IsAlive == 0)
                    continue;
                if (runtime.ValueRO.UnitType == MilitaryUnitType.Medic)
                    medicsAlive++;
            }

            var active = 0u;
            var wounded = 0u;
            var killedToday = 0u;
            var woundedToday = 0u;
            var friendlyEquipmentLostToday = 0u;
            var enemyKilledToday = 0f;
            var enemyEquipmentLostToday = 0f;
            var moralePanicToday = 0u;
            var moraleSum = 0f;
            var suppressionSum = 0f;
            var fatigueSum = 0f;
            var readinessSum = 0f;

            using var metaCounts = new NativeArray<uint>(32, Allocator.Temp);
            using var metaHealthSum = new NativeArray<float>(32, Allocator.Temp);
            using var metaMoraleSum = new NativeArray<float>(32, Allocator.Temp);
            using var metaAmmoSum = new NativeArray<float>(32, Allocator.Temp);
            using var metaFuelSum = new NativeArray<float>(32, Allocator.Temp);
            using var metaPosSum = new NativeArray<float3>(32, Allocator.Temp);

            var em = state.EntityManager;
            foreach (var (runtimeRef, transformRef, entity) in
                     SystemAPI.Query<RefRW<MilitaryUnitRuntimeState>, RefRO<LocalTransform>>()
                         .WithAll<BattleUnitTag>()
                         .WithEntityAccess())
            {
                ref var runtime = ref runtimeRef.ValueRW;
                var combat = em.HasComponent<CombatStats>(entity) ? em.GetComponentData<CombatStats>(entity) : default;
                var order = em.HasComponent<MilitaryOrder>(entity) ? em.GetComponentData<MilitaryOrder>(entity) : default;
                var ai = em.HasComponent<MilitaryAIState>(entity) ? em.GetComponentData<MilitaryAIState>(entity) : default;
                var hierarchy = em.HasComponent<CommandHierarchy>(entity)
                    ? em.GetComponentData<CommandHierarchy>(entity)
                    : default;
                var visual = em.HasComponent<MilitaryVisualState>(entity)
                    ? em.GetComponentData<MilitaryVisualState>(entity)
                    : default;
                var cover = em.HasComponent<MilitaryCoverState>(entity) ? em.GetComponentData<MilitaryCoverState>(entity) : default;
                var wound = em.HasComponent<WoundedState>(entity) ? em.GetComponentData<WoundedState>(entity) : default;

                if (runtime.IsAlive == 0)
                    continue;

                active++;
                var unitSeed = math.max(1u, day * 73856093u + runtime.UnitId * 19349663u + 0x9E3779B9u);
                var rng = Random.CreateFromIndex(unitSeed);
                var isAttackingOrder = order.Type == OrderType.Attack;
                var engageChance = math.saturate(offensivePressure * 0.7f + (isAttackingOrder ? 0.25f : 0f));
                runtime.IsEngaged = rng.NextFloat() < engageChance ? (byte)1 : (byte)0;

                var qualityDrop = env.WeatherSeverity01 * 0.15f;
                cover.QualityMultiplier = math.clamp(cover.QualityMultiplier - qualityDrop + 0.03f, 0.65f, 1.35f);
                cover.BaseProtection01 = math.clamp(cover.BaseProtection01 + defensiveCoverBonus, 0f, 0.95f);
                cover.EffectiveProtection01 = MilitarySimulationMath.ComputeEffectiveCoverProtection(
                    cover.BaseProtection01,
                    cover.QualityMultiplier,
                    cover.OccupantState01,
                    cover.DirectionFactor01);

                var distanceMeters = math.distance(transformRef.ValueRO.Position, order.TargetPosition);
                runtime.CommandDelayMinutes = MilitarySimulationMath.ComputeOrderDelayMinutes(
                    distanceMeters / 1000f,
                    hierarchy.ReactionTimeSeconds,
                    hierarchy.HasRadio,
                    relay.RadioInterference01,
                    env.CommunicationPenalty01,
                    env.NightPenalty01,
                    relay.CommanderLossPenaltyMinutes);

                var restState = order.Type == OrderType.HoldPosition && runtime.IsEngaged == 0;
                var activityRate = restState
                    ? 0f
                    : runtime.IsEngaged != 0
                        ? 15f
                        : order.Type switch
                        {
                            OrderType.Move => 2f,
                            OrderType.Attack => 8f,
                            OrderType.Retreat => 10f,
                            _ => 1f
                        };
                var loadMultiplier = runtime.IsVehicle != 0 ? 0.8f : 1.15f;
                var terrainMultiplier = 1f + env.WeatherSeverity01 * 0.2f;
                var weatherMultiplier = 1f + env.WeatherSeverity01 * 0.4f;
                var medicalSupport01 = math.saturate(medicsAlive / math.max(1f, active * 0.12f));
                var fatigueDelta = MilitarySimulationMath.ComputeFatigueDelta(activityRate, loadMultiplier, terrainMultiplier,
                    weatherMultiplier, restState, 1f, supplyAdequacy, medicalSupport01);
                runtime.Fatigue = math.clamp(runtime.Fatigue + fatigueDelta * 0.1f, 0f, 100f);

                var suppressionGain = MilitarySimulationMath.ComputeSuppressionGain(
                    baseSuppression: runtime.IsEngaged != 0 ? 15f : 4f,
                    suppressionTypeMultiplier: env.SuppressionModifier,
                    proximityMultiplier: 1f + offensivePressure * 0.4f,
                    coverReduction: cover.EffectiveProtection01,
                    moraleResistance: math.saturate(runtime.Morale / 140f));
                var decayPerSec = MilitarySimulationMath.ComputeSuppressionDecayPerSecond(
                    inCombat: runtime.IsEngaged != 0,
                    underFire: runtime.IsEngaged != 0 && offensivePressure > 0.7f);
                runtime.Suppression = math.clamp(runtime.Suppression + suppressionGain - decayPerSec * 0.05f, 0f, 120f);

                var commanderBonus = hierarchy.MoraleBonus;
                var veteranBonus = math.clamp((colonyMorale01 - 0.45f) * 18f, 0f, 20f);
                var supplyBonus = supplyAdequacy * 15f;
                var casualtyPenalty = math.clamp(runtime.LastDamageTaken * 0.08f, 0f, 50f);
                var isolationPenalty = math.clamp((runtime.CommandDelayMinutes - 2f) * 4f, 0f, 25f);
                var suppressionPenalty = math.clamp(runtime.Suppression * 0.4f, 0f, 40f);
                var coverBonus = math.clamp(cover.EffectiveProtection01 * 10f, 0f, 10f);
                var nightPenalty = isNight && runtime.HasNightVision == 0 ? 10f * env.NightPenalty01 : 0f;
                runtime.Morale = MilitarySimulationMath.ComputeMorale(commanderBonus, veteranBonus, supplyBonus, casualtyPenalty,
                    isolationPenalty, suppressionPenalty, coverBonus, nightPenalty);

                var moraleAccuracyMult = MilitarySimulationMath.GetMoraleAccuracyMultiplier(runtime.Morale);
                var moraleSpeedMult = MilitarySimulationMath.GetMoraleSpeedMultiplier(runtime.Morale);
                var fatigueMod = math.saturate(1f - runtime.Fatigue / 140f);
                var nvgPenalty = isNight && runtime.HasNightVision == 0 ? 0.4f : 1f;
                var hitChance = MilitarySimulationMath.ComputeHitChance(
                    runtime.BaseAccuracy01,
                    distanceMeters,
                    runtime.RangeMeters * 0.55f,
                    runtime.RangeMeters,
                    movementModifier: order.Type == OrderType.Move ? 0.7f : 1f,
                    targetSizeModifier: runtime.IsVehicle != 0 ? 1.2f : 1f,
                    coverModifier: 1f - cover.EffectiveProtection01 * 0.7f,
                    visibilityModifier: env.Visibility01 * nvgPenalty,
                    weatherModifier: env.AccuracyMultiplier,
                    fatigueModifier: fatigueMod,
                    moraleModifier: moraleAccuracyMult);

                combat.Accuracy = hitChance;
                combat.Damage = runtime.BaseDamage * (0.65f + runtime.Morale / 200f);
                combat.FireRate = runtime.FireRatePerMinute * math.max(0.3f, moraleSpeedMult) *
                                  math.max(0.35f, 1f - runtime.Suppression / 170f);
                combat.Range = runtime.RangeMeters * (isNight && runtime.HasNightVision == 0 ? 0.6f : 1f);

                if (runtime.IsEngaged != 0)
                {
                    var ammoNeed = combat.FireRate * 0.08f;
                    var ammoSpent = math.min(runtime.Ammo, ammoNeed);
                    runtime.Ammo = math.max(0f, runtime.Ammo - ammoSpent);
                    var fuelSpent = runtime.IsVehicle != 0
                        ? (runtime.UnitType == MilitaryUnitType.AttackHelicopter ? 50f : 6f) * math.max(0.2f, env.VehicleMobilityMultiplier)
                        : 0f;
                    runtime.Fuel = math.max(0f, runtime.Fuel - fuelSpent);

                    if (ammoSpent > 0f)
                        ConsumeAmmunition(ref stock, runtime.UnitType, ammoSpent);
                    if (fuelSpent > 0f)
                        ConsumeFuel(ref stock, runtime.UnitType, fuelSpent);
                }

                var battlePressure = offensivePressure * (runtime.IsEngaged != 0 ? 1.3f : 0.55f);
                var incomingBase = battlePressure * (runtime.IsVehicle != 0 ? 18f : 7f);
                if (runtime.Ammo <= 0.01f)
                    incomingBase *= 1.2f;
                if (runtime.IsVehicle != 0 && runtime.Fuel <= 0.01f)
                    incomingBase *= 1.2f;

                var armorDamage = MilitarySimulationMath.ResolveArmorDamage(
                    incomingBase,
                    penetrationValue: 120f + offensivePressure * 120f,
                    armorValue: math.max(1f, runtime.Armor),
                    random01: rng.NextFloat());
                var finalDamage = armorDamage * (1f - cover.EffectiveProtection01);
                finalDamage *= rng.NextFloat(0.9f, 1.1f);
                runtime.Health -= finalDamage;
                runtime.LastDamageTaken = finalDamage;

                var wasWounded = wound.Type != MilitaryWoundType.None;
                if (finalDamage > 0.01f && runtime.MaxHealth > 1e-3f)
                {
                    var woundType = MilitarySimulationMath.ResolveWoundType(finalDamage / runtime.MaxHealth);
                    if (woundType > wound.Type)
                    {
                        wound.Type = woundType;
                        wound.BleedingRateHpPerSecond = MilitarySimulationMath.GetBleedingRate(woundType);
                        wound.TimeToDeathSeconds = MilitarySimulationMath.GetTimeToDeathSeconds(woundType);
                        wound.AidLevel = medicsAlive > 0 ? MilitaryAidLevel.FieldMedic : MilitaryAidLevel.BuddyAid;
                    }
                }

                if (wound.Type != MilitaryWoundType.None && runtime.IsAlive != 0)
                {
                    wound.Health = runtime.Health;
                    wound.MaxHealth = runtime.MaxHealth;
                    wound.PainLevel = math.clamp(wound.PainLevel + finalDamage * 0.4f, 0f, 100f);
                    wound.ShockLevel = math.clamp(wound.ShockLevel + finalDamage * 0.25f, 0f, 100f);
                    wound.BleedingRateHpPerSecond = math.max(0f, wound.BleedingRateHpPerSecond - medicalSupport01 * 0.08f);
                    runtime.Health -= wound.BleedingRateHpPerSecond * 25f;
                    wound.TimeToDeathSeconds = math.max(0f, wound.TimeToDeathSeconds - 180f);
                    var treatment = math.max(0f, medicalSupport01 * 4f + supplyAdequacy * 2f);
                    runtime.Health = math.min(runtime.MaxHealth, runtime.Health + treatment);

                    wound.IsConscious = wound.Type >= MilitaryWoundType.Critical ? (byte)0 : (byte)1;
                    wound.CanWalk = wound.Type <= MilitaryWoundType.Medium ? (byte)1 : (byte)0;
                    wound.CanFight = wound.Type <= MilitaryWoundType.Light ? (byte)1 : (byte)0;
                    if (runtime.Health >= runtime.MaxHealth * 0.85f && wound.Type <= MilitaryWoundType.Medium)
                    {
                        wound.Type = MilitaryWoundType.None;
                        wound.BleedingRateHpPerSecond = 0f;
                        wound.TimeToDeathSeconds = 0f;
                        wound.PainLevel = math.max(0f, wound.PainLevel - 15f);
                        wound.ShockLevel = math.max(0f, wound.ShockLevel - 15f);
                        wound.AidLevel = MilitaryAidLevel.None;
                    }
                }

                if (!wasWounded && wound.Type != MilitaryWoundType.None)
                    woundedToday++;

                if (runtime.Health <= 0f || wound.Type == MilitaryWoundType.Fatal)
                {
                    runtime.IsAlive = 0;
                    runtime.IsEngaged = 0;
                    wound.Type = MilitaryWoundType.Fatal;
                    wound.IsConscious = 0;
                    wound.CanFight = 0;
                    wound.CanWalk = 0;
                    killedToday++;
                    active = math.max(0u, active - 1u);
                    if (runtime.IsVehicle != 0)
                        friendlyEquipmentLostToday++;
                }
                else
                {
                    var retreatChance = MilitarySimulationMath.GetRetreatChance01(runtime.Morale);
                    if (rng.NextFloat() < retreatChance)
                    {
                        order.Type = OrderType.Retreat;
                        ai.CurrentBehavior = AIBehavior.Defend;
                        if (runtime.Morale < 10f)
                            moralePanicToday++;
                    }
                    else
                    {
                        ai.CurrentBehavior = runtime.IsEngaged != 0 ? AIBehavior.Engage : AIBehavior.MoveTo;
                    }

                    var enemyLossEstimate = combat.Damage * hitChance * (runtime.IsEngaged != 0 ? 0.02f : 0.004f);
                    enemyKilledToday += enemyLossEstimate * (runtime.IsVehicle != 0 ? 0.015f : 0.05f);
                    enemyEquipmentLostToday += enemyLossEstimate * (runtime.IsVehicle != 0 ? 0.01f : 0.002f);

                    var distToOrigin = math.length(transformRef.ValueRO.Position.xz);
                    visual.LodLevel = distToOrigin switch
                    {
                        <= 50f => 0,
                        <= 200f => 1,
                        <= 500f => 2,
                        <= 1000f => 3,
                        _ => 4
                    };
                    if (visual.LodLevel >= 4 && runtime.IsEngaged == 0)
                    {
                        var typeIndex = math.clamp((int)runtime.UnitType, 0, metaCounts.Length - 1);
                        metaCounts[typeIndex] += 1u;
                        metaHealthSum[typeIndex] += runtime.Health / math.max(1f, runtime.MaxHealth);
                        metaMoraleSum[typeIndex] += runtime.Morale / 100f;
                        metaAmmoSum[typeIndex] += runtime.Ammo / math.max(1f, runtime.MaxAmmo);
                        metaFuelSum[typeIndex] += runtime.IsVehicle != 0 ? runtime.Fuel / math.max(1f, runtime.MaxFuel) : 1f;
                        metaPosSum[typeIndex] += transformRef.ValueRO.Position;
                    }

                    if (wound.Type != MilitaryWoundType.None)
                        wounded++;

                    moraleSum += runtime.Morale / 100f;
                    suppressionSum += runtime.Suppression / 100f;
                    fatigueSum += runtime.Fatigue / 100f;
                    readinessSum += ComputeReadiness(runtime, supplyAdequacy);
                }

                if (em.HasComponent<CombatStats>(entity))
                    em.SetComponentData(entity, combat);
                if (em.HasComponent<MilitaryOrder>(entity))
                    em.SetComponentData(entity, order);
                if (em.HasComponent<MilitaryAIState>(entity))
                    em.SetComponentData(entity, ai);
                if (em.HasComponent<MilitaryVisualState>(entity))
                    em.SetComponentData(entity, visual);
                if (em.HasComponent<MilitaryCoverState>(entity))
                    em.SetComponentData(entity, cover);
                if (em.HasComponent<WoundedState>(entity))
                    em.SetComponentData(entity, wound);
            }

            sim.ActiveArmyUnits = active;
            sim.WoundedUnits = wounded;
            sim.OrdersInTransit = ordersInTransit;
            sim.CasualtiesFriendlyKilled += killedToday;
            sim.CasualtiesFriendlyWounded += woundedToday;
            sim.EquipmentDestroyedFriendly += friendlyEquipmentLostToday;
            sim.CasualtiesEnemyKilled += (uint)math.round(enemyKilledToday);
            sim.EnemyEquipmentDestroyed += (uint)math.round(enemyEquipmentLostToday);
            sim.AverageMorale01 = active > 0 ? math.saturate(moraleSum / active) : 0f;
            sim.AverageSuppression01 = active > 0 ? math.saturate(suppressionSum / active) : 0f;
            sim.AverageFatigue01 = active > 0 ? math.saturate(fatigueSum / active) : 0f;
            sim.CombatReadiness01 = active > 0 ? math.saturate(readinessSum / active) : 0f;

            if (SystemAPI.HasSingleton<ManufacturingSimulationState>())
            {
                var m = SystemAPI.GetSingleton<ManufacturingSimulationState>();
                var reinforcements = (uint)math.floor(math.max(0f, m.MilitaryOutputToday) / 1200f);
                sim.ReserveUnits += reinforcements;
            }

            if (killedToday > 0 && sim.ReserveUnits > 0)
            {
                var replenish = math.min(sim.ReserveUnits, killedToday / 2 + 1);
                sim.ReserveUnits -= replenish;
                // Резерв переводится в состояние подготовки; мгновенный спавн в тот же день не делаем.
            }

            if (offensivePressure > 0.45f && sim.ActiveArmyUnits > 0)
            {
                sim.BattlesTotal++;
                var friendlyLoss = killedToday + woundedToday * 0.35f;
                var enemyLoss = enemyKilledToday + enemyEquipmentLostToday * 1.5f;
                if (enemyLoss > friendlyLoss * 1.2f)
                {
                    sim.BattlesWon++;
                    sim.TerritoryCapturedKm2 += 2f + enemyLoss * 0.01f;
                }
                else if (friendlyLoss > enemyLoss * 1.2f)
                {
                    sim.BattlesLost++;
                }
                else
                {
                    sim.BattlesDraw++;
                }
                TryEnqueueStoryEvent(ref state, tick, EventBattleStarted, new FixedString64Bytes("mil-battle"));
            }

            if (supplyAdequacy < 0.45f)
                TryEnqueueStoryEvent(ref state, tick, EventSupplyCollapse, new FixedString64Bytes("mil-supply-collapse"));
            if (killedToday >= 8 || woundedToday >= 20)
                TryEnqueueStoryEvent(ref state, tick, EventHeavyCasualties, new FixedString64Bytes("mil-heavy-casualties"));
            if (moralePanicToday > 0)
                TryEnqueueStoryEvent(ref state, tick, EventMoralePanic, new FixedString64Bytes("mil-panic"));

            UpdateMetaUnits(ref meta, ref sim, ref metaCounts, ref metaHealthSum, ref metaMoraleSum, ref metaAmmoSum, ref metaFuelSum,
                ref metaPosSum);
            UpdateStrategicArmies(ref state, sim.AverageFatigue01, sim.AverageMorale01, supplyAdequacy);

            var militaryBudgetPercent = SystemAPI.HasSingleton<EconomySimulationState>()
                ? math.saturate(SystemAPI.GetSingleton<EconomySimulationState>().MilitaryProductionShare01) * 100f
                : 0f;
            var population = SystemAPI.HasSingleton<SettlerSimulationState>()
                ? math.max(1f, SystemAPI.GetSingleton<SettlerSimulationState>().PopulationAlive)
                : 100f;

            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.MilitaryActiveArmy, sim.ActiveArmyUnits);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.MilitaryReserve, sim.ReserveUnits);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.MilitaryDraftAgePool,
                math.max(0f, population - sim.ActiveArmyUnits));
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.MilitaryBudgetPercentGdp, militaryBudgetPercent);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.MilitaryBattlesTotal, sim.BattlesTotal);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.MilitaryBattlesWon, sim.BattlesWon);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.MilitaryBattlesLost, sim.BattlesLost);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.MilitaryBattlesDraw, sim.BattlesDraw);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.MilitaryCasualtiesFriendlyKilled,
                sim.CasualtiesFriendlyKilled);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.MilitaryCasualtiesFriendlyWounded,
                sim.CasualtiesFriendlyWounded);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.MilitaryCasualtiesFriendlyMia,
                sim.CasualtiesFriendlyMia);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.MilitaryEquipmentDestroyedFriendly,
                sim.EquipmentDestroyedFriendly);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.MilitaryCasualtiesEnemyKilled,
                sim.CasualtiesEnemyKilled);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.MilitaryEnemyEquipmentDestroyed,
                sim.EnemyEquipmentDestroyed);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.MilitaryTerritoryCapturedKm2,
                sim.TerritoryCapturedKm2);

            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.MilitaryAverageMorale01, sim.AverageMorale01);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.MilitaryAverageSuppression01,
                sim.AverageSuppression01);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.MilitaryAverageFatigue01, sim.AverageFatigue01);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.MilitaryCombatReadiness01,
                sim.CombatReadiness01);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.MilitarySupplyAdequacy01,
                sim.SupplyAdequacy01);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.MilitaryWoundedActive, sim.WoundedUnits);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.MilitaryMetaUnitsCount, sim.MetaUnitsCount);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.MilitaryOrdersInTransit, sim.OrdersInTransit);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.MilitaryNightOperation,
                env.IsNightOperation);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.MilitaryWeatherSeverity01,
                env.WeatherSeverity01);
        }

        private static MilitaryWeatherType ResolveWeather(float roll01, float climateRisk01)
        {
            if (climateRisk01 > 0.65f && roll01 > 0.75f)
                return roll01 > 0.92f ? MilitaryWeatherType.Blizzard : MilitaryWeatherType.Thunderstorm;
            if (roll01 < 0.30f)
                return MilitaryWeatherType.Clear;
            if (roll01 < 0.44f)
                return MilitaryWeatherType.Cloudy;
            if (roll01 < 0.58f)
                return MilitaryWeatherType.LightRain;
            if (roll01 < 0.68f)
                return MilitaryWeatherType.HeavyRain;
            if (roll01 < 0.76f)
                return MilitaryWeatherType.Fog;
            if (roll01 < 0.84f)
                return MilitaryWeatherType.Snow;
            if (roll01 < 0.92f)
                return MilitaryWeatherType.Thunderstorm;
            return climateRisk01 > 0.45f ? MilitaryWeatherType.Sandstorm : MilitaryWeatherType.Blizzard;
        }

        private static float ResolveOffensivePressure(ref SystemState state, in MilitarySimulationState sim, float supplyAdequacy01)
        {
            var pressure = 0.30f + (sim.ActiveArmyUnits > 100 ? 0.15f : 0f);
            if (SystemAPI.HasSingleton<EconomySimulationState>())
            {
                var eco = SystemAPI.GetSingleton<EconomySimulationState>();
                pressure += eco.Phase switch
                {
                    EconomyCyclePhase.Warfare => 0.55f,
                    EconomyCyclePhase.Preparation => 0.30f,
                    EconomyCyclePhase.Recovery => -0.10f,
                    _ => 0f
                };
            }

            pressure += (supplyAdequacy01 - 0.5f) * 0.4f;
            return math.clamp(pressure, 0f, 1.2f);
        }

        private static MilitaryFormationEntry FindFormation(ref DynamicBuffer<MilitaryFormationEntry> formations, uint formationId)
        {
            for (var i = 0; i < formations.Length; i++)
            {
                if (formations[i].FormationId == formationId)
                    return formations[i];
            }

            return default;
        }

        private static void ConsumeAmmunition(ref DynamicBuffer<ResourceStockEntry> stock, MilitaryUnitType unitType, float amount)
        {
            if (amount <= 0f)
                return;
            var normalized = amount * 0.03f;
            switch (unitType)
            {
                case MilitaryUnitType.LightTank:
                case MilitaryUnitType.MediumTank:
                case MilitaryUnitType.HeavyTank:
                case MilitaryUnitType.Howitzer122:
                case MilitaryUnitType.Howitzer152:
                case MilitaryUnitType.Mlrs:
                    ResourceStockpileOps.TryConsume(ref stock, ResourceId.ArtilleryShellExplosiveEpoch2, normalized);
                    ResourceStockpileOps.TryConsume(ref stock, ResourceId.Gunpowder, normalized * 0.8f);
                    break;
                case MilitaryUnitType.Grenadier:
                case MilitaryUnitType.AntiTankRifleman:
                    ResourceStockpileOps.TryConsume(ref stock, ResourceId.HandGrenadeWW1, normalized * 0.6f);
                    ResourceStockpileOps.TryConsume(ref stock, ResourceId.Dynamite, normalized * 0.5f);
                    break;
                default:
                    ResourceStockpileOps.TryConsume(ref stock, ResourceId.Gunpowder, normalized);
                    break;
            }
        }

        private static void ConsumeFuel(ref DynamicBuffer<ResourceStockEntry> stock, MilitaryUnitType unitType, float liters)
        {
            if (liters <= 0f)
                return;
            var normalized = liters * 0.02f;
            if (unitType == MilitaryUnitType.AttackHelicopter)
            {
                ResourceStockpileOps.TryConsume(ref stock, ResourceId.HighOctaneGasoline, normalized);
                return;
            }

            ResourceStockpileOps.TryConsume(ref stock, ResourceId.PetroleumProducts, normalized);
            ResourceStockpileOps.TryConsume(ref stock, ResourceId.SyntheticFuel, normalized * 0.6f);
        }

        private static float ComputeReadiness(in MilitaryUnitRuntimeState runtime, float supplyAdequacy01)
        {
            var health = runtime.Health / math.max(1f, runtime.MaxHealth);
            var ammo = runtime.Ammo / math.max(1f, runtime.MaxAmmo);
            var fuel = runtime.IsVehicle != 0 ? runtime.Fuel / math.max(1f, runtime.MaxFuel) : 1f;
            var morale = runtime.Morale / 100f;
            var suppressionPenalty = 1f - math.saturate(runtime.Suppression / 120f);
            var fatiguePenalty = 1f - math.saturate(runtime.Fatigue / 100f);
            return math.saturate((health + ammo + fuel + morale + suppressionPenalty + fatiguePenalty + supplyAdequacy01) / 7f);
        }

        private static void UpdateMetaUnits(ref DynamicBuffer<MilitaryMetaUnitEntry> meta, ref MilitarySimulationState sim,
            ref NativeArray<uint> counts, ref NativeArray<float> health, ref NativeArray<float> morale,
            ref NativeArray<float> ammo, ref NativeArray<float> fuel, ref NativeArray<float3> positions)
        {
            meta.Clear();
            uint metaId = 1;
            for (var i = 0; i < counts.Length; i++)
            {
                if (counts[i] == 0u)
                    continue;
                var c = counts[i];
                meta.Add(new MilitaryMetaUnitEntry
                {
                    MetaUnitId = metaId++,
                    DominantType = (MilitaryUnitType)i,
                    UnitsRepresented = c,
                    AverageHealth01 = health[i] / c,
                    AverageMorale01 = morale[i] / c,
                    AverageAmmo01 = ammo[i] / c,
                    AverageFuel01 = fuel[i] / c,
                    Position = positions[i] / c,
                    LodLevel = 4,
                    State = MilitaryMetaUnitState.Moving
                });
            }

            sim.MetaUnitsCount = (uint)meta.Length;
        }

        private static void UpdateStrategicArmies(ref SystemState state, float avgFatigue01, float avgMorale01, float supplyAdequacy01)
        {
            if (!SystemAPI.HasSingleton<WorldMapSimulationSingleton>())
                return;

            var armies = SystemAPI.GetSingletonBuffer<StrategicArmyEntry>(ref state);
            for (var i = 0; i < armies.Length; i++)
            {
                var a = armies[i];
                a.Fatigue01 = math.saturate(a.Fatigue01 + avgFatigue01 * 0.08f - avgMorale01 * 0.03f + (1f - supplyAdequacy01) * 0.1f);
                a.CarryingSupply = (byte)(supplyAdequacy01 > 0.65f ? 1 : 0);
                armies[i] = a;
            }
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
