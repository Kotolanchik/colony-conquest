# ТЕХНИЧЕСКАЯ АРХИТЕКТУРА — СПЕЦИФИКАЦИЯ
## Colony & Conquest — Гибрид Colony Sim + RTS + Tactical Warfare

**Версия:** 1.1  
**Дата:** 2026-03-21  
**Целевые показатели:** 10,000+ юнитов, 60 FPS, плавный переход между 4 уровнями камеры

---

## 1. ВЫБОР ТЕХНОЛОГИЙ

### 1.1 Сравнительная таблица движков

| Критерий | Unity + DOTS | Godot 4 | Unreal Engine 5 | Custom Engine |
|----------|--------------|---------|-----------------|---------------|
| **ECS** | ⭐⭐⭐ Отличный (DOTS) | ⭐⭐ Кастомный | ⭐⭐ Mass Entities | ⭐⭐⭐ Полный контроль |
| **Производительность** | ⭐⭐⭐ Burst, Job System | ⭐⭐ GDScript медленнее | ⭐⭐ Тяжеловат | ⭐⭐⭐ Максимальная |
| **Кривая обучения** | ⭐⭐ Средняя | ⭐⭐⭐ Низкая | ⭐⭐ Средняя | ⭐ Высокая |
| **Стоимость** | Бесплатно до $200K | Полностью бесплатно | 5% royalty | Бесплатно |
| **Сообщество** | ⭐⭐⭐ Огромное | ⭐⭐ Растущее | ⭐⭐⭐ Огромное | ⭐ Нет |
| **10K юнитов** | ⭐⭐⭐ Возможно | ⭐⭐ Сложно | ⭐⭐ Сложно | ⭐⭐⭐ Возможно |
| **Сетевой код** | ⭐⭐ Netcode for Entities | ⭐⭐ Кастомный | ⭐⭐⭐ Built-in | ⭐⭐⭐ Кастомный |
| **Время разработки** | ⭐⭐ Среднее | ⭐⭐⭐ Быстрое | ⭐⭐ Среднее | ⭐ Медленное |

### 1.2 Финальное решение: **Unity + DOTS**

**Обоснование:**

```
┌─────────────────────────────────────────────────────────────────┐
│                    ПОЧЕМУ UNITY + DOTS                          │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  1. ECS-first архитектура — DOTS создан специально для         │
│     масштабных симуляций с тысячами объектов                   │
│                                                                 │
│  2. Burst Compiler — автоматическая векторизация и             │
│     оптимизация кода на C# до уровня C++                       │
│                                                                 │
│  3. Job System — безопасное многопоточное выполнение           │
│     с автоматическим управлением зависимостями                 │
│                                                                 │
│  4. Netcode for Entities — готовое решение для                 │
│     многопользовательских игр с ECS                            │
│                                                                 │
│  5. Entity Graphics — интеграция с Render Graph и              │
│     поддержка GPU instancing из коробки                        │
│                                                                 │
│  6. Ресурсы команды — C# широко распространён,                 │
│     легко найти разработчиков                                  │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

### 1.3 Стек технологий

**Целевая версия редактора (репозиторий):** Unity **6** (`6000.2.x` LTS). Фиксируется в `game/Client/ProjectSettings/ProjectVersion.txt`. Обоснование и альтернативы — ADR `docs/decisions/002-unity6-editor-baseline.md`.

Исторически стек проектировался под **Unity 2023.2+ LTS**; архитектура **DOTS / ECS / URP / Unity Physics / Input System** сохраняется. При смене мажорной версии редактора версии пакетов в `game/Client/Packages/manifest.json` нужно согласовывать с документацией Unity и Netcode for Entities.

| Слой | Технология | Назначение |
|------|------------|------------|
| **Движок** | Unity 6 (6000.2.x LTS) | Основной движок |
| **ECS** | Entities 1.x (DOTS) | Entity Component System |
| **Физика** | Unity Physics (DOTS) | Физика для 10K+ объектов |
| **Рендеринг** | URP (Universal RP) | Кросс-платформенный рендер |
| **Сеть** | Netcode for Entities | Мультиплеер |
| **Аудио** | FMOD / Wwise | Профессиональный аудио |
| **Ввод** | Input System | Новая система ввода |
| **UI** | UI Toolkit + UIToolkit.DOTS | Интерфейс |
| **Сериализация** | MemoryPack / MessagePack | Быстрая сериализация |
| **Профайлинг** | Unity Profiler + custom | Оптимизация |

---

## 2. ECS АРХИТЕКТУРА

### 2.1 Общая структура ECS

```
┌─────────────────────────────────────────────────────────────────────────┐
│                         ECS АРХИТЕКТУРА                                 │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  ┌─────────────┐    ┌─────────────┐    ┌─────────────┐                 │
│  │  ENTITIES   │    │ COMPONENTS  │    │   SYSTEMS   │                 │
│  │  (Сущности) │    │(Компоненты) │    │  (Системы)  │                 │
│  └──────┬──────┘    └──────┬──────┘    └──────┬──────┘                 │
│         │                  │                  │                         │
│         │   ┌──────────────┴──────────────────┘                         │
│         │   │                                                           │
│         ▼   ▼                                                           │
│  ┌─────────────────────────────────────────────────────────┐            │
│  │              WORLD (Мир симуляции)                       │            │
│  │  ┌─────────────────────────────────────────────────┐    │            │
│  │  │  EntityManager — создаёт, уничтожает, изменяет  │    │            │
│  │  │  ComponentData — хранит данные в структурах     │    │            │
│  │  │  SystemGroup — управляет порядком выполнения    │    │            │
│  │  └─────────────────────────────────────────────────┘    │            │
│  └─────────────────────────────────────────────────────────┘            │
│                                                                         │
│  ┌─────────────────────────────────────────────────────────┐            │
│  │              SYSTEM EXECUTION ORDER                      │            │
│  │                                                          │            │
│  │  InitializationSystemGroup → SimulationSystemGroup       │            │
│  │         ↓                        ↓                       │            │
│  │  PresentationSystemGroup → RenderSystemGroup             │            │
│  └─────────────────────────────────────────────────────────┘            │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

