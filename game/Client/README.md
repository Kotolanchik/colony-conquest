# Colony & Conquest — Unity Client

Проект собран на базе официального **EntityComponentSystemSamples / EntitiesSamples** (Unity Technologies): DOTS, URP, Unity Physics, Input System.

## Версия редактора

См. `ProjectSettings/ProjectVersion.txt`. Сейчас: **Unity 6** (6000.2.x) — см. ADR `docs/decisions/002-unity6-editor-baseline.md`.

Установите соответствующую версию через **Unity Hub** (редактор должен совпадать с `m_EditorVersion`).

## Первый запуск

1. Откройте папку `game/Client` как проект в Unity Hub.
2. Дождитесь импорта и разрешения пакетов (появится `Packages/packages-lock.json` — его стоит закоммитить после успешного разрешения).
3. Откройте сцену **`Assets/_Project/Scenes/Bootstrap.unity`** (двойной клик) и нажмите **Play**. Сцена уже в **File → Build Settings** как первая.
4. В консоли — лог создания синглтона `SimulationRootState` и подсистем (`SubsystemBootstrapUtility` в `GameBootstrapSystem`). В подсцене **`SubScenes/GameSubScene`** — объект **TestMoveTarget** (authoring → после baking сущность с `LocalTransform` + `PlayerMoveTargetTag`); **WASD** сдвигает её через `PlayerMoveFromInputSystem` (плоскость XZ). **B** — переключение режима строительства (`ConstructionBuildModeToggleSystem` → `ConstructionGhostState`), якорь призрака обновляет `ConstructionGhostCursorSystem`. `EventsQuestDailySystem` ведёт полный контур событий/квестов, а `StoryEventPipelineSystem` остаётся fallback-обработчиком очереди для legacy-режима. `UiUxRuntimeSystem` обновляет hotkeys/панели/уведомления, `AudioSimulationRuntimeSystem` — адаптивную музыку и ingest аудио-шины; `AudioBusDrainSystem` остаётся только fallback, если full-audio singleton не активен. `WorldMapFocusFromPlayerSystem` обновляет `WorldMapFocusState` по чанку игрока. При первом импорте Unity может пересчитать baking SubScene.

**Проверка агро/добычи (`spec/agriculture_mining_spec.md`):** в bootstrap — демо-грядка (`CropCareDailySystem` — уход в фазе роста: вредители/сорняки; `CropGrowthSimulationSystem` — этапы §1.2, урожай §1.3 с водой `WaterSupplyKind`, пищевой ценностью §1.1 и сезонностью §1.2), два загона (`LivestockDailyProductionSystem` — куры и козы), лес `(3,0,0)` и железная руда `(6,0,2)` — удерживайте **E** для `ManualMiningGatherSystem`. Нагрузка агрохимии — синглтон `ColonyAgrochemicalLoadState`. Дополнительные грядки: `CropPlotAuthoring` в SubScene. Склад — `ResourceStockpileSingleton` / `ResourceStockEntry`.

**Проверка селекции/религии/жилья (`spec/plant_breeding_spec.md`, `spec/religion_cults_spec.md`, `spec/comfort_housing_spec.md`):**

- Селекция: в мире создаётся `PlantBreedingLabSingleton` с буферами `PlantCultivarEntry` и `PlantBreedingWorkOrderEntry`; `PlantBreedingDailySimulationSystem` раз в игровой день завершает заявки, добавляет новые сорта и пишет метрики `PlantBreeding*`.
- Религия: синглтоны `ReligionSimulationState`, `ReligiousConflictState`, `HolyWarState`; `ReligionDailySimulationSystem` считает веру/напряжение, запускает фазы священной войны и публикует события в `GameEventQueueEntry`.
- Жильё: демо-юниты `HousingUnitRuntime` + очередь расселения `HousingAssignmentRequestEntry`; `HousingAssignmentSystem` и `HousingDailyComfortSystem` пересчитывают расселение, уют, износ и аварии, метрики `Housing*`.
- В **Entities Hierarchy** проверка: наличие перечисленных синглтонов/буферов; при росте `DayIndex` у `GameCalendarState` значения в них меняются ежедневно.

