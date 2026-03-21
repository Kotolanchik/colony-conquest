using Unity.Collections;
using Unity.Entities;

namespace ColonyConquest.Story
{
    /// <summary>Элемент очереди сюжетных событий для последующей обработки директором.</summary>
    public struct GameEventQueueEntry : IBufferElementData
    {
        public StoryEventKind Kind;
        public uint EventDefinitionId;
        public ulong EnqueueSimulationTick;
        public FixedString64Bytes DebugLabel;
    }
}
