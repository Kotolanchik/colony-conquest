using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ColonyConquest.Core
{
    /// <summary>
    /// Синтетическая нагрузка на transform (Burst-совместимый цикл без managed API).
    /// </summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(BenchmarkSpawnSystem))]
    public partial struct BenchmarkDriftSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var cfg = SystemAPI.GetSingleton<BenchmarkPhase0State>();
            if (!cfg.Enabled || !cfg.DriftEnabled || !cfg.SpawnCompleted)
                return;

            var t = (float)SystemAPI.Time.ElapsedTime;
            foreach (var lt in SystemAPI.Query<RefRW<LocalTransform>>().WithAll<BenchmarkDriftTag>())
            {
                var pos = lt.ValueRO.Position;
                var wobble = math.sin(t * 2f + pos.x * 0.1f) * 0.02f;
                var rot = lt.ValueRO.Rotation;
                var scale = lt.ValueRO.Scale;
                lt.ValueRW = LocalTransform.FromPositionRotationScale(
                    pos + new float3(0f, wobble, 0f),
                    rot,
                    scale);
            }
        }
    }
}
