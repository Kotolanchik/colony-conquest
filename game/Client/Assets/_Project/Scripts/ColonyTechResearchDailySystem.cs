using ColonyConquest.Simulation;
using ColonyConquest.Technology;
using Unity.Entities;

namespace ColonyConquest.Core
{
    /// <summary>
    /// Начисление очков исследований по календарным суткам — §2.1–2.2 <c>spec/technology_tree_spec.md</c>
    /// (база × навыки учёных × институты × модификатор эпохи).
    /// </summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(ColonyCalendarAdvanceSystem))]
    public partial struct ColonyTechResearchDailySystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ColonyTechProgressState>();
            state.RequireForUpdate<GameCalendarState>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var cal = SystemAPI.GetSingleton<GameCalendarState>();
            ref var tech = ref SystemAPI.GetSingletonRW<ColonyTechProgressState>().ValueRW;

            if (tech.LastResearchDayIndex == 0 && cal.DayIndex > 1)
            {
                tech.LastResearchDayIndex = cal.DayIndex;
                return;
            }

            if (cal.DayIndex <= tech.LastResearchDayIndex)
                return;

            var days = cal.DayIndex - tech.LastResearchDayIndex;
            tech.LastResearchDayIndex = cal.DayIndex;

            var scientistMult = 1f + tech.ScientistsCount * 0.2f;
            var instMult = 1f + tech.ResearchInstitutions * TechResearchTuning.ResearchInstitutionBonusPerBuilding;
            var eraMult = TechResearchTuning.GetEraResearchMultiplier(tech.CurrentEra);
            var perDay = tech.ResearchPointsPerDay * scientistMult * instMult * eraMult;
            // Начисление «сырого» дохода исследований; распределение по активной технологии/эпохам
            // выполняет TechTreeDailySystem.
            tech.ResearchPointsAccumulated += perDay * days;
        }
    }
}
