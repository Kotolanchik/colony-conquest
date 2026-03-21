using ColonyConquest.Simulation;
using Unity.Entities;

namespace ColonyConquest.Technology
{
    /// <summary>Создаёт runtime-слой дерева технологий поверх базового `ColonyTechProgressState`.</summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct TechTreeBootstrapSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            if (SystemAPI.HasSingleton<TechTreeSimulationSingleton>())
                return;

            var e = state.EntityManager.CreateEntity();
            state.EntityManager.AddComponent<TechTreeSimulationSingleton>(e);
            state.EntityManager.AddComponentData(e, new TechTreeSimulationState
            {
                LastProcessedDay = uint.MaxValue,
                ActiveResearch = TechDefinitionId.None,
                ActiveResearchProgressPoints = 0f,
                ResearchPoolPoints = 0f,
                EraTransitionsTotal = 0,
                UnlocksEra1 = 0,
                UnlocksEra2 = 0,
                UnlocksEra3 = 0,
                UnlocksEra4 = 0,
                UnlocksEra5 = 0
            });
            state.EntityManager.AddBuffer<TechUnlockedEntry>(e);

            if (SystemAPI.HasSingleton<ColonyTechProgressState>())
            {
                ref var tech = ref SystemAPI.GetSingletonRW<ColonyTechProgressState>().ValueRW;
                if (tech.CurrentEra == TechEraId.None)
                    tech.CurrentEra = TechEraId.Era1_Foundation;
            }
        }
    }
}
