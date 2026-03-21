using ColonyConquest.Economy;
using Unity.Entities;

namespace ColonyConquest.Agriculture
{
    /// <summary>
    /// Промышленная добыча: номинал из <see cref="IndustrialMiningFormulas"/>, масштаб по числу рабочих.
    /// </summary>
    public struct IndustrialMiningSiteRuntime : IComponentData
    {
        public IndustrialMiningMethodId Method;
        public byte WorkersAssigned;
        public float OutputAccumulator;
        public ResourceId OutputResourceId;
    }
}
