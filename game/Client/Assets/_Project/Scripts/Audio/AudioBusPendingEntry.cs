using Unity.Entities;
using Unity.Mathematics;

namespace ColonyConquest.Audio
{
    /// <summary>Очередь логических аудио-событий до подключения FMOD/Wwise.</summary>
    public struct AudioBusPendingEntry : IBufferElementData
    {
        public uint EventId;
        public AudioSfxCategory Category;
        public float3 WorldPosition;
        public byte HasWorldPosition;
    }
}