**Проверка дипломатии/правосудия/развлечений (`spec/diplomacy_trade_spec.md`, `spec/crime_justice_spec.md`, `spec/entertainment_spec.md`):**

- Дипломатия: `DiplomacySimulationSingleton` + буферы `FactionProfileEntry`, `DiplomaticRelationEntry`, `TradeDealEntry`, `DiplomaticAllianceEntry`, `ActiveWarEntry`; `DiplomacyDailySystem` обновляет отношения/сделки, и пишет метрики `Diplomacy*`.
- Правосудие: `CrimeJusticeSingleton` + `CrimeJusticeState`, `PoliceForceState`, `JusticeCourtState`; `CrimeJusticeDailySystem` генерирует ежедневные инциденты, рассчитывает раскрываемость и рецидив, пишет метрики `Crime*`.
- Развлечения: `EntertainmentSimulationSingleton` + `EntertainmentSimulationState`; `EntertainmentDailySystem` считает итоговое настроение/продуктивность, праздники и передаёт `EntertainmentAccess01` в `CrimeJusticeState`.
- В очереди событий `GameEventQueueEntry` появляются события по торговым завершениям/войнам, преступлениям и праздникам.

**Проверка технологий/политики/глобальной карты (`spec/technology_tree_spec.md`, `spec/political_system_spec.md`, `spec/global_map_spec.md`):**

- Технологии: `TechTreeSimulationSingleton` + `TechTreeSimulationState` + буфер `TechUnlockedEntry`; `TechTreeDailySystem` ведёт активное исследование, разблокировки и переходы эпох.
- Политика: `PoliticalSimulationSingleton`, `PoliticalSimulationState`, `PoliticalLawState`; `PoliticalDailySystem` пересчитывает модификаторы доктрины/законов и обновляет связки с технологиями/crime/дипломатией.
- Глобальная карта: `WorldMapSimulationSingleton` + буферы `WorldResourceNodeEntry`, `TerritoryControlEntry`, `StrategicArmyEntry`, `SpecialSiteEntry`; `WorldMapDailySimulationSystem` обновляет открытие чанков, влияние территорий и стратегическое движение армий.
- У `WorldMapFocusState` поле `ActiveScale` меняется через `WorldMapScaleFromTechSystem` по текущей эпохе.
- В аналитике появляются метрики `Tech*` (runtime), `Politics*`, `WorldMap*`.

**Проверка строительства/укреплений/биоинженерии (`spec/construction_system_spec.md`, `spec/defensive_structures_spec.md`, `spec/bioengineering_spec.md`):**

- Строительство: `ConstructionRuntimeBootstrapSystem` создаёт `ConstructionSimulationSingleton` и буфер `ConstructionProjectEntry`; `ConstructionRuntimeDailySystem` по `GameCalendarState.DayIndex` списывает материалы со склада (`ResourceStockEntry`), продвигает стадии (`Planning→...→Completed`) и завершает проекты.
- Оборона: `DefensiveBootstrapSystem` создаёт `DefensiveSimulationSingleton`, буферы `DefensiveConstructionOrderEntry`/`DefensiveStructureRuntimeEntry`; `DefensiveDailySystem` строит укрепления с учётом `UnderFireIntensity`, применяет суточный урон и энергопитание high-tech сооружений.
- Биоинженерия: `BioengineeringBootstrapSystem` создаёт `BioengineeringSimulationSingleton`, буферы `BioPatientEntry`/`BioengineeringProcedureEntry`; `BioengineeringDailySystem` обрабатывает процедуры (протезы, стимуляторы, генная терапия, клонирование, нейроинтерфейсы), эффекты/откат стимуляторов и зависимость.
- В `GameEventQueueEntry` появляются события `build-complete`/`defense-built`/`bio-success`; в аналитике пишутся метрики `Construction*`, `Defense*`, `Bioengineering*`.

