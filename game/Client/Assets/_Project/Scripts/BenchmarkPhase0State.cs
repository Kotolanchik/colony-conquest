using Unity.Entities;

namespace ColonyConquest.Core
{
    /// <summary>
    /// Синглтон конфигурации и состояния замера ECS (фаза 0).
    /// </summary>
    public struct BenchmarkPhase0State : IComponentData
    {
        public bool Enabled;
        public int TargetEntityCount;
        public bool SpawnCompleted;
        public float LogIntervalSeconds;
        public bool DriftEnabled;
    }
}
