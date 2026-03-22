using System;
using System.Collections.Generic;
using ColonyConquest.Core;
using ColonyConquest.Military;
using ColonyConquest.Presentation.Catalogs;
using Unity.Mathematics;
using UnityEngine;

namespace ColonyConquest.Presentation
{
    /// <summary>
    /// Runtime bridge-resolver: превращает ECS-запросы в реальные Unity-представления (prefab/icon/vfx).
    /// Добавляется на объект сцены (например, Bootstrap) и конфигурируется через каталоги.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PresentationRuntimeResolverService : MonoBehaviour
    {
        [Serializable]
        public struct ResolvedIconFeedEntry
        {
            public string IconId;
            public PresentationIconKind Kind;
            public Sprite Sprite;
            public Color Tint;
            public float ExpiresAtTime;
            public byte Priority;
        }

        struct ActiveVfxInstance
        {
            public PresentationVfxKind Kind;
            public GameObject Instance;
            public float ReturnAtTime;
        }

        [Header("Catalogs")]
        [SerializeField] UnitVisualCatalog _unitCatalog;
        [SerializeField] BuildingVisualCatalog _buildingCatalog;
        [SerializeField] UiIconCatalog _iconCatalog;
        [SerializeField] VfxCatalog _vfxCatalog;

        [Header("Scene Roots (optional)")]
        [SerializeField] Transform _unitsRoot;
        [SerializeField] Transform _buildingsRoot;
        [SerializeField] Transform _vfxRoot;

        [Header("Icon Feed")]
        [SerializeField] int _maxRecentIcons = 24;

        readonly Dictionary<MilitaryUnitType, UnitVisualCatalogEntry> _unitByType = new();
        readonly Dictionary<ConstructionBlueprintId, BuildingVisualCatalogEntry> _buildingByBlueprint = new();
        readonly Dictionary<string, UiIconCatalogEntry> _iconById = new(StringComparer.OrdinalIgnoreCase);
        readonly Dictionary<PresentationVfxKind, VfxCatalogEntry> _vfxByKind = new();

        readonly Dictionary<uint, GameObject> _unitInstances = new();
        readonly Dictionary<uint, MilitaryUnitType> _unitInstanceType = new();

        readonly Dictionary<uint, GameObject> _buildingInstances = new();
        readonly Dictionary<uint, ConstructionBlueprintId> _buildingInstanceBlueprint = new();

        readonly Dictionary<PresentationVfxKind, Queue<GameObject>> _vfxPools = new();
        readonly List<ActiveVfxInstance> _activeVfx = new();
        readonly List<ResolvedIconFeedEntry> _recentIcons = new();

        public static PresentationRuntimeResolverService Instance { get; private set; }
        public static bool HasActiveInstance => Instance != null && Instance.isActiveAndEnabled;

        public int ActiveVfxCount => _activeVfx.Count;
        public IReadOnlyList<ResolvedIconFeedEntry> RecentIcons => _recentIcons;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            EnsureRoots();
            RebuildCachesAndPrewarm();
        }

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        void Update()
        {
            // Снимаем просроченные icon feed entries.
            var now = Time.time;
            for (var i = _recentIcons.Count - 1; i >= 0; i--)
            {
                if (_recentIcons[i].ExpiresAtTime <= now)
                    _recentIcons.RemoveAt(i);
            }

            // Возвращаем VFX-инстансы в пул по TTL.
            for (var i = _activeVfx.Count - 1; i >= 0; i--)
            {
                var active = _activeVfx[i];
                if (active.ReturnAtTime > now)
                    continue;

                if (active.Instance != null)
                {
                    active.Instance.SetActive(false);
                    if (!_vfxPools.TryGetValue(active.Kind, out var queue))
                    {
                        queue = new Queue<GameObject>();
                        _vfxPools.Add(active.Kind, queue);
                    }

                    queue.Enqueue(active.Instance);
                }

                _activeVfx.RemoveAt(i);
            }
        }

        public bool UpsertUnitVisual(uint runtimeUnitId, MilitaryUnitType unitType, float3 worldPosition, quaternion rotation)
        {
            if (!_unitByType.TryGetValue(unitType, out var entry) || entry.Prefab == null)
                return false;

            if (!_unitInstances.TryGetValue(runtimeUnitId, out var instance) || instance == null)
            {
                instance = Instantiate(entry.Prefab, _unitsRoot);
                _unitInstances[runtimeUnitId] = instance;
                _unitInstanceType[runtimeUnitId] = unitType;
            }
            else if (_unitInstanceType.TryGetValue(runtimeUnitId, out var existingType) && existingType != unitType)
            {
                Destroy(instance);
                instance = Instantiate(entry.Prefab, _unitsRoot);
                _unitInstances[runtimeUnitId] = instance;
                _unitInstanceType[runtimeUnitId] = unitType;
            }

            if (instance == null)
                return false;

            instance.transform.SetPositionAndRotation(ToVector3(worldPosition), ToQuaternion(rotation));
            return true;
        }

