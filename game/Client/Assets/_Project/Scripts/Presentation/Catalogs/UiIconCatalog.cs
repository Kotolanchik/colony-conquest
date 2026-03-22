using System;
using System.Collections.Generic;
using ColonyConquest.Presentation;
using UnityEngine;

namespace ColonyConquest.Presentation.Catalogs
{
    [Serializable]
    public struct UiIconCatalogEntry
    {
        public string IconId;
        public PresentationIconKind Kind;
        public Sprite Sprite;
        public Color DefaultTint;
    }

    /// <summary>Каталог иконок интерфейса (id → sprite/tint).</summary>
    [CreateAssetMenu(fileName = "UiIconCatalog", menuName = "Colony Conquest/Presentation/UI Icon Catalog")]
    public sealed class UiIconCatalog : ScriptableObject
    {
        [SerializeField] List<UiIconCatalogEntry> _entries = new();

        public IReadOnlyList<UiIconCatalogEntry> Entries => _entries;
    }
}
