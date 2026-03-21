using Unity.Entities;

namespace ColonyConquest.Economy
{
    /// <summary>Демо-цех: одна очередь рецепта и прогресс цикла §2.2 (без здания в сцене).</summary>
    public struct EconomyWorkshopRuntime : IComponentData
    {
        public ProductionRecipeId ActiveRecipe;
        /// <summary>0…1 до завершения текущего цикла.</summary>
        public float Progress01;
        public byte AssignedWorkers;
        public float EnergyRatio01;
        public float ToolCondition01;
        public float AverageSkill0To100;
        public float BuildingWear01;
    }
}
