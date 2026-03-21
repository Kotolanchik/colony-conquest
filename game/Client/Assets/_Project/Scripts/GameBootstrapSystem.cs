using Unity.Entities;
using UnityEngine;

namespace ColonyConquest.Core
{
    /// <summary>
    /// Создаёт синглтон <see cref="SimulationRootState"/> и наращивает SimulationTick каждый кадр симуляции.
    /// </summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    public partial struct GameBootstrapSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.EntityManager.CreateSingleton(new SimulationRootState { SimulationTick = 0 });
            SubsystemBootstrapUtility.EnsureSubsystemEntities(ref state);
            Debug.Log("[Colony & Conquest] ECS bootstrap: SimulationRootState + подсистемы (аналитика, аудио, события, карта, стройка, netcode, демо-юнит).");
        }

        public void OnUpdate(ref SystemState state)
        {
            ref var root = ref SystemAPI.GetSingletonRW<SimulationRootState>().ValueRW;
            root.SimulationTick++;
        }
    }
}
