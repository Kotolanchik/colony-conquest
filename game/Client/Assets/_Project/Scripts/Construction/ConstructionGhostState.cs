using Unity.Entities;
using Unity.Mathematics;

namespace ColonyConquest.Core
{
    /// <summary>
    /// Синглтон состояния «призрака» размещения: активность, чертёж, сетка без полноценной карты тайлов.
    /// </summary>
    public struct ConstructionGhostState : IComponentData
    {
        public byte Active;
        public ConstructionBlueprintId BlueprintId;
        public int2 FootprintCells;
        public float3 AnchorWorld;
        public float RotationRadians;
        public byte PlacementValid;
    }
}
