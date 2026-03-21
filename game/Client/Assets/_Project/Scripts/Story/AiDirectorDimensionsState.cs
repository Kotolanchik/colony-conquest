using Unity.Entities;

namespace ColonyConquest.Story
{
    /// <summary>
    /// Пять измерений для AI Director — <c>spec/events_quests_spec.md</c> §2.2 (0 = плохо, 100 = хорошо для Wealth/Security/…; Tension: 0 скучно, 100 перегруз).
    /// </summary>
    public struct AiDirectorDimensionsState : IComponentData
    {
        public float Wealth0to100;
        public float Security0to100;
        public float Stability0to100;
        public float Progress0to100;
        public float Tension0to100;
    }
}
