using ColonyConquest.Technology;
using Unity.Entities;

namespace ColonyConquest.Simulation
{
    /// <summary>Показатели научного прогресса до полноценного дерева технологий.</summary>
    public struct ColonyTechProgressState : IComponentData
    {
        public float ResearchPointsPerDay;
        public uint TechnologiesUnlocked;
        public float CurrentEraProgress01;
        public uint ResearchInstitutions;
        public uint ScientistsCount;

        /// <summary>Текущая эпоха для множителя §2.2 <c>spec/technology_tree_spec.md</c>.</summary>
        public TechEraId CurrentEra;

        /// <summary>Накопленные очки исследований (между «слайсами» прогресса эпохи).</summary>
        public float ResearchPointsAccumulated;

        /// <summary>Последний игровой день, на который начислены очки (чтобы не дублировать).</summary>
        public uint LastResearchDayIndex;
    }
}