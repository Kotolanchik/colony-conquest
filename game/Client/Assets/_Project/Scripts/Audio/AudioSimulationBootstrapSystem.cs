using ColonyConquest.Core;
using Unity.Entities;

namespace ColonyConquest.Audio
{
    /// <summary>Создаёт singleton полной аудиосистемы и runtime-буферы.</summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(GameBootstrapSystem))]
    public partial struct AudioSimulationBootstrapSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<SimulationRootState>();
            state.RequireForUpdate<AudioBusServiceSingleton>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var em = state.EntityManager;
            using var query = em.CreateEntityQuery(ComponentType.ReadOnly<AudioSimulationSingleton>());
            if (query.CalculateEntityCount() != 0)
                return;

            var entity = em.CreateEntity();
            em.AddComponent<AudioSimulationSingleton>(entity);
            em.AddComponent(entity, new AudioSimulationState
            {
                LastProcessedDay = uint.MaxValue,
                ConcurrentVoiceBudget = 64,
                Concurrent3dBudget = 32,
                ThemeEra = 1,
                MusicLevel = AudioMusicIntensityLevel.Calm,
                AdaptiveMusicEnabled = 1,
                SpatialAudioEnabled = 1,
                SoundSignalsEnabled = 1,
                MusicCrossfadeSeconds = 2f,
                MasterVolume01 = 1f,
                MusicVolume01 = 0.8f,
                SfxVolume01 = 0.9f,
                VoiceVolume01 = 0.85f,
                EstimatedLatencyMs = 18f,
                EstimatedMemoryMb = 24f
            });

            em.AddBuffer<AudioActiveEmitterEntry>(entity);
            em.AddBuffer<AudioMusicTransitionEntry>(entity);
        }
    }
}
