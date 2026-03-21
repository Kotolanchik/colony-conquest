using Unity.Collections;
using Unity.Entities;

namespace ColonyConquest.Settlers
{
    /// <summary>Источник: спека §1.4 (PsychologyState).</summary>
    public struct PsychologyState : IComponentData, IEnableableComponent
    {
        public float Mood;
        public float MoodTrend;
        public float Stress;
        public float StressResistance;
        public float MentalBreakThreshold;
        public float MentalBreakRecovery;
        public byte CurrentBreakRisk;
        public byte ActiveBreakType;
    }

    /// <summary>Источник: спека §1.4; описание — FixedString32Bytes.</summary>
    public struct MoodModifier
    {
        public byte SourceType;
        public short Value;
        public ushort Duration;
        public ushort Remaining;
        public FixedString32Bytes Description;
    }

    /// <summary>Источник: спека §1.4.</summary>
    public struct MoodModifiers : IComponentData
    {
        public FixedList128Bytes<MoodModifier> Modifiers;
        public float BaseMood;
    }

    /// <summary>Источник: спека §1.4.</summary>
    public struct SocialBond
    {
        public int TargetId;
        public sbyte Value;
        public byte BondType;
        public ushort Duration;
        public byte InteractionCount;
    }

    /// <summary>Источник: спека §1.4.</summary>
    public struct SocialBonds : IComponentData
    {
        public FixedList64Bytes<SocialBond> Bonds;
        public int PartnerId;
        public int MentorId;
        public int RivalId;
    }
}
