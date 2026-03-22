using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ColonyConquest.Military
{
    /// <summary>Инициализация military runtime: штаб, формации, приказы и стартовый состав войск.</summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct MilitarySimulationBootstrapSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            if (SystemAPI.HasSingleton<MilitarySimulationSingleton>())
                return;

            var em = state.EntityManager;
            var singleton = em.CreateEntity();
            em.AddComponent<MilitarySimulationSingleton>(singleton);
            em.AddComponent(singleton, new MilitarySimulationState
            {
                LastProcessedDay = uint.MaxValue,
                LastUnitId = 0,
                LastOrderId = 5,
                ActiveArmyUnits = 0,
                ReserveUnits = 120,
                WoundedUnits = 0,
                MetaUnitsCount = 0,
                OrdersInTransit = 0,
                AverageMorale01 = 0.75f,
                AverageSuppression01 = 0.1f,
                AverageFatigue01 = 0.15f,
                CombatReadiness01 = 0.78f,
                SupplyAdequacy01 = 0.8f,
                TerritoryCapturedKm2 = 0f,
                BattlesTotal = 0,
                BattlesWon = 0,
                BattlesLost = 0,
                BattlesDraw = 0,
                CasualtiesFriendlyKilled = 0,
                CasualtiesFriendlyWounded = 0,
                CasualtiesFriendlyMia = 0,
                EquipmentDestroyedFriendly = 0,
                CasualtiesEnemyKilled = 0,
                EnemyEquipmentDestroyed = 0
            });
            em.AddComponent(singleton, new MilitaryEnvironmentState
            {
                Weather = MilitaryWeatherType.Clear,
                OperationHour = 10,
                IsNightOperation = 0,
                Visibility01 = 1f,
                AccuracyMultiplier = 1f,
                MovementMultiplier = 1f,
                VehicleMobilityMultiplier = 1f,
                CommunicationPenalty01 = 0f,
                NightPenalty01 = 0f,
                SuppressionModifier = 1f,
                WeatherSeverity01 = 0f
            });
            em.AddComponent(singleton, new MilitaryCommandRelayState
            {
                BaseDelayMinutes = 0.5f,
                DistanceDelayMinutesPer5Km = 1f,
                RadioInterference01 = 0.1f,
                CommanderLossPenaltyMinutes = 0f
            });

            var formations = em.AddBuffer<MilitaryFormationEntry>(singleton);
            formations.Add(BuildFormation(1, MilitaryCommandLevel.Headquarters, 1, 50f, 600f, true, 0.92f, 12f, 5f, 1f));
            formations.Add(BuildFormation(2, MilitaryCommandLevel.ArmyFront, 2, 30f, 360f, true, 0.85f, 10f, 4f, 0.95f));
            formations.Add(BuildFormation(3, MilitaryCommandLevel.Division, 4, 15f, 120f, true, 0.8f, 8f, 2.5f, 0.85f));
            formations.Add(BuildFormation(4, MilitaryCommandLevel.Brigade, 8, 8f, 60f, true, 0.76f, 6f, 1.6f, 0.75f));
            formations.Add(BuildFormation(5, MilitaryCommandLevel.Battalion, 16, 2f, 15f, true, 0.72f, 4f, 0.8f, 0.65f));
            formations.Add(BuildFormation(6, MilitaryCommandLevel.Company, 24, 0.8f, 5f, true, 0.68f, 3f, 0.3f, 0.55f));
            formations.Add(BuildFormation(7, MilitaryCommandLevel.Platoon, 32, 0.3f, 2f, false, 0.64f, 2f, 0.2f, 0.45f));
            formations.Add(BuildFormation(8, MilitaryCommandLevel.Squad, 40, 0.1f, 0.5f, false, 0.6f, 1f, 0.1f, 0.35f));

            var orders = em.AddBuffer<MilitaryOperationOrderEntry>(singleton);
            orders.Add(BuildOrder(1, 2, OrderType.HoldPosition, MilitaryPosture.Defensive, 0.8f, 5f, "hold-front"));
            orders.Add(BuildOrder(2, 3, OrderType.Attack, MilitaryPosture.Offensive, 1f, 18f, "offensive-axis-a"));
            orders.Add(BuildOrder(3, 5, OrderType.Move, MilitaryPosture.Breakthrough, 0.9f, 12f, "breakthrough-group"));
            orders.Add(BuildOrder(4, 6, OrderType.Retreat, MilitaryPosture.Encirclement, 0.6f, 9f, "withdraw-reserve"));
            orders.Add(BuildOrder(5, 7, OrderType.HoldPosition, MilitaryPosture.Guerrilla, 0.7f, 4f, "urban-hold"));

            em.AddBuffer<MilitaryMetaUnitEntry>(singleton);

            var sim = em.GetComponentData<MilitarySimulationState>(singleton);
            var rng = new Random(0xC01A51D3u);

            SpawnGroup(ref em, ref sim, ref rng, MilitaryUnitType.Rifleman, 48, MilitaryCommandLevel.Squad, new float3(20f, 0f, 20f));
            SpawnGroup(ref em, ref sim, ref rng, MilitaryUnitType.Assault, 20, MilitaryCommandLevel.Squad, new float3(24f, 0f, 24f));
            SpawnGroup(ref em, ref sim, ref rng, MilitaryUnitType.MachineGunner, 12, MilitaryCommandLevel.Platoon, new float3(28f, 0f, 21f));
            SpawnGroup(ref em, ref sim, ref rng, MilitaryUnitType.Sniper, 8, MilitaryCommandLevel.Company, new float3(33f, 0f, 16f));
            SpawnGroup(ref em, ref sim, ref rng, MilitaryUnitType.Engineer, 12, MilitaryCommandLevel.Company, new float3(18f, 0f, 28f));
            SpawnGroup(ref em, ref sim, ref rng, MilitaryUnitType.Medic, 10, MilitaryCommandLevel.Company, new float3(16f, 0f, 24f));
            SpawnGroup(ref em, ref sim, ref rng, MilitaryUnitType.Grenadier, 10, MilitaryCommandLevel.Platoon, new float3(30f, 0f, 26f));
            SpawnGroup(ref em, ref sim, ref rng, MilitaryUnitType.AntiTankRifleman, 8, MilitaryCommandLevel.Platoon, new float3(36f, 0f, 26f));
            SpawnGroup(ref em, ref sim, ref rng, MilitaryUnitType.ManpadsOperator, 6, MilitaryCommandLevel.Company, new float3(38f, 0f, 30f));
            SpawnGroup(ref em, ref sim, ref rng, MilitaryUnitType.LightTank, 6, MilitaryCommandLevel.Battalion, new float3(42f, 0f, 20f));
            SpawnGroup(ref em, ref sim, ref rng, MilitaryUnitType.MediumTank, 5, MilitaryCommandLevel.Battalion, new float3(46f, 0f, 20f));
            SpawnGroup(ref em, ref sim, ref rng, MilitaryUnitType.HeavyTank, 3, MilitaryCommandLevel.Battalion, new float3(50f, 0f, 21f));
            SpawnGroup(ref em, ref sim, ref rng, MilitaryUnitType.InfantryFightingVehicle, 5, MilitaryCommandLevel.Battalion, new float3(44f, 0f, 25f));
            SpawnGroup(ref em, ref sim, ref rng, MilitaryUnitType.ArmoredPersonnelCarrier, 5, MilitaryCommandLevel.Battalion, new float3(48f, 0f, 25f));
            SpawnGroup(ref em, ref sim, ref rng, MilitaryUnitType.Mortar82, 6, MilitaryCommandLevel.Company, new float3(26f, 0f, 34f));
            SpawnGroup(ref em, ref sim, ref rng, MilitaryUnitType.Mortar120, 4, MilitaryCommandLevel.Company, new float3(30f, 0f, 34f));
            SpawnGroup(ref em, ref sim, ref rng, MilitaryUnitType.Howitzer122, 3, MilitaryCommandLevel.Battalion, new float3(40f, 0f, 34f));
            SpawnGroup(ref em, ref sim, ref rng, MilitaryUnitType.Howitzer152, 2, MilitaryCommandLevel.Battalion, new float3(44f, 0f, 34f));
            SpawnGroup(ref em, ref sim, ref rng, MilitaryUnitType.Mlrs, 2, MilitaryCommandLevel.Battalion, new float3(48f, 0f, 34f));
            SpawnGroup(ref em, ref sim, ref rng, MilitaryUnitType.ReconDrone, 4, MilitaryCommandLevel.Division, new float3(35f, 0f, 38f));
            SpawnGroup(ref em, ref sim, ref rng, MilitaryUnitType.AttackDrone, 4, MilitaryCommandLevel.Division, new float3(38f, 0f, 38f));
            SpawnGroup(ref em, ref sim, ref rng, MilitaryUnitType.AttackHelicopter, 2, MilitaryCommandLevel.Division, new float3(42f, 0f, 38f));

            sim.ActiveArmyUnits = sim.LastUnitId;
            em.SetComponentData(singleton, sim);
        }

        private static MilitaryFormationEntry BuildFormation(uint id, MilitaryCommandLevel level, uint units,
            float radiusKm, float reactionSec, bool radio, float commanderQuality01, float moraleBonus,
            float spreadKm, float supplyPriority01)
        {
            return new MilitaryFormationEntry
            {
                FormationId = id,
                Level = level,
                UnitCount = units,
                CommandRadiusKm = radiusKm,
                ReactionTimeSeconds = reactionSec,
                HasRadio = radio ? (byte)1 : (byte)0,
                CommanderQuality01 = commanderQuality01,
                MoraleBonus = moraleBonus,
                PositionSpreadKm = spreadKm,
                SupplyPriority01 = supplyPriority01
            };
        }

        private static MilitaryOperationOrderEntry BuildOrder(uint id, uint formationId, OrderType type,
            MilitaryPosture posture, float priority01, float distanceKm, in FixedString64Bytes label)
        {
            return new MilitaryOperationOrderEntry
            {
                OrderId = id,
                FormationId = formationId,
                Type = type,
                Posture = posture,
                Priority01 = priority01,
                DistanceKm = distanceKm,
                DelayMinutesRemaining = 0f,
                ExpireAfterMinutes = 240f,
                IsAcknowledged = 0,
                IsExecuted = 0,
                IsFailed = 0,
                DebugName = label
            };
        }

        private static void SpawnGroup(ref EntityManager em, ref MilitarySimulationState sim, ref Random rng,
            MilitaryUnitType unitType, int count, MilitaryCommandLevel level, in float3 center)
        {
            if (!MilitarySimulationMath.TryGetUnitTemplate(unitType, out var template))
                return;

            for (var i = 0; i < count; i++)
            {
                sim.LastUnitId++;
                var e = em.CreateEntity();
                em.AddComponent<BattleUnitTag>(e);

                var offset = new float3(
                    rng.NextFloat(-3f, 3f),
                    0f,
                    rng.NextFloat(-3f, 3f));
                var pos = center + offset + new float3((i % 6) * 0.9f, 0f, (i / 6) * 0.6f);
                em.AddComponent(e, LocalTransform.FromPosition(pos));

                em.AddComponent(e, new CombatStats
                {
                    Accuracy = template.Accuracy01,
                    Damage = template.Damage,
                    FireRate = template.FireRatePerMinute,
                    Range = template.RangeMeters,
                    Ammo = (int)math.round(template.Ammo),
                    MaxAmmo = (int)math.round(template.Ammo)
                });
                em.AddComponent(e, new MilitaryOrder
                {
                    Type = OrderType.HoldPosition,
                    TargetPosition = pos,
                    Priority = 0.5f,
                    IssueTime = 0,
                    ExpireTime = 0,
                    IssuedBy = Entity.Null,
                    TargetEntity = Entity.Null
                });
                em.AddComponent(e, new MilitaryAIState
                {
                    CurrentBehavior = AIBehavior.Defend,
                    Target = Entity.Null,
                    AggroRange = math.max(60f, template.RangeMeters * 0.6f),
                    Destination = pos
                });
                em.AddComponent(e, new MilitaryVisualState
                {
                    LodLevel = template.IsVehicle != 0 ? 2 : 1,
                    VisibilityTimer = 0f,
                    IsVisible = true
                });
                em.AddComponent(e, new CommandHierarchy
                {
                    Superior = Entity.Null,
                    FirstSubordinate = Entity.Null,
                    CommandLevel = (byte)level,
                    CommandRadiusMeters = GetRadiusMeters(level),
                    ReactionTimeSeconds = GetReactionSeconds(level),
                    HasRadio = level <= MilitaryCommandLevel.Company,
                    MoraleBonus = level switch
                    {
                        MilitaryCommandLevel.Headquarters => 12f,
                        MilitaryCommandLevel.ArmyFront => 10f,
                        MilitaryCommandLevel.Corps => 8f,
                        MilitaryCommandLevel.Division => 7f,
                        MilitaryCommandLevel.Brigade => 6f,
                        MilitaryCommandLevel.Regiment => 5f,
                        MilitaryCommandLevel.Battalion => 4f,
                        MilitaryCommandLevel.Company => 3f,
                        MilitaryCommandLevel.Platoon => 2f,
                        _ => 1f
                    }
                });

                em.AddComponent(e, new MilitaryUnitRuntimeState
                {
                    UnitId = sim.LastUnitId,
                    UnitType = unitType,
                    CommandLevel = level,
                    Health = template.Health,
                    MaxHealth = template.Health,
                    Armor = template.Armor,
                    ArmorPenetration = template.ArmorPenetration,
                    BaseAccuracy01 = template.Accuracy01,
                    BaseDamage = template.Damage,
                    FireRatePerMinute = template.FireRatePerMinute,
                    RangeMeters = template.RangeMeters,
                    Ammo = template.Ammo,
                    MaxAmmo = template.Ammo,
                    Fuel = template.Fuel,
                    MaxFuel = math.max(1f, template.Fuel),
                    Morale = 75f,
                    Suppression = 6f,
                    Fatigue = 10f,
                    CommandDelayMinutes = 0.5f,
                    LastDamageTaken = 0f,
                    HasNightVision = template.HasNightVision,
                    IsInCover = 1,
                    IsVehicle = template.IsVehicle,
                    IsAlive = 1,
                    IsEngaged = 0
                });
                em.AddComponent(e, new MilitaryCoverState
                {
                    BaseProtection01 = template.IsVehicle != 0 ? 0.2f : 0.5f,
                    QualityMultiplier = 1f,
                    OccupantState01 = template.IsVehicle != 0 ? 0.5f : 1f,
                    DirectionFactor01 = 0.8f,
                    EffectiveProtection01 = template.IsVehicle != 0 ? 0.15f : 0.40f,
                    StructureHp = 300f,
                    StructureMaxHp = 300f
                });
                em.AddComponent(e, new WoundedState
                {
                    Health = template.Health,
                    MaxHealth = template.Health,
                    Type = MilitaryWoundType.None,
                    BleedingRateHpPerSecond = 0f,
                    PainLevel = 0f,
                    ShockLevel = 0f,
                    IsConscious = 1,
                    CanWalk = 1,
                    CanFight = 1,
                    TimeToDeathSeconds = 0f,
                    AssignedMedic = Entity.Null,
                    EvacuationTarget = Entity.Null,
                    AidLevel = MilitaryAidLevel.None
                });
            }
        }

        private static float GetRadiusMeters(MilitaryCommandLevel level)
        {
            return level switch
            {
                MilitaryCommandLevel.Headquarters => 50000f,
                MilitaryCommandLevel.ArmyFront => 30000f,
                MilitaryCommandLevel.Corps => 15000f,
                MilitaryCommandLevel.Division => 8000f,
                MilitaryCommandLevel.Brigade => 5000f,
                MilitaryCommandLevel.Regiment => 2000f,
                MilitaryCommandLevel.Battalion => 800f,
                MilitaryCommandLevel.Company => 300f,
                MilitaryCommandLevel.Platoon => 120f,
                _ => 80f
            };
        }

        private static float GetReactionSeconds(MilitaryCommandLevel level)
        {
            return level switch
            {
                MilitaryCommandLevel.Headquarters => 300f,
                MilitaryCommandLevel.ArmyFront => 180f,
                MilitaryCommandLevel.Corps => 120f,
                MilitaryCommandLevel.Division => 90f,
                MilitaryCommandLevel.Brigade => 45f,
                MilitaryCommandLevel.Regiment => 20f,
                MilitaryCommandLevel.Battalion => 10f,
                MilitaryCommandLevel.Company => 4f,
                MilitaryCommandLevel.Platoon => 2f,
                _ => 1f
            };
        }
    }
}
