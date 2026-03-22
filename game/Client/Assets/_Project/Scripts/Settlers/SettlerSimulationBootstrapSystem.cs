using ColonyConquest.Simulation;
using Unity.Collections;
using Unity.Entities;

namespace ColonyConquest.Settlers
{
    /// <summary>Создаёт singleton и стартовую популяцию поселенцев.</summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct SettlerSimulationBootstrapSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            if (SystemAPI.HasSingleton<SettlerSimulationSingleton>())
                return;

            var em = state.EntityManager;
            var singletonEntity = em.CreateEntity();
            em.AddComponent<SettlerSimulationSingleton>(singletonEntity);
            em.AddComponent(singletonEntity, new SettlerSimulationState
            {
                LastProcessedDay = uint.MaxValue,
                LastSettlerId = 0,
                PopulationAlive = 0,
                BirthsTotal = 0,
                DeathsTotal = 0,
                MentalBreaksTotal = 0,
                InteractionEventsTotal = 0,
                AverageMood = 50f,
                AverageStress = 10f,
                AverageHealth01 = 0.9f,
                AverageWorkEfficiency01 = 1f,
                ColonyMorale01 = 0.6f,
                ResourceFoodDemandPerDay = 0f,
                ResourceFoodSatisfied01 = 1f,
                WaterReserveUnits = 250f,
                EducationIndex01 = 0.45f
            });
            em.AddComponent(singletonEntity, new SettlerAutonomyPolicyState
            {
                DefaultAutonomyLevel = 1,
                GlobalAlertLevel = 0,
                SafetyOverrideHealthThreshold = 25f
            });
            em.AddBuffer<SettlerRelationshipEdge>(singletonEntity);

            const int InitialPopulation = 32;
            uint lastId = 0;
            for (var i = 0; i < InitialPopulation; i++)
            {
                lastId++;
                var veteran = i % 7 == 0;
                var faction = (byte)(i % 3);
                SettlerEntityFactory.CreateSettler(em, lastId, 0, 0xA531u + (uint)i * 17u, faction, veteran);
            }

            SeedInitialBonds(ref em);

            var sim = em.GetComponentData<SettlerSimulationState>(singletonEntity);
            sim.LastSettlerId = lastId;
            sim.PopulationAlive = InitialPopulation;
            em.SetComponentData(singletonEntity, sim);

            if (SystemAPI.HasSingleton<ColonyDemographyState>())
            {
                ref var demography = ref SystemAPI.GetSingletonRW<ColonyDemographyState>().ValueRW;
                demography.Population = InitialPopulation;
            }
        }

        private static void SeedInitialBonds(ref EntityManager em)
        {
            using var query = em.CreateEntityQuery(
                ComponentType.ReadOnly<SettlerRuntimeId>(),
                ComponentType.ReadWrite<SocialBonds>());
            using var entities = query.ToEntityArray(Allocator.Temp);
            if (entities.Length < 2)
                return;

            for (var i = 0; i < entities.Length; i++)
            {
                var self = entities[i];
                var social = em.GetComponentData<SocialBonds>(self);
                var next = entities[(i + 1) % entities.Length];
                var nextId = em.GetComponentData<SettlerRuntimeId>(next).Value;
                social.Bonds.Add(new SocialBond
                {
                    TargetId = (int)nextId,
                    Value = 18,
                    BondType = 2,
                    Duration = 1,
                    InteractionCount = 1
                });

                if (i % 12 == 0)
                    social.PartnerId = (int)nextId;
                else if (i % 7 == 0)
                    social.MentorId = (int)nextId;
                else if (i % 9 == 0)
                    social.RivalId = (int)nextId;

                em.SetComponentData(self, social);
            }
        }
    }
}