**Проверка полной экономики (`spec/economic_system_specification.md`):**

- В мире есть `EconomySimulationSingleton` + состояния: `EconomySimulationState`, `EconomyEnergyState`, `EconomyLogisticsState`, `EconomyWarehouseState`, `EconomyMilitaryIndustryState`, `EconomyArmySupplyState`.
- Буферы экономики: `EconomyProductionFacilityEntry`, `EconomyPowerGeneratorEntry`, `EconomyTransportRouteEntry`, `EconomyWarehouseEntry`.
- `EconomySimulationDailySystem` раз в игровой день:
  - ведёт фазы цикла экономики,
  - считает генерацию/потери/доставку энергии,
  - прогоняет производство по рецептам с формулами эффективности,
  - обновляет логистику/склады и bottleneck,
  - считает adequacy снабжения армии и приоритет военного производства.
- В аналитике появляются метрики `EconomyPower*`, `EconomyLogistics*`, `EconomyWarehouse*`, `EconomyArmySupplyAdequacy01`, `EconomyCurrentCyclePhase`; в `AnalyticsLocalSnapshot.Economy` заполняются инфляция/безработица/экспорт/импорт/баланс.

**Проверка симуляции поселенцев (`spec/settler_simulation_system_spec.md`):**

- В мире присутствуют `SettlerSimulationSingleton`, `SettlerSimulationState`, `SettlerAutonomyPolicyState` и буфер `SettlerRelationshipEdge`.
- `SettlerSimulationBootstrapSystem` создаёт стартовую популяцию сущностей с полным набором компонентов (`SettlerIdentity`, `SkillSet`, `PsychologyState`, `PhysiologyState`, `NeedsState`, `SocialBonds`, `LifecycleState`, `SettlerStats`, теги `IsHungry/IsExhausted/HasMentalBreak/...`).
- `SettlerSimulationDailySystem` по `GameCalendarState.DayIndex` обновляет нужды, стресс/настроение, травмы/инфекции, рост и деградацию навыков, уровни автономии, демографию, а также интеграции с `ResourceStockEntry`, `CrimeJusticeState`, `ColonyTechProgressState`.
- В аналитике появляются метрики `Settler*` (население, настроение, стресс, эффективность, доля голодных/истощённых/инфицированных), а `AnalyticsLocalSnapshot.Social` заполняется фактическими данными из `SettlerSimulationState`.

**Проверка экологии и загрязнения (`spec/ecology_spec.md`):**

- В мире есть `EcologySimulationSingleton` + `EcologySimulationState`, `EcologyMitigationState`, `EcologyDisasterState`, а также буферы `EcologyAirSourceEntry`, `EcologyWaterSourceEntry`, `EcologySoilSourceEntry`.
- `EcologySimulationDailySystem` раз в игровой день:
  - рассчитывает загрязнение воздуха/воды/почвы из источников и экономики,
  - применяет уровни очистки и восстановление природы,
  - пересчитывает климат (`GreenhouseGasIndex`, `TemperatureAnomalyC`, `ExtremeWeatherRisk01`),
  - генерирует события `eco-catastrophe` / `eco-protest` / `climate-anomaly`.
- `ColonyEcologyIndicatorsState` и `ColonyPollutionSummaryState` обновляются в runtime; в аналитике пишутся метрики `Ecology*`, а `AnalyticsLocalSnapshot.Social.Ecology01` синхронизирован с фактическим состоянием экосистемы.
- Интеграция: `SettlerSimulationDailySystem` использует `ColonyPollutionSummaryState.Band` для модификации здоровья/настроения населения; `CropGrowthSimulationSystem` использует тот же band для урожайности.

**Проверка производственных заводов (`spec/manufacturing_plants_spec.md`):**

