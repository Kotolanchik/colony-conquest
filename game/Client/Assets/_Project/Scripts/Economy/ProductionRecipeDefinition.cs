namespace ColonyConquest.Economy
{
    /// <summary>Один рецепт: входы (до 3), выход, длительность, оптимальное число рабочих §2.2.</summary>
    public readonly struct ProductionRecipeDefinition
    {
        public readonly ProductionRecipeId Id;
        public readonly ResourceId In0;
        public readonly float Amount0;
        public readonly ResourceId In1;
        public readonly float Amount1;
        public readonly ResourceId In2;
        public readonly float Amount2;
        public readonly ResourceId Output;
        public readonly float OutputAmount;
        public readonly float DurationSeconds;
        public readonly byte OptimalWorkers;

        public ProductionRecipeDefinition(
            ProductionRecipeId id,
            ResourceId in0,
            float amount0,
            ResourceId in1,
            float amount1,
            ResourceId in2,
            float amount2,
            ResourceId output,
            float outputAmount,
            float durationSeconds,
            byte optimalWorkers)
        {
            Id = id;
            In0 = in0;
            Amount0 = amount0;
            In1 = in1;
            Amount1 = amount1;
            In2 = in2;
            Amount2 = amount2;
            Output = output;
            OutputAmount = outputAmount;
            DurationSeconds = durationSeconds;
            OptimalWorkers = optimalWorkers;
        }
    }
}
