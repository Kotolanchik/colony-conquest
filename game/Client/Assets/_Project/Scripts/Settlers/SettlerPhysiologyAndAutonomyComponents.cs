using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace ColonyConquest.Settlers
{
    /// <summary>Источник: спека §1.5.</summary>
    public struct PhysiologyState : IComponentData
    {
        public float Health;
        public float MaxHealth;
        public float BloodVolume;
        public float Pain;
        public float PainTolerance;
        public float Consciousness;
        public float Mobility;
        public float Manipulation;
        public float Vision;
        public float Hearing;
        public float Breathing;
        public float BloodPumping;
    }

    /// <summary>Источник: спека §1.5.</summary>
    public struct NeedsState : IComponentData
    {
        public float Hunger;
        public float Thirst;
        public float Rest;
        public float Recreation;
        public float Comfort;
        public float Beauty;
        public float Space;
        public float TemperatureComfort;
    }

    /// <summary>Источник: спека §1.5.</summary>
    public struct Injury
    {
        public byte BodyPart;
        public byte InjuryType;
        public float Severity;
        public float HealingProgress;
        public bool IsTended;
        public bool IsInfected;
        public int TendedById;
        public ushort Age;
    }

    /// <summary>Источник: спека §1.5.</summary>
    public struct InjuryTracker : IComponentData
    {
        public FixedList128Bytes<Injury> Injuries;
        public byte InjuryCount;
        public bool HasCriticalInjury;
        public float BleedingRate;
        public float InfectionRisk;
    }

    /// <summary>Источник: спека §1.5.</summary>
    public struct Condition
    {
        public byte ConditionId;
        public byte Severity;
        public ushort Duration;
        public ushort Progress;
        public bool IsChronic;
    }

    /// <summary>Источник: спека §1.5.</summary>
    public struct MedicalConditions : IComponentData
    {
        public FixedList64Bytes<Condition> Conditions;
        public byte Immunity;
        public float Toxicity;
        public float Radiation;
    }

    /// <summary>Источник: спека §1.6.</summary>
    public struct AutonomyLevel : IComponentData
    {
        public byte Level;
        public byte PreviousLevel;
        public uint AllowedBehaviors;
        public float ReactionTime;
    }

    /// <summary>Источник: спека §1.6.</summary>
    public struct CurrentTask : IComponentData
    {
        public byte TaskType;
        public int TargetEntity;
        public float3 TargetPosition;
        public byte Priority;
        public ushort TimeLimit;
        public byte AssignedBy;
    }

    /// <summary>Источник: спека §1.6.</summary>
    public struct AIState : IComponentData
    {
        public byte BehaviorState;
        public byte LastDecisionTick;
        public float DecisionCooldown;
        public uint ConsideredOptions;
        public byte ChosenOption;
        public float Confidence;
    }

    /// <summary>Источник: спека §1.6.</summary>
    public struct CommandHierarchy : IComponentData
    {
        public int CommanderId;
        public int SubordinateCount;
        public int UnitId;
        public byte Rank;
        public byte CommandStyle;
        public float LeadershipBonus;
    }
}
