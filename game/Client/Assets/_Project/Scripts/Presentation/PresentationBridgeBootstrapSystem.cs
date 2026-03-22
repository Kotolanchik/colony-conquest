using ColonyConquest.Core;
using Unity.Entities;

namespace ColonyConquest.Presentation
{
    /// <summary>Создаёт singleton и очереди запросов presentation-слоя.</summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(GameBootstrapSystem))]
    public partial struct PresentationBridgeBootstrapSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<SimulationRootState>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var em = state.EntityManager;
            using var q = em.CreateEntityQuery(ComponentType.ReadOnly<PresentationBridgeSingleton>());
            if (q.CalculateEntityCount() != 0)
                return;

            var entity = em.CreateEntity();
            em.AddComponent<PresentationBridgeSingleton>(entity);
            em.AddComponent(entity, new PresentationBridgeState());
            em.AddBuffer<UnitVisualRequestEntry>(entity);
            em.AddBuffer<BuildingVisualRequestEntry>(entity);
            em.AddBuffer<UiIconRequestEntry>(entity);
            em.AddBuffer<VfxRequestEntry>(entity);
        }
    }
}
