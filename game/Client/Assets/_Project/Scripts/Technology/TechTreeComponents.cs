using Unity.Entities;

namespace ColonyConquest.Technology
{
    /// <summary>Маркер сущности дерева технологий с буферами статуса исследований.</summary>
    public struct TechTreeSimulationSingleton : IComponentData
    {
    }

    /// <summary>Runtime-состояние активного исследования и эпохальных переходов.</summary>
    public struct TechTreeSimulationState : IComponentData
    {
        public uint LastProcessedDay;
        public TechDefinitionId ActiveResearch;
        public float ActiveResearchProgressPoints;
        public float ResearchPoolPoints;
        public uint EraTransitionsTotal;
        public ushort UnlocksEra1;
        public ushort UnlocksEra2;
        public ushort UnlocksEra3;
        public ushort UnlocksEra4;
        public ushort UnlocksEra5;
    }

    /// <summary>Запись о разблокированной технологии.</summary>
    public struct TechUnlockedEntry : IBufferElementData
    {
        public TechDefinitionId Id;
    }
}
