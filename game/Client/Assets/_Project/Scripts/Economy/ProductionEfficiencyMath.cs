using Unity.Burst;
using Unity.Mathematics;

namespace ColonyConquest.Economy
{
    /// <summary>Формула §2.1 <c>spec/economic_system_specification.md</c> (множитель к базовой скорости).</summary>
    [BurstCompile]
    public static class ProductionEfficiencyMath
    {
        /// <param name="energyRatio01">1 = полная энергия, 0 = минимум (модификатор падает до 0.3).</param>
        /// <param name="toolCondition01">1 = без износа инструмента, 0 = полный износ (скорость ×0.5 к базе инструмента).</param>
        /// <param name="averageSkill0To100">Средний навык 0…100 для множителя мастерства.</param>
        /// <param name="buildingWear01">Износ здания / макс. износ (штраф к скорости).</param>
        public static float ComputeSpeedMultiplier(
            float workerCount,
            float optimalWorkers,
            float energyRatio01,
            float toolCondition01,
            float averageSkill0To100,
            float buildingWear01)
        {
            optimalWorkers = math.max(optimalWorkers, 1e-3f);
            var ratio = workerCount / optimalWorkers;
            var modWorkers = 0.5f + math.pow(ratio, 0.7f);
            var modEnergy = 0.3f + 0.7f * math.saturate(energyRatio01);
            var modTools = 0.5f + 0.5f * math.saturate(toolCondition01);
            var modMastery = 1f + (math.saturate(averageSkill0To100) / 100f) * 0.5f;
            var modWear = 1f - math.saturate(buildingWear01);
            return modWorkers * modEnergy * modTools * modMastery * modWear;
        }
    }
}
