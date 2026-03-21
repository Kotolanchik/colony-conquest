using Unity.Mathematics;

namespace ColonyConquest.Core
{
    /// <summary>
    /// Размеры сетки в клетках по <c>spec/construction_system_spec.md</c> §2.2 (жилые эпохи 1–2).
    /// </summary>
    public static class ConstructionBlueprintFootprints
    {
        public static int2 GetFootprintCells(ConstructionBlueprintId id)
        {
            return id switch
            {
                ConstructionBlueprintId.EarthHut => new int2(2, 2),
                ConstructionBlueprintId.Cabin => new int2(3, 3),
                ConstructionBlueprintId.House => new int2(4, 4),
                ConstructionBlueprintId.Manor => new int2(6, 6),
                ConstructionBlueprintId.WorkerTenement => new int2(4, 6),
                ConstructionBlueprintId.TownhouseEpoch2 => new int2(5, 8),
                _ => new int2(2, 2),
            };
        }
    }
}
