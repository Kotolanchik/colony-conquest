using Unity.Entities;

namespace ColonyConquest.Agriculture
{
    /// <summary>Состояние годового тика восстановления месторождений — §2.3 (лес).</summary>
    public struct MiningWorldRegenerationState : IComponentData
    {
        /// <summary>Последний игровой год, в котором уже применялось восстановление (DayIndex / DaysPerGameYear).</summary>
        public uint LastProcessedGameYear;
    }
}
