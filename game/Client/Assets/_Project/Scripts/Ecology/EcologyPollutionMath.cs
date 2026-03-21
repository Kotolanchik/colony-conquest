using Unity.Mathematics;

namespace ColonyConquest.Ecology
{
    /// <summary>
    /// Сводный индекс загрязнения 0–100 и полосы §3 <c>spec/ecology_spec.md</c> (эффекты на здоровье, урожай, настроение).
    /// </summary>
    public static class EcologyPollutionMath
    {
        /// <summary>Среднее качество экологии 0…1 из синглтона индикаторов.</summary>
        public static float GetMeanEcosystemQuality01(in ColonyEcologyIndicatorsState s)
        {
            return math.saturate(
                (s.AirQuality01 + s.WaterQuality01 + s.SoilFertilityIndicator01 + s.ForestCover01 + s.Biodiversity01) *
                0.2f);
        }

        /// <summary>Сводное загрязнение 0 (чисто) … 100 (критично): инверсия среднего качества.</summary>
        public static float GetCombinedPollutionPercent0to100(in ColonyEcologyIndicatorsState s)
        {
            return (1f - GetMeanEcosystemQuality01(s)) * 100f;
        }

        public static PollutionLevelBand GetPollutionLevelBand(float pollutionPercent0to100)
        {
            var p = math.clamp(pollutionPercent0to100, 0f, 100f);
            if (p <= 20f)
                return PollutionLevelBand.Clean;
            if (p <= 40f)
                return PollutionLevelBand.Low;
            if (p <= 60f)
                return PollutionLevelBand.Medium;
            if (p <= 80f)
                return PollutionLevelBand.High;
            return PollutionLevelBand.Critical;
        }

        /// <summary>§3.1 — множитель к базовому здоровью поселенцев (1 = норма).</summary>
        public static float GetPopulationHealthMultiplier(PollutionLevelBand band)
        {
            return band switch
            {
                PollutionLevelBand.Clean => 1.1f,
                PollutionLevelBand.Low => 1f,
                PollutionLevelBand.Medium => 0.95f,
                PollutionLevelBand.High => 0.85f,
                PollutionLevelBand.Critical => 0.7f,
                _ => 1f
            };
        }

        /// <summary>§3.2 — множитель урожайности (1 = норма).</summary>
        public static float GetCropYieldMultiplier(PollutionLevelBand band)
        {
            return band switch
            {
                PollutionLevelBand.Clean => 1.05f,
                PollutionLevelBand.Low => 1f,
                PollutionLevelBand.Medium => 0.9f,
                PollutionLevelBand.High => 0.75f,
                PollutionLevelBand.Critical => 0.5f,
                _ => 1f
            };
        }

        /// <summary>§3.3 — множитель к базовому настроению (1 = норма).</summary>
        public static float GetColonyMoodMultiplier(PollutionLevelBand band)
        {
            return band switch
            {
                PollutionLevelBand.Clean => 1.1f,
                PollutionLevelBand.Low => 1f,
                PollutionLevelBand.Medium => 0.95f,
                PollutionLevelBand.High => 0.85f,
                PollutionLevelBand.Critical => 0.7f,
                _ => 1f
            };
        }
    }
}
