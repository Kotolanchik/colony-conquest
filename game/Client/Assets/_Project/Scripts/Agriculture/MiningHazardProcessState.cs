using Unity.Entities;

namespace ColonyConquest.Agriculture
{
    /// <summary>Один проход рисков §2.4 за игровой день.</summary>
    public struct MiningHazardProcessState : IComponentData
    {
        public uint LastProcessedGameDay;
    }
}
