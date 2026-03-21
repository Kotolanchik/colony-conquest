using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace ColonyConquest.Core
{
    /// <summary>
    /// Периодически пишет в консоль число сущностей замера и средний dt за интервал (оценка FPS).
    /// </summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(BenchmarkDriftSystem))]
    public partial struct BenchmarkReportSystem : ISystem
    {
        private EntityQuery _driftEntityQuery;
        private float _nextLogTime;
        private float _dtAccum;
        private int _frameCount;

        public void OnCreate(ref SystemState state)
        {
            _nextLogTime = 0f;
            _driftEntityQuery = state.GetEntityQuery(ComponentType.ReadOnly<BenchmarkDriftTag>());
        }

        public void OnUpdate(ref SystemState state)
        {
            var cfg = SystemAPI.GetSingleton<BenchmarkPhase0State>();
            if (!cfg.Enabled || !cfg.SpawnCompleted)
                return;

            var now = (float)SystemAPI.Time.ElapsedTime;
            _dtAccum += SystemAPI.Time.DeltaTime;
            _frameCount++;

            if (now < _nextLogTime)
                return;

            var interval = math.max(0.1f, cfg.LogIntervalSeconds);
            _nextLogTime = now + interval;

            var n = _driftEntityQuery.CalculateEntityCount();

            var avgDt = _frameCount > 0 ? _dtAccum / _frameCount : 0f;
            var fps = avgDt > 1e-6f ? 1f / avgDt : 0f;
            _dtAccum = 0f;
            _frameCount = 0;

            Debug.Log($"[Colony & Conquest] Benchmark phase 0: entities={n} avgDt={avgDt * 1000f:F2} ms (~{fps:F0} FPS) — цель дорожной карты §6.1: 1000 юнитов при 60 FPS (ориентир).");
        }
    }
}