- В мире есть `ManufacturingSimulationSingleton`, `ManufacturingSimulationState` и буферы:
  - `ManufacturingPlantRuntimeEntry`,
  - `ManufacturingProductionOrderEntry`,
  - `ManufacturingProductStockEntry`.
- `ManufacturingSimulationDailySystem` раз в игровой день:
  - рассчитывает выпуск по заказам с учётом рабочих, автоматизации, состояния завода, энергии и эпохи технологий,
  - применяет переключение «мечи или плуги» (`Peace/Partial/Total/ResourceSaving`) и штраф переоснастки на 7 дней,
  - списывает входные ресурсы со склада (`ResourceStockEntry`) и добавляет выпуск в склад/виртуальный product stock.
- Интеграция:
  - c экономикой — учитывается `EconomyEnergyState` (энергодефицит режет выпуск), обновляются military/civilian output агрегаты,
  - c технологиями — блокировка заказов, если требуемая эпоха не достигнута,
  - c поселенцами — качество труда берётся из `SettlerSimulationState.AverageWorkEfficiency01`.
- В очереди событий `GameEventQueueEntry` появляются `manufacturing-policy-switch`, `manufacturing-order-completed`, `manufacturing-resource-blocked`, `manufacturing-era-blocked`; в аналитике — метрики `Manufacturing*`.

**Проверка военной системы (`spec/military_system_specification.md`):**

- В мире есть `MilitarySimulationSingleton`, `MilitarySimulationState`, `MilitaryEnvironmentState`, `MilitaryCommandRelayState`, а также буферы:
  - `MilitaryFormationEntry`,
  - `MilitaryOperationOrderEntry`,
  - `MilitaryMetaUnitEntry`.
- `MilitarySimulationBootstrapSystem` создаёт стартовый состав войск (боевые сущности с `BattleUnitTag`, `MilitaryUnitRuntimeState`, `WoundedState`, `MilitaryCoverState`, `CombatStats`, `CommandHierarchy`).
- `MilitarySimulationDailySystem` раз в игровой день:
  - пересчитывает погодно-временные модификаторы боя (видимость, точность, мобильность, связь),
  - проводит задержки/доставку приказов по иерархии командования,
  - обновляет мораль/подавление/усталость/боеготовность, расход боеприпасов и топлива,
  - ведёт потери (killed/wounded/MIA), исходы боёв, резерв и территориальный прогресс,
  - формирует дальние `MilitaryMetaUnitEntry` для LOD4+ агрегации.
- Интеграция:
  - c экономикой — `EconomyArmySupplyState` влияет на боеготовность и медицинскую выживаемость; snapshot бюджета берёт `MilitaryProductionShare01`,
  - c производством — `ManufacturingSimulationState.MilitaryOutputToday` пополняет резерв,
  - c миром/обороной/социальным контуром — учитываются `StrategicArmyEntry`, состояние укреплений и колониальная мораль поселенцев.
- В аналитике пишутся метрики `Military*` (`MilitaryAverageMorale01`, `MilitaryCombatReadiness01`, `MilitaryMetaUnitsCount`, `MilitaryWeatherSeverity01`), в очереди событий появляются `mil-battle`, `mil-heavy-casualties`, `mil-supply-collapse`, `mil-panic`.

**Проверка событий и квестов (`spec/events_quests_spec.md`):**

- В мире есть `StorySimulationSingleton`, `StorySimulationState` и буферы:
  - `StoryEventDefinitionEntry`,
  - `StoryEventCooldownEntry`,
  - `StoryActiveEventEntry`,
  - `StoryEventHistoryEntry`,
  - `QuestRecordEntry`,
  - `PersonalStoryArcEntry`.
- `EventsQuestDailySystem` раз в игровой день:
  - импортирует и классифицирует события из `GameEventQueueEntry`,
  - выбирает новые события через AI Director (весовой выбор по `AiDirectorDimensionsState` + `AiDirectorPolicyState`),
  - применяет immediate/ongoing эффекты по категориям (природные, военные, социальные, экономические, технологические, глобальные),
  - генерирует и прогрессирует квесты (delivery/escort/eliminate/find/defend/investigate),
  - ведёт персональные арки поселенцев (архетипы §4.2) и timeline истории.
