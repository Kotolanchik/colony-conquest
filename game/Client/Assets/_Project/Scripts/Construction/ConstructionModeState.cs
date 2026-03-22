using Unity.Entities;
using Unity.Mathematics;

namespace ColonyConquest.Core
{
    /// <summary>Состояние режима строительства (переключатель, выбранный чертёж, позиция призрака).</summary>
    public struct ConstructionModeState : IComponentData
    {
        public byte IsActive;
        public byte BlueprintId;
        public float3 GhostWorldPosition;
    }
}
