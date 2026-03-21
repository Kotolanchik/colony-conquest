using Unity.Entities;
using Unity.Mathematics;

namespace ColonyConquest.Audio
{
    /// <summary>
    /// Очередь логических аудио-событий; воспроизведение — в <see cref="AudioBusDrainSystem"/>.
    /// </summary>
    public static class AudioBusStub
    {
        public static void Post(uint eventId, AudioSfxCategory category, float3? worldPosition = null)
        {
            var world = World.DefaultGameObjectInjectionWorld;
            if (world == null || !world.IsCreated)
                return;
            var em = world.EntityManager;
            using (var q = em.CreateEntityQuery(ComponentType.ReadOnly<AudioBusServiceSingleton>()))
            {
                if (q.CalculateEntityCount() == 0)
                    return;
                var e = q.GetSingletonEntity();
                var buffer = em.GetBuffer<AudioBusPendingEntry>(e);
                var entry = new AudioBusPendingEntry
                {
                    EventId = eventId,
                    Category = category,
                    HasWorldPosition = 0,
                    WorldPosition = float3.zero
                };
                if (worldPosition.HasValue)
                {
                    entry.WorldPosition = worldPosition.Value;
                    entry.HasWorldPosition = 1;
                }

                buffer.Add(entry);
            }
        }
    }
}