### 2.2 Компоненты (Components) — Core Components

```csharp
// ============================================================
// CORE COMPONENTS — Базовые компоненты для всех сущностей
// ============================================================

// Позиция и ориентация в мире
public struct LocalTransform : IComponentData
{
    public float3 Position;
    public quaternion Rotation;
    public float Scale;
}

// Скорость перемещения
public struct MovementData : IComponentData
{
    public float3 Velocity;
    public float3 TargetPosition;
    public float Speed;
    public float RotationSpeed;
    public MovementState State; // Idle, Moving, Pathfinding
}

// Здоровье и броня
public struct HealthData : IComponentData
{
    public float CurrentHealth;
    public float MaxHealth;
    public float Armor;
    public float Shield;
    public DamageType LastDamageType;
}

// Принадлежность к фракции
public struct FactionData : IComponentData
{
    public int FactionId;      // 0 = игрок, 1-7 = ИИ/враги
    public int TeamId;         // Для союзников
    public FactionType Type;   // Player, AI, Neutral, Hostile
}

// ============================================================
// UNIT COMPONENTS — Компоненты юнитов
// ============================================================

public struct UnitTypeData : IComponentData
{
    public UnitType Type;           // Infantry, Vehicle, Aircraft, Drone
    public UnitClass Class;         // Light, Medium, Heavy, Elite
    public int Tier;                // 1-4 уровень развития
    public float2 Size;             // Габариты для коллизий
    public float Mass;
}

public struct CombatData : IComponentData
{
    public float AttackDamage;
    public float AttackRange;
    public float AttackCooldown;
    public float CurrentCooldown;
    public DamageType DamageType;
    public AttackMode Mode;
    public Entity CurrentTarget;
    public float Accuracy;
    public float CriticalChance;
}

// Вооружение (мульти-оружие через DynamicBuffer)
public struct WeaponData : IBufferElementData
{
    public WeaponType Type;
    public float Damage;
    public float Range;
    public float Cooldown;
    public float CurrentCooldown;
    public int Ammo;
    public int MaxAmmo;
    public bool IsActive;
    public float3 LocalOffset;
}

// ============================================================
// COLONY COMPONENTS — Компоненты поселенцев/колонии
// ============================================================

public struct NeedsData : IComponentData
{
    public float Hunger;
    public float Thirst;
    public float Energy;
    public float Mood;
    public float Comfort;
    public float Recreation;
    public float Social;
}

public struct SkillsData : IBufferElementData
{
    public SkillType Type;
    public int Level;           // 0-20
    public float Experience;
    public float Passion;
}

public struct JobData : IComponentData
{
    public JobType CurrentJob;
    public JobPriority Priority;
    public Entity WorkTarget;
    public float3 WorkPosition;
    public float Progress;
    public float WorkSpeed;
}

// ============================================================
// BUILDING COMPONENTS — Компоненты зданий
// ============================================================

public struct BuildingData : IComponentData
{
    public BuildingType Type;
    public int Level;
    public float ConstructionProgress;
    public bool IsConstructed;
    public bool IsOperational;
    public float PowerConsumption;
    public float PowerGeneration;
}

public struct PowerGridData : IComponentData
{
    public int GridId;
    public float PowerInput;
    public float PowerOutput;
    public float PowerStorage;
    public float MaxStorage;
    public bool IsOnline;
}

// ============================================================
// LOD & VISIBILITY COMPONENTS
// ============================================================

public struct LODData : IComponentData
{
    public LODLevel CurrentLevel;   // Full, Medium, Low, Impostor, Hidden
    public float DistanceToCamera;
    public float TransitionProgress;
    public bool IsVisible;
}

public struct MetaUnitData : IComponentData
{
    public int MetaUnitId;
    public int UnitCount;
    public float3 AveragePosition;
    public float SpreadRadius;
    public bool IsGrouped;
}

// ============================================================
// NETWORK COMPONENTS
// ============================================================

public struct NetworkedEntity : IComponentData
{
    public int NetworkId;
    public uint OwnerClientId;
    public NetworkPriority Priority;
    public float LastUpdateTime;
}

public struct PredictionData : IComponentData
{
    public float3 PredictedPosition;
    public float3 PredictedVelocity;
    public float PredictionError;
    public uint LastProcessedInput;
}
```

