using System;
using System.Collections.Generic;
using ColonyConquest.Core;
using UnityEngine;

namespace ColonyConquest.Presentation.Catalogs
{
    [Serializable]
    public struct BuildingVisualCatalogEntry
    {
        public ConstructionBlueprintId BlueprintId;
        public GameObject Prefab;
        public Sprite UiIcon;
        public Material DamageMaterial;
        public Vector3 PreviewOffset;
        public float PreviewDistance;
    }

    /// <summary>Каталог визуалов зданий/чертежей.</summary>
    [CreateAssetMenu(fileName = "BuildingVisualCatalog", menuName = "Colony Conquest/Presentation/Building Visual Catalog")]
    public sealed class BuildingVisualCatalog : ScriptableObject
    {
        [SerializeField] List<BuildingVisualCatalogEntry> _entries = new();

        public IReadOnlyList<BuildingVisualCatalogEntry> Entries => _entries;
    }
}
