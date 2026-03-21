using Unity.Entities;

namespace ColonyConquest.Ecology
{
    /// <summary>Сводка загрязнения для UI/геймплея (обновление — EcologyPollutionSummarySystem).</summary>
    public struct ColonyPollutionSummaryState : IComponentData
    {
        public float CombinedPollutionPercent0to100;
        public PollutionLevelBand Band;
    }
}
