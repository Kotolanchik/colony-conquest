using Unity.Entities;

namespace ColonyConquest.Core
{
    /// <summary>
    /// Корневое состояние пошаговой/непрерывной симуляции (тик для отладки и будущих систем).
    /// </summary>
    public struct SimulationRootState : IComponentData
    {
        public uint SimulationTick;
    }
}
