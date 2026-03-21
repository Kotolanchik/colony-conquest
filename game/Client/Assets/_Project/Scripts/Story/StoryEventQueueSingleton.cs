using Unity.Entities;

namespace ColonyConquest.Story
{
    /// <summary>Маркер сущности с буфером <see cref="GameEventQueueEntry"/>.</summary>
    public struct StoryEventQueueSingleton : IComponentData
    {
    }

    /// <summary>Состояние однократной инициализации тестовой очереди.</summary>
    public struct StoryEventPipelineState : IComponentData
    {
        public byte BootstrapEnqueued;
    }
}
