using System;
using System.Collections.Generic;
using ColonyConquest.Military;
using UnityEngine;

namespace ColonyConquest.Presentation.Catalogs
{
    [Serializable]
    public struct UnitVisualCatalogEntry
    {
        public MilitaryUnitType UnitType;
        public GameObject Prefab;
        public Sprite UiIcon;
        public RuntimeAnimatorController AnimatorController;
        public Material TeamTintMaterial;
        public Vector3 PreviewOffset;
        public float PreviewDistance;
    }

    /// <summary>Каталог визуалов боевых юнитов: prefab, icon, animator, preview setup.</summary>
    [CreateAssetMenu(fileName = "UnitVisualCatalog", menuName = "Colony Conquest/Presentation/Unit Visual Catalog")]
    public sealed class UnitVisualCatalog : ScriptableObject
    {
        [SerializeField] List<UnitVisualCatalogEntry> _entries = new();

        public IReadOnlyList<UnitVisualCatalogEntry> Entries => _entries;
    }
}