- Интеграция: с `SettlerSimulationState`, `MilitarySimulationState`, `EconomySimulationState`, `ColonyTechProgressState`, `ColonyEcologyIndicatorsState`, `WorldMap` и дипломатией.
- В аналитике пишутся метрики `Events*`, `Quests*`, `StoryArc*`, `StoryTension01`.

**Проверка UI/UX (`spec/ui_ux_spec.md`):**

- В мире есть `UiUxSimulationSingleton`, `UiUxSimulationState` и буферы:
  - `UiNotificationEntry`,
  - `UiHotkeyBindingEntry`,
  - `UiPanelStateEntry`,
  - `UiResourceIndicatorEntry`.
- `UiUxRuntimeSystem` на клиенте:
  - обрабатывает hotkeys (`Space`, `Tab`, `F1-F4`, `~`, `1-3`, `Esc`) и считает активации,
  - адаптирует `UiPanelStateEntry.IsVisible` под уровень камеры (`Micro/Tactical/Operational/Strategic`) и режим строительства,
  - пересчитывает bands ресурсов (food/energy/army-supply), стресс интерфейса и нагрузку HUD,
  - ведёт ленту уведомлений (critical/important/info/achievement) с авто-паузой на critical.
- В аналитике пишутся метрики `Ui*` (`UiCameraLevel`, `UiNotificationsActive`, `UiResourceStress01`, `UiHudLoad01`, `UiHotkeyActivationsToday`).

**Проверка аудио (`spec/audio_design_spec.md`):**

- В мире есть `AudioSimulationSingleton`, `AudioSimulationState` и буферы:
  - `AudioActiveEmitterEntry`,
  - `AudioMusicTransitionEntry`.
- `AudioSimulationRuntimeSystem` на клиенте:
  - рассчитывает adaptive music (интенсивность и уровень) по бою/кризису/времени суток/масштабу камеры,
  - выполняет ingest `AudioBusPendingEntry` в runtime-эмиттеры с приоритетами категорий SFX,
  - применяет 3D attenuation/occlusion/reverb по биому/погоде и держит бюджет `64` голоса / `32` 3D-источника.
- `AudioBusDrainSystem` при активном full-audio singleton не очищает шину (legacy fallback).
- В аналитике пишутся метрики `Audio*` (`AudioMusicIntensity01`, `AudioActiveVoices`, `AudioActive3dSources`, `AudioEstimatedMemoryMb`, `AudioEstimatedLatencyMs`).

## Замер ECS (фаза 0, дорожная карта §6.1)

Ориентир из мастер-спеки: **~1000 сущностей при 60 FPS**. В коде:

1. В `Assets/_Project/Scripts/BenchmarkPhase0Tuning.cs` выставьте **`Enabled = true`** (по умолчанию `false`, чтобы не спавнить 1000 сущностей без явного намерения).
2. Запустите Play на сцене **Bootstrap** — в консоли появятся строки `[Colony & Conquest] Benchmark phase 0: ...` с числом сущностей замера, средним `dt` и оценкой FPS.
3. Системы: `BenchmarkSpawnSystem` (однократный спавн сетки), `BenchmarkDriftSystem` (синтетическая нагрузка на `LocalTransform`), `BenchmarkReportSystem` (периодический отчёт; запрос сущностей кэшируется в `OnCreate`).

## Структура

