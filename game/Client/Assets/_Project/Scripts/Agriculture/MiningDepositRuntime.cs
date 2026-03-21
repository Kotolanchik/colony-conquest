using Unity.Entities;

namespace ColonyConquest.Agriculture
{
    /// <summary>
    /// Экземпляр месторождения в мире (остаток запаса) — см. <c>spec/agriculture_mining_spec.md</c> §2.3.
    /// Логика добычи — отдельные системы.
    /// </summary>
    public struct MiningDepositRuntime : IComponentData
    {
        public MiningDepositKindId Kind;
        public float AmountRemaining;
        /// <summary>Начальный запас для §2.3 (качество руды, исчерпание). 0 — инициализировать из <see cref="AmountRemaining"/> при первом тике.</summary>
        public float InitialAmount;
    }
}
