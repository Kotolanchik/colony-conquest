# Colony & Conquest — Unity Client

Проект собран на базе официального **EntityComponentSystemSamples / EntitiesSamples** (Unity Technologies): DOTS, URP, Unity Physics, Input System.

## Версия редактора

См. `ProjectSettings/ProjectVersion.txt`. Сейчас: **Unity 6** (6000.2.x) — см. ADR `docs/decisions/002-unity6-editor-baseline.md`.

Установите соответствующую версию через **Unity Hub** (редактор должен совпадать с `m_EditorVersion`).

## Первый запуск

1. Откройте папку `game/Client` как проект в Unity Hub.
2. Дождитесь импорта и разрешения пакетов (появится `Packages/packages-lock.json` — его стоит закоммитить после успешного разрешения).
3. Откройте сцену **`Assets/_Project/Scenes/Bootstrap.unity`** (двойной клик) и нажмите **Play**. Сцена уже в **File → Build Settings** как первая.
4. В консоли — лог создания синглтона `SimulationRootState` и подсистем (`SubsystemBootstrapUtility` в `GameBootstrapSystem`). В подсцене **`SubScenes/GameSubScene`** — объект **TestMoveTarget** (authoring → после baking сущность с `LocalTransform` + `PlayerMoveTargetTag`); **WASD** сдвигает её через `PlayerMoveFromInputSystem` (плоскость XZ). **B** — переключение режима строительства (`ConstructionBuildModeToggleSystem` → `ConstructionGhostState`), якорь призрака обновляет `ConstructionGhostCursorSystem`. На первом тике симуляции `StoryEventPipelineSystem` кладёт тестовое событие в очередь и снимает его с записью в `AnalyticsRecordEntry`; `AudioBusStub` → буфер, `AudioBusDrainSystem` очищает очередь (заглушка до FMOD/Wwise). `WorldMapFocusFromPlayerSystem` обновляет `WorldMapFocusState` по чанку игрока. При первом импорте Unity может пересчитать baking SubScene.

**Проверка агро/добычи (`spec/agriculture_mining_spec.md`):** в bootstrap — демо-грядка (`CropCareDailySystem` — уход в фазе роста: вредители/сорняки; `CropGrowthSimulationSystem` — этапы §1.2, урожай §1.3 с водой `WaterSupplyKind`, пищевой ценностью §1.1 и сезонностью §1.2), два загона (`LivestockDailyProductionSystem` — куры и козы), лес `(3,0,0)` и железная руда `(6,0,2)` — удерживайте **E** для `ManualMiningGatherSystem`. Нагрузка агрохимии — синглтон `ColonyAgrochemicalLoadState`. Дополнительные грядки: `CropPlotAuthoring` в SubScene. Склад — `ResourceStockpileSingleton` / `ResourceStockEntry`.

**Проверка селекции/религии/жилья (`spec/plant_breeding_spec.md`, `spec/religion_cults_spec.md`, `spec/comfort_housing_spec.md`):**

- Селекция: в мире создаётся `PlantBreedingLabSingleton` с буферами `PlantCultivarEntry` и `PlantBreedingWorkOrderEntry`; `PlantBreedingDailySimulationSystem` раз в игровой день завершает заявки, добавляет новые сорта и пишет метрики `PlantBreeding*`.
- Религия: синглтоны `ReligionSimulationState`, `ReligiousConflictState`, `HolyWarState`; `ReligionDailySimulationSystem` считает веру/напряжение, запускает фазы священной войны и публикует события в `GameEventQueueEntry`.
- Жильё: демо-юниты `HousingUnitRuntime` + очередь расселения `HousingAssignmentRequestEntry`; `HousingAssignmentSystem` и `HousingDailyComfortSystem` пересчитывают расселение, уют, износ и аварии, метрики `Housing*`.
- В **Entities Hierarchy** проверка: наличие перечисленных синглтонов/буферов; при росте `DayIndex` у `GameCalendarState` значения в них меняются ежедневно.

## Замер ECS (фаза 0, дорожная карта §6.1)

Ориентир из мастер-спеки: **~1000 сущностей при 60 FPS**. В коде:

1. В `Assets/_Project/Scripts/BenchmarkPhase0Tuning.cs` выставьте **`Enabled = true`** (по умолчанию `false`, чтобы не спавнить 1000 сущностей без явного намерения).
2. Запустите Play на сцене **Bootstrap** — в консоли появятся строки `[Colony & Conquest] Benchmark phase 0: ...` с числом сущностей замера, средним `dt` и оценкой FPS.
3. Системы: `BenchmarkSpawnSystem` (однократный спавн сетки), `BenchmarkDriftSystem` (синтетическая нагрузка на `LocalTransform`), `BenchmarkReportSystem` (периодический отчёт; запрос сущностей кэшируется в `OnCreate`).

## Структура

