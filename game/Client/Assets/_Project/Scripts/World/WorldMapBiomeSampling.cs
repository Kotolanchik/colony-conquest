using Unity.Mathematics;

namespace ColonyConquest.WorldMap
{
    /// <summary>
    /// Детерминированный биом по координате чанка (прототип до процедурной карты);
    /// <c>spec/global_map_spec.md</c> §2.1.
    /// </summary>
    public static class WorldMapBiomeSampling
    {
        public static WorldBiomeId GetBiomeForChunk(in MapChunkCoord coord)
        {
            var h = math.hash(new uint2((uint)coord.Grid.x, (uint)coord.Grid.y));
            return (WorldBiomeId)(h % 12u);
        }
    }
}
