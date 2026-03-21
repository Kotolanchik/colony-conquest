using Unity.Burst;
using Unity.Mathematics;

namespace ColonyConquest.Agriculture
{
    /// <summary>
    /// Формулы ручной добычи и исчерпания — <c>spec/agriculture_mining_spec.md</c> §2.2–2.3.
    /// </summary>
    [BurstCompile]
    public static class MiningManualFormulas
    {
        public static float GetBaseUnitsPerGameHour(MiningPickaxeTierId tier)
        {
            return tier switch
            {
                MiningPickaxeTierId.Stone => 1f,
                MiningPickaxeTierId.Copper => 2f,
                MiningPickaxeTierId.Bronze => 3f,
                MiningPickaxeTierId.Iron => 5f,
                MiningPickaxeTierId.Steel => 8f,
                _ => 0f
            };
        }

        public static float GetMaxDurability(MiningPickaxeTierId tier)
        {
            return tier switch
            {
                MiningPickaxeTierId.Stone => 20f,
                MiningPickaxeTierId.Copper => 50f,
                MiningPickaxeTierId.Bronze => 100f,
                MiningPickaxeTierId.Iron => 200f,
                MiningPickaxeTierId.Steel => 500f,
                _ => 0f
            };
        }

        /// <summary>Навык шахтёра: +10% за уровень (§2.2).</summary>
        public static float GetMinerSkillMultiplier(byte minerSkillLevel)
        {
            return 1f + 0.10f * minerSkillLevel;
        }

        /// <summary>Усталость: −1% эффективности за час непрерывной работы (§2.2).</summary>
        public static float GetFatigueMultiplier(float sessionWorkHours)
        {
            var m = 1f - 0.01f * sessionWorkHours;
            return math.clamp(m, 0.2f, 1f);
        }

        /// <summary>
        /// Качество руды по доле <b>уже добытого</b> запаса — <c>spec/agriculture_mining_spec.md</c> §2.3:
        /// богатая жила (первые 20% выработки) 80–100% содержания → среднее ~0.9 к выходу ресурса;
        /// средняя (следующие 50%) 50–80% → ~0.65; бедная (последние 30%) 20–50% → ~0.35.
        /// </summary>
        public static float GetOreContentAverageMultiplier(float fractionExtractedFromInitial)
        {
            var f = math.saturate(fractionExtractedFromInitial);
            if (f < 0.2f)
                return 0.9f;
            if (f < 0.7f)
                return 0.65f;
            return 0.35f;
        }

        /// <summary>
        /// Эффект исчерпания по доле <b>оставшегося</b> запаса (§2.3): при остатке &gt;75% — без штрафа;
        /// далее ступени до закрытия шахты при 0.
        /// </summary>
        public static float GetDepletionProductionMultiplier(float remainingFractionOfInitial)
        {
            var r = remainingFractionOfInitial;
            if (r <= 0f)
                return 0f;
            if (r > 0.75f)
                return 1f;
            if (r > 0.5f)
                return 0.9f;
            if (r > 0.25f)
                return 0.75f;
            if (r > 0.1f)
                return 0.5f;
            return 0.25f;
        }

        public static bool UsesOreQualityBands(MiningDepositKindId kind)
        {
            return kind switch
            {
                MiningDepositKindId.None => false,
                MiningDepositKindId.Forest => false,
                MiningDepositKindId.StoneQuarry => false,
                MiningDepositKindId.Clay => false,
                MiningDepositKindId.Sand => false,
                _ => true
            };
        }

        /// <summary>§2.1: лес — топор; прочее — кирка/бур. Для прототипа лес даёт небольшой бонус к скорости при тех же тирах инструмента.</summary>
        public static float GetManualGatherKindMultiplier(MiningDepositKindId kind)
        {
            return kind == MiningDepositKindId.Forest ? 1.12f : 1f;
        }
    }
}
