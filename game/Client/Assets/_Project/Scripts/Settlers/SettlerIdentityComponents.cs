using Unity.Collections;
using Unity.Entities;

namespace ColonyConquest.Settlers
{
    /// <summary>
    /// Источник полей: <c>spec/settler_simulation_system_spec.md</c> §1.1.
    /// </summary>
    public struct SettlerIdentity : IComponentData
    {
        public FixedString64Bytes Name;
        public FixedString64Bytes Surname;
        public FixedString64Bytes Nickname;
        public uint GenerationSeed;
        public byte Gender;
        public byte Age;
        public byte OriginFaction;
        public int PortraitId;
    }

    /// <summary>Источник: спека поселенцев §1.1.</summary>
    public struct SettlerAppearance : IComponentData
    {
        public byte BodyType;
        public byte SkinTone;
        public byte HairStyle;
        public byte HairColor;
        public byte FacialHair;
        public byte EyeColor;
        public ushort ScarsBitmask;
        public byte Prosthetics;
        public float Height;
    }
}
