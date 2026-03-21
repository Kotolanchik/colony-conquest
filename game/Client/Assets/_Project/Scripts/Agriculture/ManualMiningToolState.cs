using Unity.Entities;

namespace ColonyConquest.Agriculture
{
    /// <summary>
    /// Состояние инструмента и шахтёра для ручной добычи — см. <c>spec/agriculture_mining_spec.md</c> §2.2.
    /// </summary>
    public struct ManualMiningToolState : IComponentData
    {
        public MiningPickaxeTierId Tier;
        public float DurabilityRemaining;
        public byte MinerSkillLevel;
        public float SessionWorkHours;
    }
}
