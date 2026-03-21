using Unity.Entities;

namespace ColonyConquest.Agriculture
{
    /// <summary>
    /// Возобновляемые запасы §2.3: рыба и дичь (без симуляции вылова — только биомасса для будущих систем).
    /// </summary>
    public struct WildRenewableStockState : IComponentData
    {
        public float FishBiomass;
        public float FishBiomassCap;
        public float WildGameBiomass;
        public float WildGameCap;
    }
}
