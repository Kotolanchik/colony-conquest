using Unity.Entities;

namespace ColonyConquest.WorldMap
{
    /// <summary>Маркер сущности глобальной карты (буферы ресурсов, территорий, армий, открытых чанков).</summary>
    public struct WorldMapSimulationSingleton : IComponentData
    {
    }

    /// <summary>Сводное состояние глобальной карты.</summary>
    public struct WorldMapSimulationState : IComponentData
    {
        public uint LastProcessedDay;
        public uint WorldSeed;
        public int WorldSizeKm;
        public int ResolutionMeters;
        public uint DiscoveredChunksCount;
        public uint ControlledResourceNodes;
        public uint ActiveArmiesMoving;
    }

    /// <summary>Типы ресурсов на карте (агрегированно по §3).</summary>
    public enum WorldMapResourceKindId : byte
    {
        None = 0,
        Wood = 1,
        Stone = 2,
        IronOre = 3,
        Coal = 4,
        Oil = 5,
        Gold = 6,
        Uranium = 7,
        RareEarth = 8
    }

    /// <summary>Ресурсный узел глобальной карты.</summary>
    public struct WorldResourceNodeEntry : IBufferElementData
    {
        public MapChunkCoord Chunk;
        public WorldMapResourceKindId Kind;
        public float Amount;
        public float Quality01;
        public uint OwnerFaction;
        public WorldBiomeId Biome;
    }

    /// <summary>Контроль территории по чанкам.</summary>
    public struct TerritoryControlEntry : IBufferElementData
    {
        public MapChunkCoord Chunk;
        public uint OwnerFaction;
        public float InfluencePercent;
        public byte IsDisputed;
    }

    /// <summary>Запись открытого игроком чанка.</summary>
    public struct DiscoveredChunkEntry : IBufferElementData
    {
        public MapChunkCoord Chunk;
    }

    /// <summary>Тип перемещения армий на стратегической карте.</summary>
    public enum StrategicMovementMode : byte
    {
        Foot = 0,
        Mounted = 1,
        Mechanized = 2,
        Rail = 3,
        Air = 4,
        Naval = 5
    }

    /// <summary>Стратегическое перемещение армии между чанками.</summary>
    public struct StrategicArmyEntry : IBufferElementData
    {
        public uint FactionId;
        public MapChunkCoord CurrentChunk;
        public MapChunkCoord DestinationChunk;
        public WorldBiomeId CurrentBiome;
        public StrategicMovementMode MovementMode;
        public float DistanceRemainingKm;
        public float Fatigue01;
        public byte CarryingSupply;
    }

    /// <summary>Особое место на карте (руины, артефакты, чудеса и т.п.).</summary>
    public struct SpecialSiteEntry : IBufferElementData
    {
        public MapChunkCoord Chunk;
        public byte SiteKind;
        public byte Discovered;
    }
}
