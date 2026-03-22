using Unity.Entities;
using Unity.Mathematics;

namespace ColonyConquest.Audio
{
    public enum AudioMusicIntensityLevel : byte
    {
        Calm = 0,
        Tense = 1,
        Battle = 2,
        Epic = 3
    }

    /// <summary>Маркер singleton полной runtime аудиосистемы.</summary>
    public struct AudioSimulationSingleton : IComponentData
    {
    }

    /// <summary>Сводное состояние адаптивной музыки, SFX бюджета и 3D-каналов.</summary>
    public struct AudioSimulationState : IComponentData
    {
        public uint LastProcessedDay;
        public uint ConsumedBusEventsTotal;
        public uint DroppedBusEventsTotal;
        public uint MusicTransitionsTotal;

        public uint ActiveVoices;
        public uint Active3dSources;
        public ushort ConcurrentVoiceBudget;
        public ushort Concurrent3dBudget;

        public byte ThemeEra;
        public AudioMusicIntensityLevel MusicLevel;
        public byte AdaptiveMusicEnabled;
        public byte SpatialAudioEnabled;
        public byte SoundSignalsEnabled;

        public float CombatIntensity01;
        public float BaseCrisis01;
        public float MusicIntensity01;
        public float MusicCrossfadeSeconds;
        public float LastWeatherSeverity01;

        public float MasterVolume01;
        public float MusicVolume01;
        public float SfxVolume01;
        public float VoiceVolume01;

        public float EstimatedLatencyMs;
        public float EstimatedMemoryMb;
    }

    /// <summary>Активный эмиттер (логический голос) после ingest из шины AudioBus.</summary>
    public struct AudioActiveEmitterEntry : IBufferElementData
    {
        public uint EventId;
        public AudioSfxCategory Category;
        public float3 Position;
        public byte HasWorldPosition;
        public float Priority01;
        public float Volume01;
        public float RemainingSeconds;
        public float Occlusion01;
        public float ReverbSeconds;
    }

    /// <summary>Журнал переходов музыкальных состояний (для телеметрии и дебага).</summary>
    public struct AudioMusicTransitionEntry : IBufferElementData
    {
        public uint DayIndex;
        public uint SimulationTick;
        public byte ThemeEra;
        public AudioMusicIntensityLevel FromLevel;
        public AudioMusicIntensityLevel ToLevel;
        public float CrossfadeSeconds;
    }
}
