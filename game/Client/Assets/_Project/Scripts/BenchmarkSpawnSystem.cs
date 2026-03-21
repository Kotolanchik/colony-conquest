using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace ColonyConquest.Core
{
    /// <summary>
    /// Один раз создаёт сетку сущностей с <see cref="LocalTransform"/> и <see cref="BenchmarkDriftTag"/> для замера §6.1.
    /// </summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(BenchmarkDriftSystem))]
    public partial class BenchmarkSpawnSystem : SystemBase
    {
        protected override void OnCreate()
        {
            using var q = EntityManager.CreateEntityQuery(ComponentType.ReadOnly<BenchmarkPhase0State>());
            if (q.CalculateEntityCount() != 0)
                return;

            EntityManager.CreateSingleton(new BenchmarkPhase0State
            {
                Enabled = BenchmarkPhase0Tuning.Enabled,
                TargetEntityCount = 1000,
                SpawnCompleted = false,
                LogIntervalSeconds = 1f,
                DriftEnabled = true,
            });
        }

        protected override void OnUpdate()
        {
            var cfg = SystemAPI.GetSingleton<BenchmarkPhase0State>();
            if (!cfg.Enabled || cfg.SpawnCompleted)
                return;

            var em = EntityManager;
            var archetype = em.CreateArchetype(
                ComponentType.ReadWrite<LocalTransform>(),
                ComponentType.ReadOnly<BenchmarkDriftTag>());

            var count = math.max(1, cfg.TargetEntityCount);
            var grid = (int)math.ceil(math.sqrt(count));

            for (var i = 0; i < count; i++)
            {
                var e = em.CreateEntity(archetype);
                var x = i % grid;
                var z = i / grid;
                var px = x * 2f - grid;
                var pz = z * 2f - grid;
                em.SetComponentData(e, LocalTransform.FromPositionRotationScale(
                    new float3(px, 0f, pz),
                    quaternion.identity,
                    1f));
            }

            ref var rw = ref SystemAPI.GetSingletonRW<BenchmarkPhase0State>().ValueRW;
            rw.SpawnCompleted = true;

            Debug.Log($"[Colony & Conquest] Benchmark phase 0: spawned {count} drift entities (see BenchmarkReportSystem).");
        }
    }
}