### 2.3 Системы (Systems) — Movement & Combat

```csharp
// ============================================================
// SYSTEM GROUPS
// ============================================================

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateBefore(typeof(TransformSystemGroup))]
public partial class UnitSimulationSystemGroup : ComponentSystemGroup { }

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(UnitSimulationSystemGroup))]
public partial class CombatSystemGroup : ComponentSystemGroup { }

// ============================================================
// MOVEMENT SYSTEM
// ============================================================

[BurstCompile]
public partial struct MovementSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        float deltaTime = SystemAPI.Time.DeltaTime;
        
        new MovementJob
        {
            DeltaTime = deltaTime,
        }.ScheduleParallel();
    }
}

[BurstCompile]
public partial struct MovementJob : IJobEntity
{
    public float DeltaTime;
    
    void Execute(ref MovementData movement, ref LocalTransform transform, 
                 in UnitTypeData unitType)
    {
        if (movement.State != MovementState.Moving)
            return;
            
        float3 direction = movement.TargetPosition - transform.Position;
        float distance = math.length(direction);
        
        if (distance < 0.1f)
        {
            movement.State = MovementState.Idle;
            movement.Velocity = float3.zero;
            return;
        }
        
        direction = math.normalize(direction);
        
        quaternion targetRotation = quaternion.LookRotation(direction, math.up());
        transform.Rotation = math.slerp(transform.Rotation, targetRotation, 
            movement.RotationSpeed * DeltaTime);
        
        float speed = movement.Speed / (1 + unitType.Mass * 0.01f);
        movement.Velocity = direction * speed;
        transform.Position += movement.Velocity * DeltaTime;
    }
}

// ============================================================
// COMBAT SYSTEM
// ============================================================

[BurstCompile]
public partial struct CombatSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        float deltaTime = SystemAPI.Time.DeltaTime;
        
        new UpdateCooldownsJob { DeltaTime = deltaTime }.ScheduleParallel();
        new TargetAcquisitionJob { }.ScheduleParallel();
        new AttackJob { DeltaTime = deltaTime }.ScheduleParallel();
    }
}

[BurstCompile]
public partial struct AttackJob : IJobEntity
{
    public float DeltaTime;
    
    void Execute(ref CombatData combat, in LocalTransform transform, 
                 in DynamicBuffer<WeaponData> weapons, in FactionData faction)
    {
        if (combat.CurrentTarget == Entity.Null)
            return;
            
        if (combat.CurrentCooldown <= 0)
        {
            // Fire primary weapon
            if (weapons.Length > 0)
            {
                combat.CurrentCooldown = combat.AttackCooldown;
            }
        }
    }
}

// ============================================================
// LOD SYSTEM
// ============================================================

[UpdateInGroup(typeof(PresentationSystemGroup))]
public partial struct LODSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        float3 cameraPos = Camera.main.transform.position;
        
        new LODCalculationJob
        {
            CameraPosition = cameraPos,
            DistanceMedium = 150f,
            DistanceLow = 400f,
            DistanceImpostor = 1000f,
        }.ScheduleParallel();
    }
}

[BurstCompile]
public partial struct LODCalculationJob : IJobEntity
{
    public float3 CameraPosition;
    public float DistanceMedium;
    public float DistanceLow;
    public float DistanceImpostor;
    
    void Execute(ref LODData lod, in LocalTransform transform)
    {
        float distance = math.distance(transform.Position, CameraPosition);
        lod.DistanceToCamera = distance;
        
        LODLevel newLevel = distance switch
        {
            < 50f => LODLevel.Full,
            < DistanceMedium => LODLevel.Medium,
            < DistanceLow => LODLevel.Low,
            < DistanceImpostor => LODLevel.Impostor,
            _ => LODLevel.Hidden
        };
        
        if (newLevel != lod.CurrentLevel)
        {
            lod.CurrentLevel = newLevel;
            lod.TransitionProgress = 0f;
        }
        
        lod.IsVisible = newLevel != LODLevel.Hidden;
    }
}
```

### 2.4 Entity Archetypes

