using System;
using System.Collections.Generic;
using ColonyConquest.Presentation;
using UnityEngine;

namespace ColonyConquest.Presentation.Catalogs
{
    [Serializable]
    public struct VfxCatalogEntry
    {
        public PresentationVfxKind Kind;
        public GameObject Prefab;
        public float DefaultLifetimeSeconds;
        public ushort PrewarmPoolSize;
    }

    /// <summary>Каталог эффектов: kind → prefab/lifetime/pool.</summary>
    [CreateAssetMenu(fileName = "VfxCatalog", menuName = "Colony Conquest/Presentation/VFX Catalog")]
    public sealed class VfxCatalog : ScriptableObject
    {
        [SerializeField] List<VfxCatalogEntry> _entries = new();

        public IReadOnlyList<VfxCatalogEntry> Entries => _entries;
    }
}