- `Assets/_Project/Scenes/` — стартовая сцена **Bootstrap** и **SubScenes/GameSubScene** (DOTS).
- `Assets/_Project/Settings/ColonyInputActions.inputactions` — карта ввода (дублирует JSON в `InputActionsJson.cs` — менять вместе): **Move** (WASD), **ToggleBuild** (B).
- `Assets/_Project/Scripts/` — игровые сборки (`Colony.Conquest.Core`): `SimulationRootState`, `InputCommandState`, `InputGatherSystem` (WASD → синглтон), `GameBootstrapSystem` + `SubsystemBootstrapUtility` (аналитика, аудио-очередь, сюжетная очередь, карта, **календарь**, netcode-спайк, демо-грядка/загон/месторождение, демо-боевой юнит), `ColonyCalendarAdvanceSystem` → `GameCalendarState`, `CropGrowthSimulationSystem` (§1.2–1.3 `spec/agriculture_mining_spec.md`), `LivestockDailyProductionSystem` (§1.4), `PlayerMoveTargetTag` / `PlayerMoveTargetAuthoring` + Baker, `PlayerMoveFromInputSystem`, `WorldMapFocusFromPlayerSystem`, `StoryEventPipelineSystem`, `AudioBusDrainSystem`, строительство (`ConstructionGhostBootstrapSystem`, `ConstructionBuildModeToggleSystem`, `ConstructionGhostCursorSystem`), доменные enum и данные (`Agriculture`, `Technology`, `Ecology`, …), замер фазы 0 (`BenchmarkPhase0Tuning`, …).
- `Assets/_Project/Scripts/PlantBreeding/` — селекция: `PlantBreedingLabState`, `PlantCultivarEntry`, `PlantBreedingWorkOrderEntry`, `PlantBreedingMath`, `PlantBreedingDailySimulationSystem`.
- `Assets/_Project/Scripts/Religion/` — религия и культы: `ReligionSimulationState`, `ReligiousConflictState`, `CultActivityState`, `HolyWarState`, `ReligionDailySimulationSystem`.
- `Assets/_Project/Scripts/Housing/` — жильё и комфорт: `HousingUnitRuntime`, `HousingComfortSnapshot`, `HousingAssignmentSystem`, `HousingDailyComfortSystem`, `HousingMath`.
- `Assets/_Project/Scripts/Settlers/` — схема ECS-компонентов поселенца по `spec/settler_simulation_system_spec.md` §1.1–1.7 (пространство имён `ColonyConquest.Settlers`; симуляция не подключена — только типы).
- `Assets/_Project/Scripts/Economy/` — идентификаторы и каталог ресурсов по `spec/economic_system_specification.md` §1.2: `ResourceId` (промышленный блок 1…55, сельхоз 56…67, доп. эпоха 1: 68…72, добыча/прочее 73…77, эпоха 2: известь/прокат 78…79, рыба 80, артиллерия эпохи 1: 81…82), `ResourceCatalog`, `ColonyConquest.Economy`.

### Экономика (данные)

- Список ресурсов и **базовые цены** соответствуют таблицам §1.2 экономической спеки; доступ: `ResourceCatalog.Get(ResourceId.IronOre)` и т.д.
- Добавление нового ресурса: только **новое значение в конце** `ResourceId` + строка в `ResourceCatalog.BuildTable()` и обновление спеки; версионирование сохранений — отдельная задача.
- Фермерство и добыча (`spec/agriculture_mining_spec.md`): `CropCareDailySystem`, `CropGrowthSimulationSystem`, `FertilizerEcologyTuning` → `ColonyAgrochemicalLoadState` → `AgrochemicalEcologyBridgeSystem` → `ColonyEcologyIndicatorsState` (связь с `AnalyticsLocalSnapshot.Social.Ecology01`), `LivestockDailyProductionSystem`, `ManualMiningGatherSystem` (**E**), `MiningForestRegenerationSystem`, `IndustrialMiningFormulas` + `IndustrialMiningProductionSystem` (демо-карьер), `MiningHazardDailySystem`; демо в `SubsystemBootstrapUtility`.
- Производство (`spec/economic_system_specification.md` §2.1–2.3): `ProductionEfficiencyMath`, `ProductionRecipeCatalog` (эпоха 1 + домен/конвертер/прокат/НПЗ/динамит/известь), `EconomyWorkshopProductionSystem` — приоритетный список рецептов; bootstrap: руда, нефть, кокс, камень, уголь и пр.
- `Packages/manifest.json` — зависимости (Entities, URP, Input System, Unity Physics).

## Сеть (спайк)

В `Packages/manifest.json` — `com.unity.netcode`; точка входа `ColonyNetcodeBootstrap` (`ClientServerBootstrap`), сценарий «go in game» — `GoInGameSpike.cs`. Синглтон `NetcodeSpikeState` создаётся в `SubsystemBootstrapUtility`; `NetcodeSpikeStateSyncSystem` выставляет `TransportReady`, когда в мире есть `NetworkStreamInGame`. Дальнейшая синхронизация геймплейных сущностей — отдельные задачи.