```
┌─────────────────────────────────────────────────────────────────────────┐
│                      ENTITY ARCHETYPES                                  │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  SETTLER (Поселенец):
│  LocalTransform + MovementData + HealthData + FactionData
│  + NeedsData + SkillsData (Buffer) + JobData + InventoryData
│  + UnitTypeData + LODData
│
│  COMBAT UNIT (Боевой юнит):
│  LocalTransform + MovementData + HealthData + FactionData
│  + CombatData + WeaponData (Buffer) + UnitTypeData
│  + LODData + NetworkedEntity
│
│  BUILDING (Здание):
│  LocalTransform + BuildingData + HealthData + FactionData
│  + PowerGridData + ProductionData + LODData
│
│  DRONE (Дрон):
│  LocalTransform + MovementData + HealthData + FactionData
│  + DroneData + AIController + LODData + NetworkedEntity
│
│  RESOURCE (Ресурс):
│  LocalTransform + ResourceData + LODData
│
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 3. МНОГОПОТОЧНАЯ МОДЕЛЬ

### 3.1 Архитектура потоков

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    МНОГОПОТОЧНАЯ АРХИТЕКТУРА                            │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  MAIN THREAD (Главный поток):
│  ├── Input handling (ввод)
│  ├── UI rendering (UI)
│  ├── Camera control (камера)
│  ├── Scene management (сцена)
│  └── Network send/receive (сеть)
│
│  JOB SYSTEM THREAD POOL:
│  ├── Worker 1: Movement
│  ├── Worker 2: Combat
│  ├── Worker 3: Economy
│  └── Worker N: ...
│
│  BACKGROUND THREADS:
│  ├── Pathfinding Thread
│  ├── AI Planning Thread
│  └── Save/Load Thread
│
│  RENDER THREAD:
│  ├── GPU command submission
│  ├── LOD mesh switching
│  ├── Instancing preparation
│  └── Impostor rendering
│
└─────────────────────────────────────────────────────────────────────────┘
```

### 3.2 Распределение систем по потокам

| Система | Поток | Приоритет | Частота |
|---------|-------|-----------|---------|
| **InputSystem** | Main | Critical | Каждый кадр |
| **CameraSystem** | Main | Critical | Каждый кадр |
| **MovementSystem** | Job Pool | High | Каждый кадр |
| **CombatSystem** | Job Pool | High | Каждый кадр |
| **AnimationSystem** | Job Pool | High | Каждый кадр |
| **LODSystem** | Job Pool | Medium | Каждый кадр |
| **PathfindingSystem** | Background | Medium | 10 раз/сек |
| **AIPlanningSystem** | Background | Low | 5 раз/сек |
| **EconomySystem** | Job Pool | Medium | Каждый кадр |
| **MetaUnitSystem** | Background | Low | 2 раза/сек |
| **SaveSystem** | Background | Lowest | По запросу |

### 3.3 Performance Budget (60 FPS = 16.67ms)

```
┌─────────────────────────────────────────────────────────────────────────┐
│              TARGET PERFORMANCE BUDGET                                  │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  MAIN THREAD BUDGET: ~5ms
│  ├── Input + Camera: 0.5ms
│  ├── UI Rendering: 1.0ms
│  ├── Network: 1.0ms
│  └── Overhead: 2.5ms
│
│  JOB SYSTEM BUDGET: ~8ms (параллельно)
│  ├── Movement (10K units): 2.0ms
│  ├── Combat (10K units): 2.0ms
│  ├── Animation (10K units): 2.0ms
│  ├── Economy: 1.0ms
│  └── LOD System: 1.0ms
│
│  RENDER THREAD BUDGET: ~3ms
│  ├── LOD Mesh Switching: 0.5ms
│  ├── Instancing Setup: 1.0ms
│  ├── Impostor Rendering: 0.5ms
│  └── GPU Commands: 1.0ms
│
│  BACKGROUND THREADS (не влияют на FPS):
│  ├── Pathfinding: ~5ms (10 раз/сек)
│  └── AI Planning: ~10ms (5 раз/сек)
│
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 4. СИСТЕМА КАМЕРЫ

### 4.1 Архитектура 4 уровней камеры

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    СИСТЕМА 4 УРОВНЕЙ КАМЕРЫ                             │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  LEVEL 1: MICRO (Микро) — 1 юнит
│  ═══════════════════════════════════════════════════════════
│  Distance: 2-10 units
│  View: 3rd person, over-shoulder
│  Control: WASD + mouse aim (шутер)
│  • Direct unit control
│  • Precision aiming
│  • Cover system visible
│  • Detailed animations
│
│  LEVEL 2: TACTICAL (Тактика) — 5-10 юнитов (отделение)
│  ═══════════════════════════════════════════════════════════
│  Distance: 20-50 units
│  View: Top-down isometric
│  Control: RTS-style selection, formations
│  • Group selection (drag box)
│  • Formation controls
│  • Cover indicators
│  • Ability casting
│
│  LEVEL 3: OPERATIONAL (Оперативный) — рота/батальон (50-500)
│  ═══════════════════════════════════════════════════════════
│  Distance: 100-500 units
│  View: Strategic overhead
│  Control: Command groups, objectives
│  • Meta-units (grouped squads)
│  • Territory control
│  • Supply lines visible
│
│  LEVEL 4: STRATEGIC (Стратегический) — дивизия+ (1000+)
│  ═══════════════════════════════════════════════════════════
│  Distance: 1000+ units
│  View: Abstract map
│  Control: High-level orders, diplomacy
│  • Army movements (abstract)
│  • Resource regions
│  • Diplomacy interface
│
└─────────────────────────────────────────────────────────────────────────┘
```

### 4.2 Camera System Implementation

