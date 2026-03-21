using Unity.Mathematics;

namespace ColonyConquest.WorldMap
{
    /// <summary>Размер ячейки сетки для привязки игрока к чанку (метры); не весь 10×10 км регион из спеки.</summary>
    public static class WorldMapChunkMetrics
    {
        public const float LocalChunkWidthMeters = 100f;
    }

    /// <summary>Биомы глобальной карты; <c>spec/global_map_spec.md</c> §2.1.</summary>
    public enum WorldBiomeId : byte
    {
        Tundra = 0,
        Taiga = 1,
        DeciduousForest = 2,
        MixedForest = 3,
        TropicalForest = 4,
        Savanna = 5,
        Desert = 6,
        Steppe = 7,
        Mountains = 8,
        Swamp = 9,
        Coast = 10,
        Ocean = 11
    }

    /// <summary>
    /// Индекс чанка в сетке стратегической карты. Границы мира задаются отдельно конфигом.
    /// </summary>
    public struct MapChunkCoord : System.IEquatable<MapChunkCoord>
    {
        public int2 Grid;

        public MapChunkCoord(int x, int y)
        {
            Grid = new int2(x, y);
        }

        public static MapChunkCoord FromWorldPosition(float3 worldPosition, float chunkWidthMeters = WorldMapChunkMetrics.LocalChunkWidthMeters)
        {
            int x = (int)math.floor(worldPosition.x / chunkWidthMeters);
            int z = (int)math.floor(worldPosition.z / chunkWidthMeters);
            return new MapChunkCoord(x, z);
        }

        public bool Equals(MapChunkCoord other) => Grid.Equals(other.Grid);
        public override bool Equals(object obj) => obj is MapChunkCoord other && Equals(other);
        public override int GetHashCode() => Grid.GetHashCode();
    }

    /// <summary>Ось-выровненный объём чанка в метрах (локальный уровень ~10×10 км в спеке).</summary>
    public struct MapChunkBounds
    {
        public float3 Min;
        public float3 Max;

        public bool Contains(float3 worldPosition)
        {
            return math.all(worldPosition >= Min & worldPosition <= Max);
        }
    }
}
