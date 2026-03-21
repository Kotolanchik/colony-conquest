using Unity.Mathematics;

namespace ColonyConquest.Agriculture
{
    /// <summary>Сезонность и обёртка над <see cref="CropYieldFormulas"/> §1.3; <c>spec/agriculture_mining_spec.md</c>.</summary>
    public static class CropYieldMath
    {
        /// <summary>Сезон §1.2: +10% в подходящий сезон, −50% в неподходящий.</summary>
        public static float SeasonalModifier(SeasonId current, SeasonId preferred)
        {
            return current == preferred ? 1.1f : 0.5f;
        }

        /// <summary>Урожай через <see cref="CropYieldFormulas.ComputeYield"/>; уровень навыка приводится к byte.</summary>
        public static float ComputeHarvestYield(
            float baseYieldPerTile,
            float soilFertility01,
            float fertilizerBonus,
            float farmerSkillLevel,
            float weatherModifier,
            float seasonalModifier,
            float pestDamage01,
            float weedDamage01,
            float waterMultiplier)
        {
            var skillByte = (byte)math.clamp(farmerSkillLevel, 0f, 255f);
            return CropYieldFormulas.ComputeYield(
                baseYieldPerTile,
                soilFertility01,
                fertilizerBonus,
                skillByte,
                weatherModifier,
                seasonalModifier,
                pestDamage01,
                weedDamage01,
                waterMultiplier);
        }

        /// <summary>Алиас для систем, ожидающих имя из раннего прототипа.</summary>
        public static float ComputeHarvest(
            float baseYieldPerTile,
            float soilFertility,
            float fertilizerBonus,
            float farmerSkillLevel,
            float weatherModifier,
            float seasonalModifier,
            float pestDamage,
            float weedDamage,
            float waterModifier) =>
            ComputeHarvestYield(
                baseYieldPerTile,
                soilFertility,
                fertilizerBonus,
                farmerSkillLevel,
                weatherModifier,
                seasonalModifier,
                pestDamage,
                weedDamage,
                waterModifier);
    }
}
