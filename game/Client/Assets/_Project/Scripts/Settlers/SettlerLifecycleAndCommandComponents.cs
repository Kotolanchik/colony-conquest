using Unity.Collections;
using Unity.Entities;

namespace ColonyConquest.Settlers
{
    /// <summary>Источник: спека §1.7.</summary>
    public struct LifecycleState : IComponentData
    {
        public byte LifeStage;
        public int BirthTick;
        public int DeathTick;
        public byte DeathCause;
        public int KillerId;
    }

    /// <summary>Источник: спека §1.7.</summary>
    public struct SettlerStats : IComponentData
    {
        public uint EnemiesKilled;
        public uint BattlesParticipated;
        public uint BuildingsBuilt;
        public uint ItemsCrafted;
        public uint ResearchPoints;
        public uint PatientsTreated;
        public uint CropsHarvested;
        public uint AnimalsTamed;
        public float TotalWorkDone;
        public float CombatDamageDealt;
        public float CombatDamageTaken;
        public ushort MentalBreaksCount;
        public ushort InjuriesCount;
        public ushort DiseasesSurvived;
    }

    /// <summary>Источник: спека §6.1.</summary>
    public enum BodyPart : byte
    {
        Head = 0,
        Torso = 1,
        LeftArm = 2,
        RightArm = 3,
        LeftLeg = 4,
        RightLeg = 5,
        LeftEye = 6,
        RightEye = 7,
        LeftEar = 8,
        RightEar = 9,
        LeftLung = 10,
        RightLung = 11,
        Heart = 12,
        Stomach = 13,
        Liver = 14,
        Kidneys = 15
    }

    /// <summary>Источник: спека §6.2.</summary>
    public enum InjuryType : byte
    {
        Bruise = 0,
        Cut = 1,
        Puncture = 2,
        Gunshot = 3,
        Burn = 4,
        Fracture = 5,
        Internal = 6,
        Chemical = 7,
        Radiation = 8
    }

    /// <summary>Источник: спека §8.2.</summary>
    public enum CommandStyle : byte
    {
        Autocratic = 0,
        Democratic = 1,
        LaissezFaire = 2,
        Transformational = 3,
        Transactional = 4
    }

    /// <summary>
    /// Данные ИИ-командира из §8.2.
    /// В спеке указан NativeList, в ECS-компоненте используется FixedList для blittable runtime-данных.
    /// </summary>
    public struct CommanderAI : IComponentData
    {
        public int CommanderId;
        public FixedList64Bytes<int> Subordinates;
        public byte CommandStyle;
        public float LeadershipSkill;
        public float TacticalSkill;
        public byte Aggressiveness;
        public byte CautionLevel;
        public int CurrentOrderId;
        public float OrderConfidence;
    }
}
