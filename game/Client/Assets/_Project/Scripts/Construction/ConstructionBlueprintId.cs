namespace ColonyConquest.Core
{
    /// <summary>
    /// Идентификатор чертежа для режима призрака (без полной сетки и каталога построек).
    /// </summary>
    public enum ConstructionBlueprintId : byte
    {
        None = 0,
        /// <summary>Землянка 2×2, эпоха 1 (см. construction_system_spec §2.2).</summary>
        EarthHut = 1,
        /// <summary>Хижина 3×3, эпоха 1.</summary>
        Cabin = 2,
        /// <summary>Дом 4×4, эпоха 1.</summary>
        House = 3,
        /// <summary>Усадьба 6×6, эпоха 1.</summary>
        Manor = 4,
        /// <summary>Рабочий дом 4×6, эпоха 2.</summary>
        WorkerTenement = 5,
        /// <summary>Таунхаус 5×8, эпоха 2.</summary>
        TownhouseEpoch2 = 6,
    }
}
