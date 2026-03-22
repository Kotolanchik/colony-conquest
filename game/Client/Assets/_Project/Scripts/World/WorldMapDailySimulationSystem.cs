using ColonyConquest.Analytics;
using ColonyConquest.Core;
using ColonyConquest.Simulation;
using ColonyConquest.Story;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace ColonyConquest.WorldMap
{
    /// <summary>Суточная симуляция глобальной карты: открытие чанков, влияние, движение армий, контроль ресурсов.</summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(ColonyCalendarAdvanceSystem))]
    [UpdateAfter(typeof(WorldMapFocusFromPlayerSystem))]
    public partial struct WorldMapDailySimulationSystem : ISystem
    {
        private const uint EventSpecialSiteDiscovered = 0xD301;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameCalendarState>();
            state.RequireForUpdate<WorldMapSimulationSingleton>();
            state.RequireForUpdate<WorldMapSimulationState>();
            state.RequireForUpdate<WorldMapFocusState>();
            state.RequireForUpdate<SimulationRootState>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var day = SystemAPI.GetSingleton<GameCalendarState>().DayIndex;
            ref var sim = ref SystemAPI.GetSingletonRW<WorldMapSimulationState>().ValueRW;
            if (sim.LastProcessedDay == day)
                return;
            sim.LastProcessedDay = day;

            var focus = SystemAPI.GetSingleton<WorldMapFocusState>();
            var discovered = SystemAPI.GetSingletonBuffer<DiscoveredChunkEntry>(ref state);
            var territories = SystemAPI.GetSingletonBuffer<TerritoryControlEntry>(ref state);
            var nodes = SystemAPI.GetSingletonBuffer<WorldResourceNodeEntry>(ref state);
            var armies = SystemAPI.GetSingletonBuffer<StrategicArmyEntry>(ref state);
            var special = SystemAPI.GetSingletonBuffer<SpecialSiteEntry>(ref state);
            var tick = SystemAPI.GetSingleton<SimulationRootState>().SimulationTick;

            EnsureDiscovered(ref discovered, focus.PlayerChunk);
            UpdateTerritoryInfluence(ref territories, focus.PlayerChunk);
            UpdateArmies(ref armies);
            UpdateResourceOwnership(ref nodes, ref territories);
            DiscoverSpecialSites(ref special, ref discovered, tick, ref state);

            sim.DiscoveredChunksCount = (uint)discovered.Length;
            sim.ControlledResourceNodes = CountControlledResourceNodes(ref nodes);
            sim.ActiveArmiesMoving = CountMovingArmies(ref armies);

            AnalyticsHooks.Record(AnalyticsDomain.GlobalWorld, AnalyticsMetricIds.WorldMapDiscoveredChunks,
                sim.DiscoveredChunksCount);
            AnalyticsHooks.Record(AnalyticsDomain.GlobalWorld, AnalyticsMetricIds.WorldMapControlledResourceNodes,
                sim.ControlledResourceNodes);
            AnalyticsHooks.Record(AnalyticsDomain.GlobalWorld, AnalyticsMetricIds.WorldMapActiveArmiesMoving,
                sim.ActiveArmiesMoving);
            AnalyticsHooks.Record(AnalyticsDomain.GlobalWorld, AnalyticsMetricIds.WorldMapActiveScale,
                (byte)focus.ActiveScale);
        }

        private static void EnsureDiscovered(ref DynamicBuffer<DiscoveredChunkEntry> discovered, in MapChunkCoord chunk)
        {
            for (var i = 0; i < discovered.Length; i++)
            {
                if (discovered[i].Chunk.Equals(chunk))
                    return;
            }

            discovered.Add(new DiscoveredChunkEntry { Chunk = chunk });
        }

        private static void UpdateTerritoryInfluence(ref DynamicBuffer<TerritoryControlEntry> territories,
            in MapChunkCoord playerChunk)
        {
            for (var i = 0; i < territories.Length; i++)
            {
                var t = territories[i];
                var distance = WorldMapSimulationMath.DistanceKmBetweenChunks(playerChunk, t.Chunk);
                var influence = WorldMapSimulationMath.ComputeInfluencePercent(
                    distanceKm: distance,
                    militaryUnitsNearby: 1,
                    infrastructureLevel: 1,
                    populationTens: 4,
                    hasTradeRoute: true);
                t.InfluencePercent = influence;
                t.IsDisputed = (byte)(influence is > 40f and < 60f ? 1 : 0);
                territories[i] = t;
            }
        }

        private static void UpdateArmies(ref DynamicBuffer<StrategicArmyEntry> armies)
        {
            for (var i = 0; i < armies.Length; i++)
            {
                var army = armies[i];
                var baseSpeed = WorldMapSimulationMath.GetMovementSpeedKmPerDay(army.MovementMode, army.CurrentBiome);
                var speed = WorldMapSimulationMath.ApplyMovementPenalties(baseSpeed, army.Fatigue01, army.CarryingSupply != 0);
                army.DistanceRemainingKm = math.max(0f, army.DistanceRemainingKm - speed);

                army.Fatigue01 = math.saturate(army.Fatigue01 + 0.05f);
                if (army.DistanceRemainingKm <= 0.01f)
                {
                    army.CurrentChunk = army.DestinationChunk;
                    army.DestinationChunk = new MapChunkCoord(army.CurrentChunk.Grid.x + 1, army.CurrentChunk.Grid.y);
                    army.CurrentBiome = WorldMapBiomeSampling.GetBiomeForChunk(army.CurrentChunk);
                    army.DistanceRemainingKm = 20f;
                    army.Fatigue01 = math.max(0f, army.Fatigue01 - 0.4f);
                }

                armies[i] = army;
            }
        }

        private static void UpdateResourceOwnership(ref DynamicBuffer<WorldResourceNodeEntry> nodes,
            ref DynamicBuffer<TerritoryControlEntry> territories)
        {
            for (var i = 0; i < nodes.Length; i++)
            {
                var node = nodes[i];
                node.OwnerFaction = 0u;
                for (var j = 0; j < territories.Length; j++)
                {
                    var t = territories[j];
                    if (!t.Chunk.Equals(node.Chunk))
                        continue;
                    if (t.InfluencePercent < 50f)
                        continue;
                    node.OwnerFaction = t.OwnerFaction;
                    break;
                }

                nodes[i] = node;
            }
        }

        private static void DiscoverSpecialSites(ref DynamicBuffer<SpecialSiteEntry> sites,
            ref DynamicBuffer<DiscoveredChunkEntry> discovered, uint tick, ref SystemState state)
        {
            for (var i = 0; i < sites.Length; i++)
            {
                var site = sites[i];
                if (site.Discovered != 0)
                    continue;
                if (!IsChunkDiscovered(ref discovered, site.Chunk))
                    continue;

                site.Discovered = 1;
                sites[i] = site;
                TryEnqueueStoryEvent(ref state, tick, EventSpecialSiteDiscovered, new FixedString64Bytes("special-site"));
            }
        }

        private static bool IsChunkDiscovered(ref DynamicBuffer<DiscoveredChunkEntry> discovered, in MapChunkCoord chunk)
        {
            for (var i = 0; i < discovered.Length; i++)
            {
                if (discovered[i].Chunk.Equals(chunk))
                    return true;
            }

            return false;
        }

        private static uint CountControlledResourceNodes(ref DynamicBuffer<WorldResourceNodeEntry> nodes)
        {
            uint count = 0;
            for (var i = 0; i < nodes.Length; i++)
            {
                if (nodes[i].OwnerFaction != 0u)
                    count++;
            }

            return count;
        }

        private static uint CountMovingArmies(ref DynamicBuffer<StrategicArmyEntry> armies)
        {
            uint count = 0;
            for (var i = 0; i < armies.Length; i++)
            {
                if (armies[i].DistanceRemainingKm > 0.01f)
                    count++;
            }

            return count;
        }

        private static void TryEnqueueStoryEvent(ref SystemState state, uint tick, uint eventDefinitionId,
            in FixedString64Bytes label)
        {
            if (!SystemAPI.HasSingleton<StoryEventQueueSingleton>())
                return;
            var queue = SystemAPI.GetSingletonBuffer<GameEventQueueEntry>(ref state);
            queue.Add(new GameEventQueueEntry
            {
                Kind = StoryEventKind.Triggered,
                EventDefinitionId = eventDefinitionId,
                EnqueueSimulationTick = tick,
                DebugLabel = label
            });
        }
    }
}
