using Unity.Entities;

namespace ColonyConquest.Politics
{
    /// <summary>Инициализация политических синглтонов и начального набора законов.</summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct PoliticalBootstrapSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            if (SystemAPI.HasSingleton<PoliticalSimulationSingleton>())
                return;

            var e = state.EntityManager.CreateEntity();
            state.EntityManager.AddComponent<PoliticalSimulationSingleton>(e);
            state.EntityManager.AddComponentData(e, new PoliticalSimulationState
            {
                LastProcessedDay = uint.MaxValue,
                Doctrine = PoliticalDoctrineId.SocialDemocracy,
                GovernmentForm = GovernmentFormId.Republic,
                Stability01 = 0.60f,
                DecisionEfficiency01 = 0.70f,
                DemocracyLevel01 = 0.60f,
                EconomyModifier = 0f,
                HappinessModifier = 0f,
                ScienceModifier = 0f,
                DefenseModifier = 0f,
                CrimeModifier = 0f,
                DecisionCooldownDaysRemaining = 0
            });

            state.EntityManager.CreateSingleton(new PoliticalLawState
            {
                TaxRate01 = 0.20f,
                CivilRightsLevel = 2,
                ReligionFreedomLevel = 2,
                MilitaryBudgetGdp01 = 0.10f,
                ImmigrationPolicy = 1
            });
        }
    }
}
