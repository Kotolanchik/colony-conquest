using Unity.Entities;

namespace ColonyConquest.Ecology
{
    /// <summary>Создаёт singleton экологии, меры защиты и демо-источники загрязнения.</summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct EcologySimulationBootstrapSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            if (SystemAPI.HasSingleton<EcologySimulationSingleton>())
                return;

            var em = state.EntityManager;
            var entity = em.CreateEntity();
            em.AddComponent<EcologySimulationSingleton>(entity);
            em.AddComponent(entity, new EcologySimulationState
            {
                LastProcessedDay = uint.MaxValue,
                AirPollutionUnitsPerDay = 0f,
                WaterPollutionUnitsPerDay = 0f,
                SoilContaminationUnitsPerDay = 0f,
                GreenhouseGasIndex = 15f,
                TemperatureAnomalyC = 0.2f,
                SeaLevelRiseMeters = 0f,
                ExtremeWeatherRisk01 = 0.1f,
                EcosystemHealth01 = 0.88f,
                SustainableDevelopment01 = 0.62f,
                EcologicalEventsTotal = 0u,
                CatastrophesTotal = 0u
            });
            em.AddComponent(entity, new EcologyMitigationState
            {
                AirCleanupLevel = 1,
                WaterCleanupLevel = 1,
                SoilRestorationLevel = 1,
                ReforestationIntensity01 = 0.35f,
                WildlifeProtection01 = 0.30f,
                CarbonCapture01 = 0.08f,
                GeoengineeringEnabled = 0
            });
            em.AddComponent(entity, new EcologyDisasterState
            {
                LastDisasterType = 0,
                LastDisasterDay = uint.MaxValue
            });

            var air = em.AddBuffer<EcologyAirSourceEntry>(entity);
            air.Add(new EcologyAirSourceEntry
            {
                SourceId = EcologyAirPollutionSourceId.CampfireOrFurnace,
                Count = 8,
                ActiveHoursPerDay = 7f,
                Utilization01 = 0.9f
            });
            air.Add(new EcologyAirSourceEntry
            {
                SourceId = EcologyAirPollutionSourceId.BlastFurnace,
                Count = 2,
                ActiveHoursPerDay = 16f,
                Utilization01 = 0.85f
            });
            air.Add(new EcologyAirSourceEntry
            {
                SourceId = EcologyAirPollutionSourceId.SteamEngine,
                Count = 3,
                ActiveHoursPerDay = 14f,
                Utilization01 = 0.8f
            });

            var water = em.AddBuffer<EcologyWaterSourceEntry>(entity);
            water.Add(new EcologyWaterSourceEntry
            {
                SourceId = EcologyWaterPollutionSourceId.UntreatedSewage,
                Count = 4,
                ActiveHoursPerDay = 24f,
                SpillChancePerDay01 = 0f
            });
            water.Add(new EcologyWaterSourceEntry
            {
                SourceId = EcologyWaterPollutionSourceId.MineDrainage,
                Count = 2,
                ActiveHoursPerDay = 12f,
                SpillChancePerDay01 = 0f
            });
            water.Add(new EcologyWaterSourceEntry
            {
                SourceId = EcologyWaterPollutionSourceId.IndustrialEffluent,
                Count = 2,
                ActiveHoursPerDay = 16f,
                SpillChancePerDay01 = 0.02f
            });

            var soil = em.AddBuffer<EcologySoilSourceEntry>(entity);
            soil.Add(new EcologySoilSourceEntry
            {
                SourceId = EcologySoilImpactSourceId.Manure,
                Intensity01 = 0.4f
            });
            soil.Add(new EcologySoilSourceEntry
            {
                SourceId = EcologySoilImpactSourceId.ChemicalFertilizers,
                Intensity01 = 0.2f
            });
            soil.Add(new EcologySoilSourceEntry
            {
                SourceId = EcologySoilImpactSourceId.Pesticides,
                Intensity01 = 0.15f
            });
        }
    }
}
