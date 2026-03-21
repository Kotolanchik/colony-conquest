using Unity.Burst;

namespace ColonyConquest.Bioengineering
{
    /// <summary>
    /// Эффективность манипуляции / мобильности протеза 0…1 — <c>spec/bioengineering_spec.md</c> §2.2–2.3 (ориентиры).
    /// Без симуляции установки на посёленца.
    /// </summary>
    [BurstCompile]
    public static class ProsthesisEfficiencyMath
    {
        /// <summary>Эффективность руки/кисти (для задач «подработка», бой).</summary>
        public static float GetArmEfficiency01(CyberneticProsthesisKindId id)
        {
            return id switch
            {
                CyberneticProsthesisKindId.Hook => 0.5f,
                CyberneticProsthesisKindId.MechanicalArm => 0.7f,
                CyberneticProsthesisKindId.BionicArm => 1f,
                CyberneticProsthesisKindId.CombatProsthesis => 1f,
                _ => 0f,
            };
        }

        /// <summary>Множитель скорости передвижения относительно здоровой ноги (1 = норма).</summary>
        public static float GetLegSpeedMultiplier(CyberneticProsthesisKindId id)
        {
            return id switch
            {
                CyberneticProsthesisKindId.WoodenLeg => 0.5f,
                CyberneticProsthesisKindId.MechanicalLeg => 0.8f,
                CyberneticProsthesisKindId.BionicLeg => 1f,
                _ => 1f,
            };
        }
    }
}
