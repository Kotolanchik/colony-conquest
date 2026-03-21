namespace ColonyConquest.Bioengineering
{
    /// <summary>Параметры эффектов §3.1 <c>spec/bioengineering_spec.md</c> (игровое время; без симуляции зависимости §3.2).</summary>
    public readonly struct StimulantEffectDefinition
    {
        /// <summary>Длительность основного эффекта в игровых минутах.</summary>
        public readonly float DurationGameMinutes;
        public readonly float HpBonus;
        public readonly float MoveSpeedMultiplier;
        public readonly float DamageMultiplier;
        public readonly float DamageTakenMultiplier;
        public readonly float StaminaMultiplier;
        public readonly float StrengthMultiplier;
        public readonly bool IgnoresWounds;
        public readonly bool RadiationImmunity;
        public readonly bool NeutralizesPoison;
        public readonly float RegenHpPerGameSecond;
        /// <summary>HP после окончания (отрицательное = урон).</summary>
        public readonly float AftermathHpDelta;
        public readonly float AftermathAccuracyMultiplier;
        public readonly float AftermathMoveSpeedMultiplier;
        public readonly float AddictionRisk01;

        public StimulantEffectDefinition(
            float durationGameMinutes,
            float hpBonus,
            float moveSpeedMultiplier,
            float damageMultiplier,
            float damageTakenMultiplier,
            float staminaMultiplier,
            float strengthMultiplier,
            bool ignoresWounds,
            bool radiationImmunity,
            bool neutralizesPoison,
            float regenHpPerGameSecond,
            float aftermathHpDelta,
            float aftermathAccuracyMultiplier,
            float aftermathMoveSpeedMultiplier,
            float addictionRisk01)
        {
            DurationGameMinutes = durationGameMinutes;
            HpBonus = hpBonus;
            MoveSpeedMultiplier = moveSpeedMultiplier;
            DamageMultiplier = damageMultiplier;
            DamageTakenMultiplier = damageTakenMultiplier;
            StaminaMultiplier = staminaMultiplier;
            StrengthMultiplier = strengthMultiplier;
            IgnoresWounds = ignoresWounds;
            RadiationImmunity = radiationImmunity;
            NeutralizesPoison = neutralizesPoison;
            RegenHpPerGameSecond = regenHpPerGameSecond;
            AftermathHpDelta = aftermathHpDelta;
            AftermathAccuracyMultiplier = aftermathAccuracyMultiplier;
            AftermathMoveSpeedMultiplier = aftermathMoveSpeedMultiplier;
            AddictionRisk01 = addictionRisk01;
        }
    }

    public static class StimulantEffectTuning
    {
        public static StimulantEffectDefinition Get(StimulantKindId id)
        {
            return id switch
            {
                StimulantKindId.Stimpack => new StimulantEffectDefinition(
                    5f, 50f, 1.2f, 1f, 1f, 1f, 1f, false, false, false, 0f, -10f, 1f, 1f, 0f),
                StimulantKindId.MedX => new StimulantEffectDefinition(
                    10f, 100f, 1f, 1f, 1f, 1f, 1f, true, false, false, 0f, -20f, 1f, 1f, 0.1f),
                StimulantKindId.Psycho => new StimulantEffectDefinition(
                    5f, 0f, 1f, 1.5f, 0.5f, 1f, 1f, false, false, false, 0f, 0f, 0.7f, 1f, 0.2f),
                StimulantKindId.Buffout => new StimulantEffectDefinition(
                    10f, 0f, 1f, 1f, 1f, 1.5f, 1.2f, false, false, false, 0f, 0f, 1f, 0.8f, 0.15f),
                StimulantKindId.RadX => new StimulantEffectDefinition(
                    10f, 0f, 1f, 1f, 1f, 1f, 1f, false, true, false, 0f, 0f, 1f, 1f, 0f),
                StimulantKindId.Antidote => new StimulantEffectDefinition(
                    0f, 0f, 1f, 1f, 1f, 1f, 1f, false, false, true, 0f, 0f, 1f, 1f, 0f),
                StimulantKindId.Nanobots => new StimulantEffectDefinition(
                    1f, 0f, 1f, 1f, 1f, 1f, 1f, false, false, false, 10f, 0f, 1f, 1f, 0f),
                _ => default,
            };
        }
    }
}
