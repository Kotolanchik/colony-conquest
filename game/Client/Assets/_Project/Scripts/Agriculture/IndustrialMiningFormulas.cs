using Unity.Burst;

namespace ColonyConquest.Agriculture
{
    /// <summary>Производительность промышленной добычи (ед/час или л/час для нефти) — §2.2.</summary>
    [BurstCompile]
    public static class IndustrialMiningFormulas
    {
        /// <summary>Нефтяная вышка: 200 л/час — возвращаем те же единицы для единообразия пайплайна.</summary>
        public static float GetNominalOutputPerGameHour(IndustrialMiningMethodId method)
        {
            return method switch
            {
                IndustrialMiningMethodId.Mine => 50f,
                IndustrialMiningMethodId.OpenQuarry => 100f,
                IndustrialMiningMethodId.Adit => 80f,
                IndustrialMiningMethodId.GoldDredge => 30f,
                IndustrialMiningMethodId.OilPumpjack => 200f,
                IndustrialMiningMethodId.OpenPit => 500f,
                IndustrialMiningMethodId.DeepMine => 300f,
                IndustrialMiningMethodId.AutomatedMine => 1000f,
                IndustrialMiningMethodId.AsteroidMining => 2000f,
                _ => 0f
            };
        }
    }
}
