using Unity.Entities;

namespace ColonyConquest.WorldMap
{
    /// <summary>Текущий чанк под игроком и предпросмотр биома (упрощённо).</summary>
    public struct WorldMapFocusState : IComponentData
    {
        public MapChunkCoord PlayerChunk;
        public WorldBiomeId PreviewBiome;
    }
}
