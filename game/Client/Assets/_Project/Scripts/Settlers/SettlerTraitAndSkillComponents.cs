using Unity.Collections;
using Unity.Entities;

namespace ColonyConquest.Settlers
{
    /// <summary>Источник: спека §1.2.</summary>
    public struct TraitSlot
    {
        public byte TraitId;
        public sbyte Intensity;
        public byte Source;
    }

    /// <summary>Источник: спека §1.2.</summary>
    public struct PersonalityTraits : IComponentData
    {
        public FixedList64Bytes<TraitSlot> ActiveTraits;
        public uint TraitCompatibilityMask;
    }

    /// <summary>Источник: спека §1.2.</summary>
    public struct Aptitude
    {
        public byte SkillCategory;
        public byte AptitudeLevel;
    }

    /// <summary>Источник: спека §1.2.</summary>
    public struct Aptitudes : IComponentData
    {
        public FixedList32Bytes<Aptitude> NaturalTalents;
        public FixedList32Bytes<Aptitude> AcquiredTalents;
        public FixedList32Bytes<byte> SkillAffinities;
    }

    /// <summary>Источник: спека §1.2.</summary>
    public struct Phobia
    {
        public byte PhobiaType;
        public byte Severity;
        public uint TriggerMask;
    }

    /// <summary>Источник: спека §1.2.</summary>
    public struct Trauma
    {
        public byte TraumaType;
        public byte Severity;
        public int OriginEventId;
        public bool IsProcessed;
    }

    /// <summary>Источник: спека §1.2.</summary>
    public struct MentalConditions : IComponentData
    {
        public FixedList32Bytes<Phobia> Phobias;
        public FixedList32Bytes<Trauma> Traumas;
        public byte PTSDLevel;
        public byte DepressionLevel;
    }

    /// <summary>Источник: спека §1.3.</summary>
    public struct Skill
    {
        public byte SkillId;
        public byte Level;
        public ushort Experience;
        public ushort ExperienceToNext;
        public byte PassionLevel;
        public byte NaturalCap;
        public byte LearnedCap;
        public ushort TotalUses;
    }

    /// <summary>
    /// Источник: спека §1.3. В спеке указан FixedList128Bytes — для 20 навыков при размере <see cref="Skill"/>
    /// используется <see cref="FixedList256Bytes{T}"/>, иначе не помещается.
    /// </summary>
    public struct SkillSet : IComponentData
    {
        public FixedList256Bytes<Skill> Skills;
        public ushort TotalSkillPoints;
        public byte PrimaryRole;
        public byte SecondaryRole;
    }
}
