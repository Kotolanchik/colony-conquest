using Unity.Entities;

namespace ColonyConquest.Story
{
    /// <summary>
    /// Решение AI Director о «необходимости» события — <c>spec/events_quests_spec.md</c> §2.3 шаг 2.
    /// </summary>
    public enum AiDirectorPolicyKind : byte
    {
        None = 0,
        /// <summary>Снять напряжение (tension &gt; 80).</summary>
        Relief = 1,
        /// <summary>Испытание (wealth &gt; 80 и tension &lt; 30).</summary>
        Challenge = 2,
        /// <summary>Стабилизация (stability &lt; 30).</summary>
        Stabilize = 3,
        /// <summary>Военный акцент (security &lt; 30).</summary>
        Military = 4
    }

    /// <summary>Текущая политика директора и момент последней смены.</summary>
    public struct AiDirectorPolicyState : IComponentData
    {
        public AiDirectorPolicyKind ActivePolicy;
        public uint LastChangeTick;
    }
}