- `Assets/_Project/Scenes/` — стартовая сцена **Bootstrap** и **SubScenes/GameSubScene** (DOTS).
- `Assets/_Project/Settings/ColonyInputActions.inputactions` — карта ввода (дублирует JSON в `InputActionsJson.cs` — менять вместе): **Move** (WASD), **ToggleBuild** (B).
- `Assets/_Project/Scripts/` — игровые сборки (`Colony.Conquest.Core`): `SimulationRootState`, `InputCommandState`, `InputGatherSystem` (WASD → синглтон), `GameBootstrapSystem` + `SubsystemBootstrapUtility` (аналитика, аудио-очередь, сюжетная очередь, карта, **календарь**, netcode-спайк, демо-грядка/загон/месторождение), `ColonyCalendarAdvanceSystem` → `GameCalendarState`, `CropGrowthSimulationSystem` (§1.2–1.3 `spec/agriculture_mining_spec.md`), `LivestockDailyProductionSystem` (§1.4), `PlayerMoveTargetTag` / `PlayerMoveTargetAuthoring` + Baker, `PlayerMoveFromInputSystem`, `WorldMapFocusFromPlayerSystem`, `StoryEventPipelineSystem`, `AudioBusDrainSystem` (fallback), строительство (`ConstructionGhostBootstrapSystem`, `ConstructionBuildModeToggleSystem`, `ConstructionGhostCursorSystem`), доменные enum и данные (`Agriculture`, `Technology`, `Ecology`, …), замер фазы 0 (`BenchmarkPhase0Tuning`, …).
- `Assets/_Project/Scripts/PlantBreeding/` — селекция: `PlantBreedingLabState`, `PlantCultivarEntry`, `PlantBreedingWorkOrderEntry`, `PlantBreedingMath`, `PlantBreedingDailySimulationSystem`.
- `Assets/_Project/Scripts/Religion/` — религия и культы: `ReligionSimulationState`, `ReligiousConflictState`, `CultActivityState`, `HolyWarState`, `ReligionDailySimulationSystem`.
- `Assets/_Project/Scripts/Housing/` — жильё и комфорт: `HousingUnitRuntime`, `HousingComfortSnapshot`, `HousingAssignmentSystem`, `HousingDailyComfortSystem`, `HousingMath`.
- `Assets/_Project/Scripts/Diplomacy/` — межфракционные отношения, торговые сделки, союзы и войны: `DiplomacySimulationState`, `DiplomacyDailySystem`, `DiplomacyMath`.
- `Assets/_Project/Scripts/Justice/` — преступность, полиция, суд, наказания и рецидив: `CrimeJusticeState`, `CrimeJusticeDailySystem`, `CrimeJusticeMath`.
- `Assets/_Project/Scripts/Entertainment/` — досуг и праздники, влияние на настроение/продуктивность и преступность: `EntertainmentSimulationState`, `EntertainmentDailySystem`, `EntertainmentMath`.
- `Assets/_Project/Scripts/Technology/` — runtime дерева технологий: `TechTreeSimulationState`, `TechTreeCatalog`, `TechTreeDailySystem` (разблокировки/переход эпох).
- `Assets/_Project/Scripts/Politics/` — политический контур: `PoliticalSimulationState`, `PoliticalLawState`, `PoliticalDailySystem`, `PoliticalMath`.
- `Assets/_Project/Scripts/World/` — симуляция глобальной карты: `WorldMapSimulationState`, территориальный контроль, стратегические армии, `WorldMapDailySimulationSystem`.
- `Assets/_Project/Scripts/Military/` — полный runtime военной системы: иерархия командования, приказы, боевые unit-runtime состояния, погода/ночь, потери/медицина, мета-юниты LOD, `MilitarySimulationDailySystem`.
- `Assets/_Project/Scripts/Construction/` — runtime строительства: `ConstructionSimulationState`, `ConstructionProjectEntry`, `ConstructionRuntimeDailySystem` + режим призрака (`ConstructionGhostState`, `ConstructionBuildModeToggleSystem`).
- `Assets/_Project/Scripts/Defense/` — оборонительные сооружения: `DefensiveSimulationState`, `DefensiveConstructionOrderEntry`, `DefensiveStructureRuntimeEntry`, `DefensiveDailySystem`.
- `Assets/_Project/Scripts/Bioengineering/` — биоинженерия: `BioengineeringSimulationState`, `BioPatientEntry`, `BioengineeringProcedureEntry`, `BioengineeringDailySystem`.
- `Assets/_Project/Scripts/Economy/` — полный runtime экономики: `EconomySimulationState`, энергия/логистика/склады/военное производство/снабжение, `EconomySimulationDailySystem`, плюс каталоги ресурсов и рецептов.
- `Assets/_Project/Scripts/Ecology/` — полный runtime экологии: источники загрязнения, меры очистки, климат, восстановление природы, `EcologySimulationDailySystem`, `EcologySimulationMath`.
- `Assets/_Project/Scripts/Manufacturing/` — полный runtime заводов: типы площадок/продукции, мобилизационные режимы, queue-based выпуск и интеграции (`ManufacturingSimulationDailySystem`).
- `Assets/_Project/Scripts/Settlers/` — полный runtime поселенцев: генератор (`SettlerCharacterGenerator`), фабрика сущностей (`SettlerEntityFactory`), bootstrap (`SettlerSimulationBootstrapSystem`), daily-контур (`SettlerSimulationDailySystem`), формулы и ECS-компоненты §1–§9 спеки.
- `Assets/_Project/Scripts/Story/` — полный runtime событий/квестов: AI Director catalog/cooldowns/active history, процедурные квесты, персональные арки (`EventsQuestBootstrapSystem`, `EventsQuestDailySystem`, `EventsQuestSimulationMath`).
- `Assets/_Project/Scripts/UI/` — полный runtime UI/UX: адаптивные уровни камеры, панели HUD, уведомления, accessibility и hotkey-телеметрия (`UiUxBootstrapSystem`, `UiUxRuntimeSystem`, `UiUxSimulationMath`).
- `Assets/_Project/Scripts/Audio/` — полный runtime аудио: adaptive music, ingest шины SFX, бюджет голосов/3D источников, transition history (`AudioSimulationBootstrapSystem`, `AudioSimulationRuntimeSystem`, `AudioSimulationMath`).

