using Unity.Entities;

namespace ColonyConquest.Settlers
{
    /// <summary>Источник: спека §1.7 — теги для фильтрации запросов (только enableable).</summary>
    public struct IsIncapacitated : IEnableableComponent
    {
    }

    public struct IsDead : IEnableableComponent
    {
    }

    public struct IsSleeping : IEnableableComponent
    {
    }

    public struct IsInCombat : IEnableableComponent
    {
    }

    public struct IsDrafted : IEnableableComponent
    {
    }

    public struct IsCaravanMember : IEnableableComponent
    {
    }

    public struct IsPrisoner : IEnableableComponent
    {
    }

    public struct IsSlave : IEnableableComponent
    {
    }

    public struct HasMentalBreak : IEnableableComponent
    {
    }

    public struct IsWounded : IEnableableComponent
    {
    }

    public struct IsInfected : IEnableableComponent
    {
    }

    public struct IsHungry : IEnableableComponent
    {
    }

    public struct IsExhausted : IEnableableComponent
    {
    }
}
