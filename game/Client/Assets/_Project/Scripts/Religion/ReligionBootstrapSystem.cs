using Unity.Entities;

namespace ColonyConquest.Religion
{
    /// <summary>Инициализация сводных религиозных синглтонов и параметров AI-политики.</summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct ReligionBootstrapSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            if (!SystemAPI.HasSingleton<ReligionSimulationState>())
            {
                state.EntityManager.CreateSingleton(new ReligionSimulationState
                {
                    LastProcessedDay = uint.MaxValue,
                    FaithLevelAvg = 46f,
                    FanaticismAvg = 25f,
                    DoubtAvg = 30f,
                    TempleVisit = 0.8f,
                    SermonExposure = 0.6f,
                    PersonalCrisis = 0.3f,
                    CommunityCohesion = 0.5f,
                    EducationPressure = 0.4f,
                    ProsperitySecularPull = 0.3f
                });
            }

            if (!SystemAPI.HasSingleton<ReligiousConflictState>())
            {
                state.EntityManager.CreateSingleton(new ReligiousConflictState
                {
                    IncidentWeight = 18f,
                    IdeologyDistance = 40f,
                    ResourceCompetition = 30f,
                    PropagandaPressure = 22f,
                    LawEnforcement = 28f,
                    DialoguePrograms = 24f,
                    TensionScore = 45f
                });
            }

            if (!SystemAPI.HasSingleton<CultActivityState>())
            {
                state.EntityManager.CreateSingleton(new CultActivityState
                {
                    RecruitmentRate = 0.3f,
                    HiddenCells = 0.2f,
                    RadicalizationRisk = 30f
                });
            }

            if (!SystemAPI.HasSingleton<HolyWarState>())
            {
                state.EntityManager.CreateSingleton(new HolyWarState
                {
                    Phase = HolyWarPhase.None,
                    DayInPhase = 0,
                    Preparedness = 55f,
                    TargetFaction = 1u,
                    CasusBelli = 1,
                    ActiveDaysRemaining = 0
                });
            }

            if (!SystemAPI.HasSingleton<ReligionFactionPolicyState>())
            {
                state.EntityManager.CreateSingleton(new ReligionFactionPolicyState
                {
                    Archetype = ReligionArchetypeId.Monotheism,
                    MissionaryWeight = 0.9f,
                    ToleranceWeight = 0.2f,
                    CoercionWeight = 0.7f,
                    SecularizationWeight = 0f
                });
            }
        }
    }
}
