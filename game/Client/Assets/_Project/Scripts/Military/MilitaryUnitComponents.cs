using Unity.Entities;
using Unity.Mathematics;

namespace ColonyConquest.Military
{
    /// <summary>Тип приказа; расширяется по мере появления боевых систем.</summary>
    public enum OrderType : byte
    {
        None = 0,
        Move = 1,
        Attack = 2,
        HoldPosition = 3,
        Retreat = 4
    }

    /// <summary>Поведение ИИ; источник: <c>spec/military_system_specification.md</c> §10.3.</summary>
    public enum AIBehavior : byte
    {
        Idle = 0,
        MoveTo = 1,
        Engage = 2,
        Defend = 3
    }

    /// <summary>Иерархия командования; псевдокод в <c>spec/military_system_specification.md</c> §1.3.</summary>
    public struct CommandHierarchy : IComponentData
    {
        public Entity Superior;
        public Entity FirstSubordinate;
        public byte CommandLevel;
        public float CommandRadiusMeters;
        public float ReactionTimeSeconds;
        public bool HasRadio;
        public float MoraleBonus;
    }

    /// <summary>Текущий приказ юниту.</summary>
    public struct MilitaryOrder : IComponentData
    {
        public OrderType Type;
        public float3 TargetPosition;
        public Entity TargetEntity;
        public float Priority;
        public double IssueTime;
        public double ExpireTime;
        public Entity IssuedBy;
    }

    /// <summary>Боевые характеристики; <c>spec/military_system_specification.md</c> §10.3.</summary>
    public struct CombatStats : IComponentData
    {
        public float Accuracy;
        public float Damage;
        public float FireRate;
        public float Range;
        public int Ammo;
        public int MaxAmmo;
    }

    /// <summary>Состояние ИИ; <c>spec/military_system_specification.md</c> §10.3.</summary>
    public struct MilitaryAIState : IComponentData
    {
        public AIBehavior CurrentBehavior;
        public Entity Target;
        public float AggroRange;
        public float3 Destination;
    }

    /// <summary>Визуальное состояние / LOD; <c>spec/military_system_specification.md</c> §10.3.</summary>
    public struct MilitaryVisualState : IComponentData
    {
        public int LodLevel;
        public float VisibilityTimer;
        public bool IsVisible;
    }

    /// <summary>Маркер сущности как боевого юнита (без полной симуляции боя).</summary>
    public struct BattleUnitTag : IComponentData
    {
    }
}
