using Unity.Burst;

namespace ColonyConquest.Agriculture
{
    /// <summary>
    /// Формула урожая — <c>spec/agriculture_mining_spec.md</c> §1.3 (факторы из §1.2).
    /// </summary>
    [BurstCompile]
    public static class CropYieldFormulas
    {
        /// <param name="fertilizerBonus">Бонус удобрений как доля, напр. 0.2 = +20%.</param>
        /// <param name="pestDamageFactor01">Доля потерь от вредителей 0…1.</param>
        /// <param name="weedDamageFactor01">Доля потерь от сорняков 0…1.</param>
        /// <param name="waterMultiplier">0.5 без воды, 1.0 норма, 1.2 автополив (§1.3).</param>
        public static float ComputeYield(
            float baseYield,
            float soilFertilityMultiplier,
            float fertilizerBonus,
            byte farmerSkillLevel,
            float weatherMultiplier,
            float seasonalMultiplier,
            float pestDamageFactor01,
            float weedDamageFactor01,
            float waterMultiplier)
        {
            var skill = 1f + farmerSkillLevel * 0.05f;
            var pests = 1f - pestDamageFactor01;
            var weeds = 1f - weedDamageFactor01;
            if (pests < 0f)
                pests = 0f;
            if (weeds < 0f)
                weeds = 0f;
            return baseYield * soilFertilityMultiplier
                   * (1f + fertilizerBonus)
                   * skill
                   * weatherMultiplier
                   * seasonalMultiplier
                   * pests
                   * weeds
                   * waterMultiplier;
        }
    }
}