```csharp
public enum CameraLevel
{
    Micro,       // Level 1: 1 unit
    Tactical,    // Level 2: 5-10 units
    Operational, // Level 3: 50-500 units
    Strategic    // Level 4: 1000+ units
}

public struct CameraData : IComponentData
{
    public CameraLevel CurrentLevel;
    public CameraLevel TargetLevel;
    public float LevelTransitionProgress;
    public float TargetDistance;
    public float CurrentDistance;
    public float3 FocusPosition;
    public quaternion TargetRotation;
}

public partial struct CameraSystem : ISystem
{
    private const float MICRO_MAX_DIST = 15f;
    private const float TACTICAL_MAX_DIST = 60f;
    private const float OPERATIONAL_MAX_DIST = 400f;
    
    public void OnUpdate(ref SystemState state)
    {
        foreach (var camera in SystemAPI.Query<RefRW<CameraData>>())
        {
            UpdateCameraLevel(ref camera.ValueRW);
            UpdateCameraTransform(ref camera.ValueRW);
        }
    }
    
    private void UpdateCameraLevel(ref CameraData camera)
    {
        CameraLevel detectedLevel = camera.CurrentDistance switch
        {
            <= MICRO_MAX_DIST => CameraLevel.Micro,
            <= TACTICAL_MAX_DIST => CameraLevel.Tactical,
            <= OPERATIONAL_MAX_DIST => CameraLevel.Operational,
            _ => CameraLevel.Strategic
        };
        
        if (detectedLevel != camera.TargetLevel)
        {
            camera.TargetLevel = detectedLevel;
            camera.LevelTransitionProgress = 0f;
        }
        
        if (camera.CurrentLevel != camera.TargetLevel)
        {
            camera.LevelTransitionProgress += SystemAPI.Time.DeltaTime * 2f;
            if (camera.LevelTransitionProgress >= 1f)
            {
                camera.CurrentLevel = camera.TargetLevel;
                camera.LevelTransitionProgress = 0f;
            }
        }
    }
    
    private void UpdateCameraTransform(ref CameraData camera)
    {
        camera.CurrentDistance = math.lerp(camera.CurrentDistance, 
            camera.TargetDistance, SystemAPI.Time.DeltaTime * 5f);
        
        float3 offset = math.rotate(camera.TargetRotation, 
            new float3(0, 0, -camera.CurrentDistance));
        // Apply to camera transform...
    }
}
```

### 4.3 Таблица рендеринга по уровням

| Уровень | Расстояние | Детализация юнитов | Детализация зданий | Эффекты | UI |
|---------|------------|-------------------|-------------------|---------|-----|
| **Micro** | 2-15м | Full (скелетная анимация) | Full | Все частицы | Прицел, HP бар |
| **Tactical** | 20-60м | Medium (упрощённая анимация) | Full | Взрывы, выстрелы | Группы, формации |
| **Operational** | 80-400м | Low (билборды) | LOD 2 | Крупные эффекты | Территории, мета-юниты |
| **Strategic** | 600м+ | Impostors/Icons | Impostors | Нет | Стратегическая карта |

---

## 5. ОПТИМИЗАЦИЯ И МАСШТАБИРОВАНИЕ

### 5.1 Архитектура оптимизации

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    СИСТЕМА ОПТИМИЗАЦИИ                                  │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  LAYER 1: ECS DATA OPTIMIZATION
│  • Struct-of-Arrays (SoA) вместо Array-of-Structs (AoS)
│  • Cache-friendly компоненты (< 64 bytes)
│  • Chunk-based хранение (16KB чанки)
│  • Archetype sharing
│
│  LAYER 2: SPATIAL PARTITIONING
│  • Spatial Hash Grid (50m cells)
│  • Quadtree для статических объектов
│  • Visibility culling по фрустуму камеры
│
│  LAYER 3: LOD SYSTEM
│  • 4 уровня LOD для юнитов (Full → Medium → Low → Impostor)
│  • 3 уровня LOD для зданий
│  • Плавные переходы (cross-fade)
│
│  LAYER 4: META-UNITS
│  • Автоматическая группировка близких юнитов
│  • Симуляция группы как одной сущности
│  • Иерархическая структура: Army → Division → Battalion → Squad
│
│  LAYER 5: RENDERING OPTIMIZATION
│  • GPU Instancing для однотипных юнитов
│  • Impostors для дальних объектов
│  • Occlusion Culling (GPU-based)
│
└─────────────────────────────────────────────────────────────────────────┘
```

### 5.2 Meta-Unit System

```csharp
public struct MetaUnit : IComponentData
{
    public int MetaUnitId;
    public int UnitCount;
    public float3 CenterPosition;
    public float Radius;
    public float AverageHealth;
    public float TotalFirepower;
    public bool IsExpanded;
}

