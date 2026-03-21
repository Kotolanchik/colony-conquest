namespace ColonyConquest.Agriculture
{
    /// <summary>Вероятности §2.4 (за игровой день на одного добывающего у узла).</summary>
    public static class MiningHazardTuning
    {
        public const float CaveInDaily = 0.01f;
        public const float SuffocationDaily = 0.005f;
        public const float GasExplosionDaily = 0.002f;
        public const float FloodingDaily = 0.003f;
        /// <summary>Уран §2.4 — «постоянно» моделируем как ежедневный риск при работе у узла.</summary>
        public const float RadiationExposureDaily = 0.02f;
    }
}
