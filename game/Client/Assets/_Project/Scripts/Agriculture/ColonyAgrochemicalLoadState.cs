using Unity.Entities;

namespace ColonyConquest.Agriculture
{
    /// <summary>Накопленная нагрузка агрохимии колонии (0 — чисто, 1 — максимум для прототипа).</summary>
    public struct ColonyAgrochemicalLoadState : IComponentData
    {
        public float ChemicalLoad01;
    }
}
