using Unity.Entities;

namespace ColonyConquest.Justice
{
    /// <summary>Инициализация синглтонов преступности, полиции и суда.</summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct CrimeJusticeBootstrapSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            if (SystemAPI.HasSingleton<CrimeJusticeSingleton>())
                return;

            var e = state.EntityManager.CreateEntity();
            state.EntityManager.AddComponent<CrimeJusticeSingleton>(e);
            state.EntityManager.AddComponentData(e, new CrimeJusticeState
            {
                LastProcessedDay = uint.MaxValue,
                CrimeLevelPercent = 28f,
                PovertyPercent = 24f,
                UnemploymentPercent = 11f,
                InequalityGini = 0.34f,
                IlliteracyPercent = 8f,
                PoliceCoveragePercent = 58f,
                CorruptionLevel = 0.22f,
                OvercrowdingThousands = 0.6f,
                EntertainmentAccess01 = 0.45f,
                ReligiousPopulationPercent = 40f,
                PenaltySeverity = 0.4f
            });

            state.EntityManager.CreateSingleton(new PoliceForceState
            {
                BaseEfficiency = 1f,
                SkillLevel = 0.5f,
                EquipmentLevel = 0.4f,
                OfficersPerPopulation = 0.012f
            });

            state.EntityManager.CreateSingleton(new JusticeCourtState
            {
                CourtType = 1,
                Fairness01 = 0.8f,
                Corruption01 = 0.2f,
                PrisonConditions01 = 0.55f,
                RehabPrograms01 = 0.35f,
                OrganizedCrimePressure01 = 0.25f
            });

            state.EntityManager.CreateSingleton(new CrimeIncidentStatsState
            {
                IncidentsToday = 0,
                SolvedToday = 0,
                InmatesCount = 8,
                RecidivistsCount = 1,
                DeathPenaltyCount = 0
            });
        }
    }
}
