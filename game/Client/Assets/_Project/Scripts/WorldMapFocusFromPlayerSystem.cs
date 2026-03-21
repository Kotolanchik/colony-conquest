using ColonyConquest.WorldMap;
using Unity.Entities;
using Unity.Transforms;

namespace ColonyConquest.Core
{
    /// <summary>Обновляет синглтон фокуса карты по позиции игрока (TestMoveTarget).</summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(PlayerMoveFromInputSystem))]
    public partial struct WorldMapFocusFromPlayerSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            foreach (var lt in SystemAPI.Query<RefRO<LocalTransform>>().WithAll<PlayerMoveTargetTag>())
            {
                var chunk = MapChunkCoord.FromWorldPosition(lt.ValueRO.Position);
                var biome = WorldMapBiomeSampling.GetBiomeForChunk(chunk);
                ref var focus = ref SystemAPI.GetSingletonRW<WorldMapFocusState>().ValueRW;
                focus.PlayerChunk = chunk;
                focus.PreviewBiome = biome;
                return;
            }
        }
    }
}
