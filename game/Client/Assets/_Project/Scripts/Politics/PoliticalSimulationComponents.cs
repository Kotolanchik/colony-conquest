using Unity.Entities;

namespace ColonyConquest.Politics
{
    /// <summary>Маркер сущности политической подсистемы.</summary>
    public struct PoliticalSimulationSingleton : IComponentData
    {
    }

    /// <summary>Сводное политическое состояние колонии.</summary>
    public struct PoliticalSimulationState : IComponentData
    {
        public uint LastProcessedDay;
        public PoliticalDoctrineId Doctrine;
        public GovernmentFormId GovernmentForm;
        public float Stability01;
        public float DecisionEfficiency01;
        public float DemocracyLevel01;

        public float EconomyModifier;
        public float HappinessModifier;
        public float ScienceModifier;
        public float DefenseModifier;
        public float CrimeModifier;

        public short DecisionCooldownDaysRemaining;
    }

    /// <summary>Выбранные группы законов из §4.1 (агрегированно).</summary>
    public struct PoliticalLawState : IComponentData
    {
        public float TaxRate01;
        public byte CivilRightsLevel; // 0 low, 1 medium, 2 high
        public byte ReligionFreedomLevel; // 0 banned, 1 limited, 2 free
        public float MilitaryBudgetGdp01;
        public byte ImmigrationPolicy; // 0 closed, 1 limited, 2 open
    }
}