### Экономика (данные)

- Список ресурсов и **базовые цены** соответствуют таблицам §1.2 экономической спеки; доступ: `ResourceCatalog.Get(ResourceId.IronOre)` и т.д.
- Добавление нового ресурса: только **новое значение в конце** `ResourceId` + строка в `ResourceCatalog.BuildTable()` и обновление спеки; версионирование сохранений — отдельная задача.
- Фермерство и добыча (`spec/agriculture_mining_spec.md`): `CropCareDailySystem`, `CropGrowthSimulationSystem`, `FertilizerEcologyTuning` → `ColonyAgrochemicalLoadState` → `AgrochemicalEcologyBridgeSystem` → `ColonyEcologyIndicatorsState` (связь с `AnalyticsLocalSnapshot.Social.Ecology01`), `LivestockDailyProductionSystem`, `ManualMiningGatherSystem` (**E**), `MiningForestRegenerationSystem`, `IndustrialMiningFormulas` + `IndustrialMiningProductionSystem` (демо-карьер), `MiningHazardDailySystem`; демо в `SubsystemBootstrapUtility`.
- Полная экономика (`spec/economic_system_specification.md`): `EconomySimulationDailySystem` (циклы, энергия, логистика, склады, военный режим, снабжение); `EconomyWorkshopProductionSystem` остаётся как fallback-демо и отключается при активном full-runtime.
- `Packages/manifest.json` — зависимости (Entities, URP, Input System, Unity Physics).

## Сеть (спайк)

В `Packages/manifest.json` — `com.unity.netcode`; точка входа `ColonyNetcodeBootstrap` (`ClientServerBootstrap`), сценарий «go in game» — `GoInGameSpike.cs`. Синглтон `NetcodeSpikeState` создаётся в `SubsystemBootstrapUtility`; `NetcodeSpikeStateSyncSystem` выставляет `TransportReady`, когда в мире есть `NetworkStreamInGame`. Дальнейшая синхронизация геймплейных сущностей — отдельные задачи.
