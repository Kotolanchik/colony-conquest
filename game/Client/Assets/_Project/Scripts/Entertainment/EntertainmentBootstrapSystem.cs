using Unity.Entities;

namespace ColonyConquest.Entertainment
{
    /// <summary>Инициализация синглтонов развлечений и праздничного календаря.</summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct EntertainmentBootstrapSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            if (!SystemAPI.HasSingleton<EntertainmentSimulationSingleton>())
            {
                var e = state.EntityManager.CreateEntity();
                state.EntityManager.AddComponent<EntertainmentSimulationSingleton>(e);
                state.EntityManager.AddComponentData(e, new EntertainmentSimulationState
                {
                    LastProcessedDay = uint.MaxValue,
                    BaseMood = 52f,
                    Diversity = 58f,
                    Quality = 54f,
                    Availability = 49f,
                    FinalMood = 0f,
                    ProductivityModifier = 0f,
                    StressReduction = 0f,
                    GamblingRisk = 0f,
                    GamblingPolicy = 1
                });
            }

            if (!SystemAPI.HasSingleton<EntertainmentFestivalState>())
            {
                state.EntityManager.CreateSingleton(new EntertainmentFestivalState
                {
                    LastFestivalDay = uint.MaxValue,
                    HolidayMoodBonus = 0f
                });
            }
        }
    }
}
