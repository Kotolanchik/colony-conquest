using Unity.Entities;

namespace ColonyConquest.Housing
{
    /// <summary>Создаёт демо-жильё, синглтон колонии и очередь заявок на расселение.</summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct HousingBootstrapSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            if (!SystemAPI.HasSingleton<HousingColonyState>())
            {
                state.EntityManager.CreateSingleton(new HousingColonyState
                {
                    LastProcessedDay = uint.MaxValue,
                    TotalCapacity = 0,
                    TotalResidents = 0,
                    OvercrowdedUnits = 0,
                    IncidentsToday = 0,
                    AssignmentBacklog = 0,
                    AverageComfort = 0f
                });
            }

            if (!SystemAPI.HasSingleton<HousingAssignmentProcessState>())
            {
                state.EntityManager.CreateSingleton(new HousingAssignmentProcessState
                {
                    LastProcessedDay = uint.MaxValue
                });
            }

            if (!SystemAPI.HasSingleton<HousingAssignmentQueueSingleton>())
            {
                var qEntity = state.EntityManager.CreateEntity();
                state.EntityManager.AddComponent<HousingAssignmentQueueSingleton>(qEntity);
                var queue = state.EntityManager.AddBuffer<HousingAssignmentRequestEntry>(qEntity);
                queue.Add(new HousingAssignmentRequestEntry
                {
                    RequestId = 1,
                    Priority = 3,
                    HouseholdSize = 4,
                    WorkProximityWeight = 0.8f,
                    FamilyNeedWeight = 1f,
                    ComfortNeedWeight = 0.6f
                });
                queue.Add(new HousingAssignmentRequestEntry
                {
                    RequestId = 2,
                    Priority = 2,
                    HouseholdSize = 2,
                    WorkProximityWeight = 0.5f,
                    FamilyNeedWeight = 0.4f,
                    ComfortNeedWeight = 1f
                });
            }

            var existingUnits = state.GetEntityQuery(ComponentType.ReadOnly<HousingUnitRuntime>()).CalculateEntityCount();
            if (existingUnits > 0)
                return;

            CreateHousingUnit(ref state, new HousingUnitRuntime
            {
                Type = HousingTypeId.Hut,
                Capacity = 4,
                Residents = 2,
                BaseComfort = 35f,
                PowerCoverage01 = 0f,
                WaterCoverage01 = 0.35f,
                SewageCoverage01 = 0.1f,
                HeatingCoverage01 = 0.5f,
                Cleanliness01 = 0.6f,
                Noise01 = 0.2f,
                NeighborhoodQuality01 = 0.45f,
                DecorLevel01 = 0.15f,
                Condition01 = 0.85f,
                BaseDecayPerDay = 0.0035f,
                MaintenanceRepairPerDay = 0.0020f,
                DistanceToWork01 = 0.4f,
                IsBarracks = 0
            });

            CreateHousingUnit(ref state, new HousingUnitRuntime
            {
                Type = HousingTypeId.House,
                Capacity = 6,
                Residents = 5,
                BaseComfort = 62f,
                PowerCoverage01 = 0.7f,
                WaterCoverage01 = 0.8f,
                SewageCoverage01 = 0.7f,
                HeatingCoverage01 = 0.65f,
                Cleanliness01 = 0.78f,
                Noise01 = 0.15f,
                NeighborhoodQuality01 = 0.7f,
                DecorLevel01 = 0.55f,
                Condition01 = 0.92f,
                BaseDecayPerDay = 0.0022f,
                MaintenanceRepairPerDay = 0.0034f,
                DistanceToWork01 = 0.3f,
                IsBarracks = 0
            });

            var barracks = CreateHousingUnit(ref state, new HousingUnitRuntime
            {
                Type = HousingTypeId.Barracks,
                Capacity = 20,
                Residents = 18,
                BaseComfort = 48f,
                PowerCoverage01 = 0.8f,
                WaterCoverage01 = 0.75f,
                SewageCoverage01 = 0.65f,
                HeatingCoverage01 = 0.8f,
                Cleanliness01 = 0.65f,
                Noise01 = 0.35f,
                NeighborhoodQuality01 = 0.55f,
                DecorLevel01 = 0.2f,
                Condition01 = 0.88f,
                BaseDecayPerDay = 0.003f,
                MaintenanceRepairPerDay = 0.003f,
                DistanceToWork01 = 0.2f,
                IsBarracks = 1
            });
            state.EntityManager.AddComponentData(barracks, new BarracksPolicyData
            {
                MinSpacePerSoldier = 3f,
                DisciplineBonus = 0.1f,
                RestRecoveryRate = 0.12f
            });
        }

        private static Entity CreateHousingUnit(ref SystemState state, in HousingUnitRuntime runtime)
        {
            var entity = state.EntityManager.CreateEntity();
            state.EntityManager.AddComponentData(entity, runtime);
            state.EntityManager.AddComponentData(entity, default(HousingComfortSnapshot));
            return entity;
        }
    }
}
