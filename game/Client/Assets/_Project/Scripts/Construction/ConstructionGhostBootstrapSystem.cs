using Unity.Entities;
using Unity.Mathematics;

namespace ColonyConquest.Core
{
    /// <summary>
    /// Создаёт синглтон <see cref="ConstructionGhostState"/> на клиентском мире (превью UI/призрак).
    /// </summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct ConstructionGhostBootstrapSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            if (SystemAPI.HasSingleton<ConstructionGhostState>())
                return;

            state.EntityManager.CreateSingleton(new ConstructionGhostState
            {
                Active = 0,
                BlueprintId = ConstructionBlueprintId.None,
                FootprintCells = new int2(2, 2),
                AnchorWorld = float3.zero,
                RotationRadians = 0f,
                PlacementValid = 0,
            });
        }
    }
}
