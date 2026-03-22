using Unity.Mathematics;

namespace ColonyConquest.WorldMap
{
    /// <summary>Формулы стратегической карты: влияние территорий и скорость армий.</summary>
    public static class WorldMapSimulationMath
    {
        public static float ComputeInfluencePercent(int distanceKm, int militaryUnitsNearby, int infrastructureLevel,
            int populationTens, bool hasTradeRoute)
        {
            var influence = 100f;
            influence -= distanceKm / 10f * 10f;
            influence += militaryUnitsNearby * 20f;
            influence += infrastructureLevel * 5f;
            influence += populationTens * 2f;
            if (hasTradeRoute)
                influence += 10f;
            return math.clamp(influence, 0f, 100f);
        }

        public static float GetMovementSpeedKmPerDay(StrategicMovementMode mode, WorldBiomeId biome)
        {
            return mode switch
            {
                StrategicMovementMode.Foot => biome switch
                {
                    WorldBiomeId.MixedForest => 15f,
                    WorldBiomeId.Mountains => 10f,
                    WorldBiomeId.Swamp => 5f,
                    _ => 20f
                },
                StrategicMovementMode.Mounted => biome switch
                {
                    WorldBiomeId.MixedForest => 30f,
                    WorldBiomeId.Mountains => 15f,
                    WorldBiomeId.Swamp => 10f,
                    _ => 40f
                },
                StrategicMovementMode.Mechanized => biome switch
                {
                    WorldBiomeId.MixedForest => 40f,
                    WorldBiomeId.Mountains => 20f,
                    WorldBiomeId.Swamp => 0f,
                    _ => 60f
                },
                StrategicMovementMode.Rail => 200f,
                StrategicMovementMode.Air => 500f,
                StrategicMovementMode.Naval => 300f,
                _ => 20f
            };
        }

        public static float ApplyMovementPenalties(float speedKmPerDay, float fatigue01, bool carryingSupply)
        {
            var result = speedKmPerDay;
            result *= math.lerp(1f, 0.5f, math.saturate(fatigue01));
            if (carryingSupply)
                result *= 0.8f;
            return math.max(0f, result);
        }

        public static int DistanceKmBetweenChunks(in MapChunkCoord a, in MapChunkCoord b)
        {
            var dx = math.abs(a.Grid.x - b.Grid.x);
            var dy = math.abs(a.Grid.y - b.Grid.y);
            var chunkKm = WorldMapChunkMetrics.LocalChunkWidthMeters / 1000f;
            return (int)math.round(math.sqrt(dx * dx + dy * dy) * chunkKm);
        }
    }
}