[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct MetaUnitSystem : ISystem
{
    private const float GROUPING_DISTANCE = 30f;
    private const int MIN_GROUP_SIZE = 10;
    private const float UPDATE_INTERVAL = 0.5f;
    
    private float _updateTimer;
    private NativeParallelMultiHashMap<int2, Entity> _spatialHash;
    
    public void OnUpdate(ref SystemState state)
    {
        _updateTimer += SystemAPI.Time.DeltaTime;
        if (_updateTimer < UPDATE_INTERVAL) return;
        _updateTimer = 0f;
        
        BuildSpatialHash();
        
        new GroupingJob
        {
            SpatialHash = _spatialHash,
            GroupingDistance = GROUPING_DISTANCE,
            MinGroupSize = MIN_GROUP_SIZE,
        }.ScheduleParallel();
    }
    
    private void BuildSpatialHash()
    {
        _spatialHash.Clear();
        foreach (var (transform, entity) in 
            SystemAPI.Query<LocalTransform>().WithEntityAccess())
        {
            int2 cell = new int2(
                (int)(transform.Position.x / GROUPING_DISTANCE),
                (int)(transform.Position.z / GROUPING_DISTANCE)
            );
            _spatialHash.Add(cell, entity);
        }
    }
}
```

### 5.3 Таблица производительности

| Метрика | Без оптимизации | С LOD | С LOD + Meta-Units | С LOD + Meta + Impostors |
|---------|-----------------|-------|-------------------|--------------------------|
| **10K юнитов** | 5 FPS | 15 FPS | 30 FPS | 60 FPS |
| **Draw Calls** | 10,000 | 3,000 | 500 | 150 |
| **Triangles** | 50M | 15M | 3M | 500K |
| **Memory** | 2 GB | 1.5 GB | 800 MB | 400 MB |
| **CPU Time** | 25ms | 12ms | 6ms | 3ms |
| **GPU Time** | 180ms | 60ms | 20ms | 8ms |

---

## 6. СЕТЕВАЯ АРХИТЕКТУРА

### 6.1 Гибридная сетевая модель

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    ГИБРИДНАЯ СЕТЕВАЯ АРХИТЕКТУРА                        │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  TACTICAL LEVEL (Микро + Тактика) — Lockstep Model
│  ═══════════════════════════════════════════════════════════
│  Distance: 0-200m от игрока
│  Entity Count: ~500 юнитов на игрока
│  • Полная синхронизация состояния
│  • Lockstep с 50ms input delay
│  • Deterministic simulation
│  • Rollback на desync
│
│  OPERATIONAL LEVEL (Оперативный) — Server Authority
│  ═══════════════════════════════════════════════════════════
│  Distance: 200-1000m
│  Entity Count: ~2000 юнитов
│  • Server-authoritative simulation
│  • Предиктивная интерполяция на клиенте
│  • Репликация 10 раз/сек
│
│  STRATEGIC LEVEL (Стратегический) — Event-Based Sync
│  ═══════════════════════════════════════════════════════════
│  Distance: 1000m+
│  Entity Count: 10000+ юнитов
│  • Только важные события
│  • Абстрактная симуляция на сервере
│  • Репликация 2-5 раз/сек
│
└─────────────────────────────────────────────────────────────────────────┘
```

### 6.2 Lockstep Implementation

```csharp
public struct LockstepConfig
{
    public const int INPUT_DELAY_FRAMES = 3;  // 50ms при 60 FPS
    public const int SYNC_CHECK_INTERVAL = 60; // Каждую секунду
}

public struct LockstepState : IComponentData
{
    public int CurrentFrame;
    public int ConfirmedFrame;
    public uint GameHash;
}

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateBefore(typeof(MovementSystem))]
public partial struct LockstepSystem : ISystem
{
    private NativeHashMap<uint, PlayerInput> _pendingInputs;
    
    public void OnUpdate(ref SystemState state)
    {
        var lockstep = SystemAPI.GetSingletonRW<LockstepState>();
        
        CollectLocalInput(lockstep.ValueRO.CurrentFrame);
        SendInputToServer();
        ReceiveConfirmedInputs();
        
        int targetFrame = lockstep.ValueRO.CurrentFrame - LockstepConfig.INPUT_DELAY_FRAMES;
        ApplyInputsForFrame(targetFrame);
        
        if (lockstep.ValueRO.CurrentFrame % LockstepConfig.SYNC_CHECK_INTERVAL == 0)
        {
            PerformSyncCheck();
        }
        
        lockstep.ValueRW.CurrentFrame++;
    }
    
    [BurstCompile]
    private uint CalculateGameStateHash()
    {
        uint hash = 0;
        foreach (var (transform, movement) in 
            SystemAPI.Query<LocalTransform, MovementData>())
        {
            hash = math.hash(new float4(transform.Position, transform.Rotation.value.w));
        }
        return hash;
    }
}
```

### 6.3 Network Replication System

```csharp
public struct ReplicationConfig
{
    public const float PRIORITY_CRITICAL = 1.0f;
    public const float PRIORITY_HIGH = 0.8f;
    public const float PRIORITY_MEDIUM = 0.5f;
    public const float PRIORITY_LOW = 0.2f;
}

public struct ReplicationData : IComponentData
{
    public int NetworkId;
    public float ReplicationPriority;
    public float LastReplicationTime;
    public float ReplicationInterval;
}

[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct NetworkReplicationSystem : ISystem
{
    private NativeList<ReplicationEntry> _replicationQueue;
    private float _replicationTimer;
    
    public void OnUpdate(ref SystemState state)
    {
        _replicationTimer += SystemAPI.Time.DeltaTime;
        
        new UpdateReplicationPriorityJob
        {
            CameraPosition = Camera.main.transform.position,
        }.ScheduleParallel();
        
        if (_replicationTimer >= 0.1f)
        {
            _replicationTimer = 0f;
            BuildReplicationQueue();
            ProcessReplicationQueue();
        }
    }
    
    private void BuildReplicationQueue()
    {
        _replicationQueue.Clear();
        foreach (var (replication, transform, entity) in 
            SystemAPI.Query<ReplicationData, LocalTransform>().WithEntityAccess())
        {
            if (replication.ReplicationPriority > 0)
            {
                _replicationQueue.Add(new ReplicationEntry
                {
                    NetworkId = replication.NetworkId,
                    Priority = replication.ReplicationPriority,
                    Entity = entity,
                });
            }
        }
        _replicationQueue.Sort(new ReplicationPriorityComparer());
    }
}
```

---

## 7. СИСТЕМА СОХРАНЕНИЙ

### 7.1 Архитектура сохранений

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    СИСТЕМА СОХРАНЕНИЙ                                   │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  SAVE FILE STRUCTURE:
│  ┌──────────────┬────────────────────────────────────────────┐
│  │ Header       │ Magic + Version + Timestamp + Checksum     │
│  ├──────────────┼────────────────────────────────────────────┤
│  │ World Data   │ Seed + Size + Time + Weather               │
│  ├──────────────┼────────────────────────────────────────────┤
│  │ Entity Table │ [EntityID, Archetype, ComponentMask, Offset│
│  ├──────────────┼────────────────────────────────────────────┤
│  │ Components   │ Component data (compressed)                │
│  ├──────────────┼────────────────────────────────────────────┤
│  │ Changelog    │ Delta since last save                      │
│  └──────────────┴────────────────────────────────────────────┘
│
│  COMPRESSION PIPELINE:
│  Raw ECS Data → Delta Encoding → ZSTD/LZ4 → Encrypted File
│
│  Размеры:
│  • Raw: ~2 GB (10K entities)
│  • After Delta: ~200 MB (10x reduction)
│  • After Compression: ~50 MB (4x reduction)
│
└─────────────────────────────────────────────────────────────────────────┘
```

### 7.2 Таблица размеров сохранений

| Содержимое | Raw Size | С Delta | Сжатый | Время сохранения |
|------------|----------|---------|--------|------------------|
| **Новая игра** (100 юнитов) | 10 MB | 1 MB | 300 KB | 50ms |
| **Средняя игра** (2K юнитов) | 200 MB | 20 MB | 5 MB | 500ms |
| **Большая игра** (10K юнитов) | 1 GB | 100 MB | 25 MB | 2s |
| **Огромная игра** (50K юнитов) | 5 GB | 500 MB | 125 MB | 10s |

---

## 8. ВИЗУАЛЬНАЯ СИСТЕМА

### 8.1 Архитектура рендеринга

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    ВИЗУАЛЬНАЯ АРХИТЕКТУРА                               │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  RENDER PIPELINE (URP):
│  Shadow Pass → GBuffer → Lighting → Post-Processing → UI
│
│  VISUAL STYLE — "Живописный реализм":
│  • They Are Billions — атмосфера, масштаб
│  • Iron Harvest — детализация юнитов
│  • Foxhole — persistent мир, стратегия
│
│  Ключевые элементы:
│  • Насыщенные, но приглушённые цвета
│  • Драматическое освещение
│  • Погодные эффекты (дождь, туман, песчаная буря)
│  • Частицы для боевых действий
│  • Динамические тени
│
└─────────────────────────────────────────────────────────────────────────┘
```

### 8.2 Погодная система

```csharp
public enum WeatherType
{
    Clear, Cloudy, Rain, Storm, Fog, Snow, Sandstorm
}

public struct WeatherData : IComponentData
{
    public WeatherType CurrentWeather;
    public float TransitionProgress;
    public float Intensity;
    public float WindSpeed;
    public float3 WindDirection;
    public float Visibility;
}

public partial struct WeatherSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var weather = SystemAPI.GetSingletonRW<WeatherData>();
        
        if (weather.ValueRO.CurrentWeather != weather.ValueRO.TargetWeather)
        {
            weather.ValueRW.TransitionProgress += SystemAPI.Time.DeltaTime * 0.1f;
            if (weather.ValueRW.TransitionProgress >= 1f)
            {
                weather.ValueRW.CurrentWeather = weather.ValueRO.TargetWeather;
                weather.ValueRW.TransitionProgress = 0f;
            }
        }
        
        // Update shaders
        Shader.SetGlobalFloat("_WeatherIntensity", weather.ValueRO.Intensity);
        Shader.SetGlobalFloat("_Visibility", weather.ValueRO.Visibility);
    }
}
```

---

## 9. ИНТЕГРАЦИЯ ПОДСИСТЕМ

### 9.1 ECS Integration

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    ИНТЕГРАЦИЯ ПОДСИСТЕМ В ECS                           │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  COLONY SIMULATION LAYER:
│  • NeedsSystem, JobSystem, ProductionSystem
│  • ConstructionSystem, ResearchSystem, TradeSystem
│
│  TACTICAL WARFARE LAYER:
│  • CombatSystem, CoverSystem, FormationSystem
│  • MoraleSystem, SupplySystem
│
│  ECONOMY LAYER:
│  • ResourceSystem, TradeSystem, MarketSystem, PowerSystem
│
│  SHARED SYSTEMS:
│  • MovementSystem, PathfindingSystem, LODSystem
│  • AnimationSystem, NetworkSystem
│
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 10. ИНСТРУМЕНТЫ И PIPELINE

### 10.1 Development Tools

| Инструмент | Назначение |
|------------|------------|
| **Unity Editor** | Основная разработка |
| **Rider/VS** | IDE для C# |
| **Git + LFS** | Контроль версий |
| **Unity Profiler** | Профайлинг |
| **Frame Debugger** | Отладка рендеринга |
| **Memory Profiler** | Анализ памяти |
| **DOTS Editor** | Отладка ECS |

### 10.2 CI/CD Pipeline (GitHub Actions)

```yaml
name: Colony Conquest CI
on: [push, pull_request]

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
        with:
          lfs: true
      - name: Build Windows
        uses: game-ci/unity-builder@v3
        with:
          targetPlatform: StandaloneWindows64
      - name: Run Tests
        uses: game-ci/unity-test-runner@v3
```

---

## 11. ЭТАПЫ РАЗРАБОТКИ

### 11.1 Технические этапы

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    ЭТАПЫ РАЗРАБОТКИ                                     │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  PHASE 0: PROTOTYPE (2-3 месяца)
│  Цель: Проверка технической осуществимости
│  • Настройка Unity DOTS проекта
│  • Базовая ECS архитектура
│  • Прототип MovementSystem (1000 юнитов)
│  • Бенчмарки производительности
│  Метрики: 1000 юнитов = 60 FPS
│
│  PHASE 1: VERTICAL SLICE (4-6 месяцев)
│  Цель: Полноценный игровой цикл на малом масштабе
│  • Полная ECS архитектура
│  • Все 4 уровня камеры
│  • Colony Sim системы
│  • Боевая система
│  Метрики: 2000 юнитов = 60 FPS
│
│  PHASE 2: MASSIVE SCALE (6-9 месяцев)
│  Цель: Масштабирование до 10,000+ юнитов
│  • Meta-unit system
│  • Impostor system
│  • Сетевая архитектура
│  Метрики: 10,000 юнитов = 60 FPS
│
│  PHASE 3: POLISH & RELEASE (3-6 месяцев)
│  Цель: Финальная оптимизация и выпуск
│  • Визуальный полиш
│  • Звук и музыка
│  • Балансировка
│  Метрики: Стабильные 60 FPS
│
└─────────────────────────────────────────────────────────────────────────┘
```

### 11.2 Команда разработки

| Роль | Phase 0-1 | Phase 2 | Phase 3 |
|------|-----------|---------|---------|
| **Tech Lead / Architect** | 1 | 1 | 1 |
| **Gameplay Programmers** | 1 | 2 | 2 |
| **ECS/DOTS Specialist** | 1 | 1 | 1 |
| **Network Programmer** | 0 | 1 | 1 |
| **Graphics Programmer** | 0 | 1 | 1 |
| **AI Programmer** | 0 | 1 | 1 |
| **3D Artists** | 1 | 2 | 3 |
| **Technical Artist** | 0 | 1 | 1 |
| **UI/UX Designer** | 0 | 1 | 1 |
| **Game Designer** | 1 | 1 | 1 |
| **QA** | 0 | 1 | 2 |
| **Итого** | **5** | **13** | **17** |

---

## ЗАКЛЮЧЕНИЕ

Данная техническая спецификация описывает архитектуру для достижения целевых показателей:

| Показатель | Цель | Подход |
|------------|------|--------|
| **10,000+ юнитов** | 60 FPS | ECS + DOTS + LOD + Meta-units |
| **4 уровня камеры** | Плавный переход | Distance-based LOD + Adaptive rendering |
| **Мультиплеер** | < 100ms latency | Hybrid Lockstep + Predictive simulation |
| **Сохранения** | < 3 секунд | Delta compression + ZSTD |
| **Память** | < 2 GB | Archetype sharing + Chunk-based allocation |

Ключевые технические решения:
1. **Unity DOTS** — лучший ECS для масштабных симуляций
2. **Иерархическая сетевая модель** — оптимальный баланс точности и производительности
3. **4-слойная оптимизация** — от данных до рендеринга
4. **Meta-unit система** — абстракция для масштабирования
5. **Job System** — максимальное использование многопоточности
