using Unity.Entities;

namespace ColonyConquest.Analytics
{
    /// <summary>Запись метрики в локальном кольце (до интеграции с бэкендом).</summary>
    public struct AnalyticsRecordEntry : IBufferElementData
    {
        public AnalyticsDomain Domain;
        public uint MetricId;
        public float Value;
        public uint SimulationTick;
    }
}
