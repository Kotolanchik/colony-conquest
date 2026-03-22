using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace ColonyConquest.Military
{
    public enum MilitaryCommandLevel : byte
    {
        Headquarters = 0,
        ArmyFront = 1,
        Corps = 2,
        Division = 3,
        Brigade = 4,
        Regiment = 5,
        Battalion = 6,
        Company = 7,
        Platoon = 8,
        Squad = 9
    }

    public enum MilitaryUnitType : byte
    {
        Rifleman = 1,
        Assault = 2,
        MachineGunner = 3,
        Sniper = 4,
        Engineer = 5,
        Medic = 6,
        Grenadier = 7,
        AntiTankRifleman = 8,
        ManpadsOperator = 9,
        LightTank = 10,
        MediumTank = 11,
        HeavyTank = 12,
        InfantryFightingVehicle = 13,
        ArmoredPersonnelCarrier = 14,
        Mortar82 = 15,
        Mortar120 = 16,
        Howitzer122 = 17,
        Howitzer152 = 18,
        Mlrs = 19,
        ReconDrone = 20,
        AttackDrone = 21,
        AttackHelicopter = 22
    }

    public enum MilitaryPosture : byte
    {
        Defensive = 0,
        Offensive = 1,
        Breakthrough = 2,
        Encirclement = 3,
        Guerrilla = 4
    }

    public enum MilitaryWeatherType : byte
    {
        Clear = 0,
        Cloudy = 1,
        LightRain = 2,
        HeavyRain = 3,
        Thunderstorm = 4,
        Fog = 5,
        Snow = 6,
        Blizzard = 7,
        Sandstorm = 8
    }

    public enum MilitaryWoundType : byte
    {
        None = 0,
        Light = 1,
        Medium = 2,
        Heavy = 3,
        Critical = 4,
        Fatal = 5
    }

    public enum MilitaryAidLevel : byte
    {
        None = 0,
        SelfAid = 1,
        BuddyAid = 2,
        FieldMedic = 3,
        Triage = 4,
        FieldHospital = 5,
        RearHospital = 6
    }

    public enum MilitaryMetaUnitState : byte
    {
        Dormant = 0,
        Moving = 1,
        Engaged = 2
    }

    /// <summary>Маркер singleton сущности полной военной симуляции.</summary>
    public struct MilitarySimulationSingleton : IComponentData
    {
    }

    /// <summary>Сводное состояние военной системы (командование, бои, потери, боеготовность).</summary>
    public struct MilitarySimulationState : IComponentData
    {
        public uint LastProcessedDay;
        public uint LastUnitId;
        public uint LastOrderId;

        public uint ActiveArmyUnits;
        public uint ReserveUnits;
        public uint WoundedUnits;
        public uint MetaUnitsCount;
        public uint OrdersInTransit;

        public float AverageMorale01;
        public float AverageSuppression01;
        public float AverageFatigue01;
        public float CombatReadiness01;
        public float SupplyAdequacy01;
        public float TerritoryCapturedKm2;

        public uint BattlesTotal;
        public uint BattlesWon;
        public uint BattlesLost;
        public uint BattlesDraw;

        public uint CasualtiesFriendlyKilled;
        public uint CasualtiesFriendlyWounded;
        public uint CasualtiesFriendlyMia;
        public uint EquipmentDestroyedFriendly;
        public uint CasualtiesEnemyKilled;
        public uint EnemyEquipmentDestroyed;
    }

    /// <summary>Погодно-временная обстановка тактических действий.</summary>
    public struct MilitaryEnvironmentState : IComponentData
    {
        public MilitaryWeatherType Weather;
        public byte OperationHour;
        public byte IsNightOperation;
        public float Visibility01;
        public float AccuracyMultiplier;
        public float MovementMultiplier;
        public float VehicleMobilityMultiplier;
        public float CommunicationPenalty01;
        public float NightPenalty01;
        public float SuppressionModifier;
        public float WeatherSeverity01;
    }

    /// <summary>Параметры цепочки передачи приказов и радиопомех.</summary>
    public struct MilitaryCommandRelayState : IComponentData
    {
        public float BaseDelayMinutes;
        public float DistanceDelayMinutesPer5Km;
        public float RadioInterference01;
        public float CommanderLossPenaltyMinutes;
    }

    /// <summary>Формация командной иерархии с реакцией и радиусом управления.</summary>
    public struct MilitaryFormationEntry : IBufferElementData
    {
        public uint FormationId;
        public MilitaryCommandLevel Level;
        public uint UnitCount;
        public float CommandRadiusKm;
        public float ReactionTimeSeconds;
        public byte HasRadio;
        public float CommanderQuality01;
        public float MoraleBonus;
        public float PositionSpreadKm;
        public float SupplyPriority01;
    }

    /// <summary>Операционный приказ на уровне формации.</summary>
    public struct MilitaryOperationOrderEntry : IBufferElementData
    {
        public uint OrderId;
        public uint FormationId;
        public OrderType Type;
        public MilitaryPosture Posture;
        public float Priority01;
        public float DistanceKm;
        public float DelayMinutesRemaining;
        public float ExpireAfterMinutes;
        public byte IsAcknowledged;
        public byte IsExecuted;
        public byte IsFailed;
        public FixedString64Bytes DebugName;
    }

    /// <summary>Мета-юнит для дальнего LOD/стратегической агрегации.</summary>
    public struct MilitaryMetaUnitEntry : IBufferElementData
    {
        public uint MetaUnitId;
        public MilitaryUnitType DominantType;
        public uint UnitsRepresented;
        public float AverageHealth01;
        public float AverageMorale01;
        public float AverageAmmo01;
        public float AverageFuel01;
        public float3 Position;
        public byte LodLevel;
        public MilitaryMetaUnitState State;
    }

    /// <summary>Runtime-состояние боевой единицы.</summary>
    public struct MilitaryUnitRuntimeState : IComponentData
    {
        public uint UnitId;
        public MilitaryUnitType UnitType;
        public MilitaryCommandLevel CommandLevel;
        public float Health;
        public float MaxHealth;
        public float Armor;
        public float ArmorPenetration;
        public float BaseAccuracy01;
        public float BaseDamage;
        public float FireRatePerMinute;
        public float RangeMeters;
        public float Ammo;
        public float MaxAmmo;
        public float Fuel;
        public float MaxFuel;
        public float Morale;
        public float Suppression;
        public float Fatigue;
        public float CommandDelayMinutes;
        public float LastDamageTaken;
        public byte HasNightVision;
        public byte IsInCover;
        public byte IsVehicle;
        public byte IsAlive;
        public byte IsEngaged;
    }

    /// <summary>Состояние укрытия и его эффективная защита.</summary>
    public struct MilitaryCoverState : IComponentData
    {
        public float BaseProtection01;
        public float QualityMultiplier;
        public float OccupantState01;
        public float DirectionFactor01;
        public float EffectiveProtection01;
        public float StructureHp;
        public float StructureMaxHp;
    }

    /// <summary>Состояние ранений и эвакуации юнита.</summary>
    public struct WoundedState : IComponentData
    {
        public float Health;
        public float MaxHealth;
        public MilitaryWoundType Type;
        public float BleedingRateHpPerSecond;
        public float PainLevel;
        public float ShockLevel;
        public byte IsConscious;
        public byte CanWalk;
        public byte CanFight;
        public float TimeToDeathSeconds;
        public Entity AssignedMedic;
        public Entity EvacuationTarget;
        public MilitaryAidLevel AidLevel;
    }

    /// <summary>Шаблон статов для фабрики боевых юнитов.</summary>
    public struct MilitaryUnitTemplate
    {
        public float Health;
        public float Armor;
        public float Damage;
        public float Accuracy01;
        public float FireRatePerMinute;
        public float RangeMeters;
        public float Ammo;
        public float Fuel;
        public float ArmorPenetration;
        public byte IsVehicle;
        public byte HasNightVision;
    }
}
