using Unity.Entities;

namespace ColonyConquest.Economy
{
    /// <summary>Запись количества ресурса на складе (разрежённый буфер по типам).</summary>
    public struct ResourceStockEntry : IBufferElementData
    {
        public ResourceId Id;
        public float Amount;
    }
}
