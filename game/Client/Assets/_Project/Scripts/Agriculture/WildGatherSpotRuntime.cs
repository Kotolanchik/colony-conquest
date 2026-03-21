using Unity.Entities;

namespace ColonyConquest.Agriculture
{
    /// <summary>Сценарий сбора: расход биомассы из <see cref="WildRenewableStockState"/>.</summary>
    public struct WildGatherSpotRuntime : IComponentData
    {
        public WildGatherKindId Kind;
    }
}
