using ColonyConquest.Simulation;
using ColonyConquest.Technology;
using Unity.Entities;

namespace ColonyConquest.WorldMap
{
    /// <summary>Меняет активный уровень масштаба карты по текущей технологической эпохе.</summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(ColonyTechResearchDailySystem))]
    public partial struct WorldMapScaleFromTechSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<WorldMapFocusState>();
            state.RequireForUpdate<ColonyTechProgressState>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var era = SystemAPI.GetSingleton<ColonyTechProgressState>().CurrentEra;
            ref var focus = ref SystemAPI.GetSingletonRW<WorldMapFocusState>().ValueRW;
            focus.ActiveScale = era switch
            {
                TechEraId.Era1_Foundation => WorldMapScaleLevel.Local,
                TechEraId.Era2_Industrialization => WorldMapScaleLevel.Regional,
                TechEraId.Era3_WorldWar1 => WorldMapScaleLevel.Regional,
                _ => WorldMapScaleLevel.Global
            };
        }
    }
}
