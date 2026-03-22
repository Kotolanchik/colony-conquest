using Unity.Entities;
using Unity.Mathematics;

namespace ColonyConquest.WorldMap
{
    /// <summary>Инициализация демо-данных процедурной карты (территории, ресурсы, армии, особые места).</summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct WorldMapBootstrapSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            if (SystemAPI.HasSingleton<WorldMapSimulationSingleton>())
                return;

            var e = state.EntityManager.CreateEntity();
            state.EntityManager.AddComponent<WorldMapSimulationSingleton>(e);
            state.EntityManager.AddComponentData(e, new WorldMapSimulationState
            {
                LastProcessedDay = uint.MaxValue,
                WorldSeed = 1337u,
                WorldSizeKm = 1000,
                ResolutionMeters = 100,
                DiscoveredChunksCount = 0,
                ControlledResourceNodes = 0,
                ActiveArmiesMoving = 0
            });

            var discovered = state.EntityManager.AddBuffer<DiscoveredChunkEntry>(e);
            discovered.Add(new DiscoveredChunkEntry { Chunk = new MapChunkCoord(0, 0) });

            var territories = state.EntityManager.AddBuffer<TerritoryControlEntry>(e);
            territories.Add(new TerritoryControlEntry
            {
                Chunk = new MapChunkCoord(0, 0),
                OwnerFaction = 1u,
                InfluencePercent = 100f,
                IsDisputed = 0
            });
            territories.Add(new TerritoryControlEntry
            {
                Chunk = new MapChunkCoord(2, 1),
                OwnerFaction = 2u,
                InfluencePercent = 80f,
                IsDisputed = 0
            });
            territories.Add(new TerritoryControlEntry
            {
                Chunk = new MapChunkCoord(-2, -1),
                OwnerFaction = 3u,
                InfluencePercent = 75f,
                IsDisputed = 0
            });

            var nodes = state.EntityManager.AddBuffer<WorldResourceNodeEntry>(e);
            AddNode(ref nodes, 1, 0, WorldMapResourceKindId.Wood);
            AddNode(ref nodes, -1, 1, WorldMapResourceKindId.Stone);
            AddNode(ref nodes, 2, 2, WorldMapResourceKindId.IronOre);
            AddNode(ref nodes, -3, 2, WorldMapResourceKindId.Coal);
            AddNode(ref nodes, 4, -1, WorldMapResourceKindId.Oil);
            AddNode(ref nodes, -4, -2, WorldMapResourceKindId.Gold);
            AddNode(ref nodes, 5, 3, WorldMapResourceKindId.Uranium);
            AddNode(ref nodes, -5, 4, WorldMapResourceKindId.RareEarth);

            var armies = state.EntityManager.AddBuffer<StrategicArmyEntry>(e);
            armies.Add(new StrategicArmyEntry
            {
                FactionId = 1u,
                CurrentChunk = new MapChunkCoord(0, 0),
                DestinationChunk = new MapChunkCoord(3, 0),
                CurrentBiome = WorldBiomeId.MixedForest,
                MovementMode = StrategicMovementMode.Foot,
                DistanceRemainingKm = 30f,
                Fatigue01 = 0.1f,
                CarryingSupply = 1
            });
            armies.Add(new StrategicArmyEntry
            {
                FactionId = 2u,
                CurrentChunk = new MapChunkCoord(2, 1),
                DestinationChunk = new MapChunkCoord(0, 0),
                CurrentBiome = WorldBiomeId.Steppe,
                MovementMode = StrategicMovementMode.Mechanized,
                DistanceRemainingKm = 40f,
                Fatigue01 = 0.2f,
                CarryingSupply = 0
            });

            var special = state.EntityManager.AddBuffer<SpecialSiteEntry>(e);
            special.Add(new SpecialSiteEntry
            {
                Chunk = new MapChunkCoord(1, 1),
                SiteKind = 0,
                Discovered = 0
            });
            special.Add(new SpecialSiteEntry
            {
                Chunk = new MapChunkCoord(-2, 2),
                SiteKind = 1,
                Discovered = 0
            });

            if (SystemAPI.HasSingleton<WorldMapFocusState>())
            {
                ref var focus = ref SystemAPI.GetSingletonRW<WorldMapFocusState>().ValueRW;
                focus.PlayerChunk = new MapChunkCoord(0, 0);
                focus.PreviewBiome = WorldMapBiomeSampling.GetBiomeForChunk(focus.PlayerChunk);
                focus.ActiveScale = WorldMapScaleLevel.Local;
            }
        }

        private static void AddNode(ref DynamicBuffer<WorldResourceNodeEntry> nodes, int x, int y, WorldMapResourceKindId kind)
        {
            var coord = new MapChunkCoord(x, y);
            nodes.Add(new WorldResourceNodeEntry
            {
                Chunk = coord,
                Kind = kind,
                Amount = 500f,
                Quality01 = 0.65f,
                OwnerFaction = 0u,
                Biome = WorldMapBiomeSampling.GetBiomeForChunk(coord)
            });
        }
    }
}