        public bool UpsertBuildingVisual(uint runtimeBuildingId, ConstructionBlueprintId blueprintId, float3 worldPosition,
            quaternion rotation)
        {
            if (!_buildingByBlueprint.TryGetValue(blueprintId, out var entry) || entry.Prefab == null)
                return false;

            if (!_buildingInstances.TryGetValue(runtimeBuildingId, out var instance) || instance == null)
            {
                instance = Instantiate(entry.Prefab, _buildingsRoot);
                _buildingInstances[runtimeBuildingId] = instance;
                _buildingInstanceBlueprint[runtimeBuildingId] = blueprintId;
            }
            else if (_buildingInstanceBlueprint.TryGetValue(runtimeBuildingId, out var existingBlueprint) &&
                existingBlueprint != blueprintId)
            {
                Destroy(instance);
                instance = Instantiate(entry.Prefab, _buildingsRoot);
                _buildingInstances[runtimeBuildingId] = instance;
                _buildingInstanceBlueprint[runtimeBuildingId] = blueprintId;
            }

            if (instance == null)
                return false;

            instance.transform.SetPositionAndRotation(ToVector3(worldPosition), ToQuaternion(rotation));
            return true;
        }

        public bool PushIcon(in UiIconRequestEntry request)
        {
            var iconId = request.IconId.ToString();
            if (string.IsNullOrWhiteSpace(iconId))
                return false;
            if (!_iconById.TryGetValue(iconId, out var entry) || entry.Sprite == null)
                return false;

            var ttl = request.LifetimeSeconds > 0f ? request.LifetimeSeconds : 1.5f;
            _recentIcons.Add(new ResolvedIconFeedEntry
            {
                IconId = iconId,
                Kind = request.Kind,
                Sprite = entry.Sprite,
                Tint = entry.DefaultTint,
                ExpiresAtTime = Time.time + ttl,
                Priority = request.Priority
            });

            while (_recentIcons.Count > Mathf.Max(1, _maxRecentIcons))
                _recentIcons.RemoveAt(0);

            return true;
        }

        public bool PlayVfx(PresentationVfxKind kind, float3 worldPosition, float intensity01, float lifetimeSeconds)
        {
            if (!_vfxByKind.TryGetValue(kind, out var entry) || entry.Prefab == null)
                return false;

            if (!_vfxPools.TryGetValue(kind, out var pool))
            {
                pool = new Queue<GameObject>();
                _vfxPools.Add(kind, pool);
            }

            GameObject instance;
            if (pool.Count > 0)
                instance = pool.Dequeue();
            else
                instance = Instantiate(entry.Prefab, _vfxRoot);

            if (instance == null)
                return false;

            var intensity = math.saturate(intensity01);
            var ttl = lifetimeSeconds > 0.01f
                ? lifetimeSeconds
                : entry.DefaultLifetimeSeconds > 0.01f ? entry.DefaultLifetimeSeconds : 1.5f;

            instance.transform.SetPositionAndRotation(ToVector3(worldPosition), Quaternion.identity);
            instance.transform.localScale = Vector3.one * math.lerp(0.75f, 1.4f, intensity);
            instance.SetActive(true);

            _activeVfx.Add(new ActiveVfxInstance
            {
                Kind = kind,
                Instance = instance,
                ReturnAtTime = Time.time + ttl
            });
            return true;
        }

        void RebuildCachesAndPrewarm()
        {
            _unitByType.Clear();
            _buildingByBlueprint.Clear();
            _iconById.Clear();
            _vfxByKind.Clear();

            if (_unitCatalog != null)
            {
                foreach (var entry in _unitCatalog.Entries)
                {
                    if (entry.Prefab == null)
                        continue;
                    _unitByType[entry.UnitType] = entry;
                }
            }

            if (_buildingCatalog != null)
            {
                foreach (var entry in _buildingCatalog.Entries)
                {
                    if (entry.Prefab == null || entry.BlueprintId == ConstructionBlueprintId.None)
                        continue;
                    _buildingByBlueprint[entry.BlueprintId] = entry;
                }
            }

            if (_iconCatalog != null)
            {
                foreach (var entry in _iconCatalog.Entries)
                {
                    if (string.IsNullOrWhiteSpace(entry.IconId) || entry.Sprite == null)
                        continue;
                    _iconById[entry.IconId] = entry;
                }
            }

            _vfxPools.Clear();
            if (_vfxCatalog != null)
            {
                foreach (var entry in _vfxCatalog.Entries)
                {
                    if (entry.Kind == PresentationVfxKind.None || entry.Prefab == null)
                        continue;

                    _vfxByKind[entry.Kind] = entry;
                    var pool = new Queue<GameObject>();
                    _vfxPools[entry.Kind] = pool;

                    for (var i = 0; i < entry.PrewarmPoolSize; i++)
                    {
                        var instance = Instantiate(entry.Prefab, _vfxRoot);
                        instance.SetActive(false);
                        pool.Enqueue(instance);
                    }
                }
            }
        }

        void EnsureRoots()
        {
            if (_unitsRoot == null)
                _unitsRoot = CreateRoot("Units");
            if (_buildingsRoot == null)
                _buildingsRoot = CreateRoot("Buildings");
            if (_vfxRoot == null)
                _vfxRoot = CreateRoot("VFX");
        }

        Transform CreateRoot(string suffix)
        {
            var go = new GameObject($"PresentationRuntime_{suffix}");
            go.transform.SetParent(transform, false);
            return go.transform;
        }

        static Vector3 ToVector3(float3 value) => new(value.x, value.y, value.z);
        static Quaternion ToQuaternion(quaternion value) => new(value.value.x, value.value.y, value.value.z, value.value.w);
    }
}
