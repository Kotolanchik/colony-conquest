using ColonyConquest.Core;
using Unity.Entities;

namespace ColonyConquest.Analytics
{
    /// <summary>Области статистики; <c>spec/statistics_analytics_spec.md</c> §1.2.</summary>
    public enum AnalyticsDomain : byte
    {
        LocalSettlement = 0,
        GlobalWorld = 1,
        HistoricalTimeline = 2
    }

    /// <summary>Запись в синглтон-буфер <see cref="AnalyticsRecordEntry"/> (до внешнего бэкенда).</summary>
    public static class AnalyticsHooks
    {
        const int MaxEntries = 1024;

        /// <summary>Записывает метрику во все мира, где есть <see cref="AnalyticsServiceSingleton"/> (Netcode: Client и Server).</summary>
        public static void Record(AnalyticsDomain domain, uint metricId, float value)
        {
            foreach (var world in World.All)
            {
                if (!world.IsCreated)
                    continue;
                TryRecordInWorld(world, domain, metricId, value);
            }
        }

        public static void RecordCounter(AnalyticsDomain domain, uint metricId, int delta = 1)
        {
            Record(domain, metricId, delta);
        }

        static void TryRecordInWorld(World world, AnalyticsDomain domain, uint metricId, float value)
        {
            var em = world.EntityManager;
            using (var q = em.CreateEntityQuery(ComponentType.ReadOnly<AnalyticsServiceSingleton>()))
            {
                if (q.CalculateEntityCount() == 0)
                    return;
                var e = q.GetSingletonEntity();
                var buffer = em.GetBuffer<AnalyticsRecordEntry>(e);
                uint tick = 0;
                using (var tq = em.CreateEntityQuery(ComponentType.ReadOnly<SimulationRootState>()))
                {
                    if (tq.CalculateEntityCount() != 0)
                        tick = tq.GetSingleton<SimulationRootState>().SimulationTick;
                }

                buffer.Add(new AnalyticsRecordEntry
                {
                    Domain = domain,
                    MetricId = metricId,
                    Value = value,
                    SimulationTick = tick
                });
                while (buffer.Length > MaxEntries)
                    buffer.RemoveAt(0);
            }
        }
    }
}
