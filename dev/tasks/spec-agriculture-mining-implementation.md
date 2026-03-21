# Задача: `spec-agriculture-mining-implementation` — Реализация `spec/agriculture_mining_spec.md` в коде

## Цель

Зафиксировать в клиенте **данные и формулы** из `spec/agriculture_mining_spec.md`, **игровой контур** ручной добычи (§2.2–2.3) и **ECS-контур** фермерства и животноводства: этапы §1.2, сбор по §1.3 (сезонность через `GameCalendarState` + `SeasonCycle`), суточная продукция §1.4, интеграция со складом через `ResourceId` 56–67 и `ResourceCatalog`, демо-сущности из `SubsystemBootstrapUtility`.

## Спецификации

- [x] `spec/agriculture_mining_spec.md` — §1.2–1.5, §2, §3 (интеграция — таблица, без отдельного кода)

## Критерии готовности (Definition of Done)

### §1 Фермерство

- [x] §1.3 — `CropYieldFormulas.ComputeYield` (все множители из формулы); сезонность §1.2 — `CropYieldMath.SeasonalModifier` (+10% / −50%) + `CropCatalog` / `SeasonCycle`; вода — `WaterSupplyKind` + `CropWaterTuning`; сорняки — отдельный множитель в формуле; пищевая ценность §1.1 — `CropDefinition.NutritionMultiplier` / `CropCatalog.GetNutritionMultiplier`.
- [x] §1.2 — `CropGrowthPhase`, длительности в `CropGrowthTuning`, симуляция `CropGrowthSimulationSystem` (`CropPlotRuntime` + `CropPlotTag`); **ежедневный уход в фазе роста** — `CropCareDailySystem` (вредители, сорняки, вклад агрохимии §1.5).
- [x] §1.5 — `FertilizerKindId`, `FertilizerTuning.GetYieldBonus`; `FertilizerEcologyTuning` → `ColonyAgrochemicalLoadState.ChemicalLoad01` → `AgrochemicalEcologyBridgeSystem` → `ColonyEcologyIndicatorsState` (§ecology_spec индикаторы 0…1); `AnalyticsSnapshotUpdateSystem` подставляет среднее в `Social.Ecology01`.
- [x] §1.1 — культуры эпохи 1 в `CropKindId` и `CropCatalog` (расширение эпох 2+ — отдельная карточка при контенте).
- [x] §1.4 — `LivestockDailyYield`, `LivestockDailyProductionSystem`, демо-загоны `LivestockPenTag` + `LivestockPenRuntime` (куры, козы, овцы, свиньи — первичные ресурсы из `LivestockDailyYield`).
- [x] Authoring в SubScene: `CropPlotAuthoring` + `CropPlotBaker`.

### §2 Добыча

- [x] §2.2 ручная — `MiningPickaxeTierId`, таблицы в `MiningManualFormulas` (ед/час, износ), `ManualMiningToolState`, навык и усталость.
- [x] §2.2 промышленная — `IndustrialMiningMethodId`, `IndustrialMiningFormulas.GetNominalOutputPerGameHour`, `IndustrialMiningProductionSystem` (склад по номиналу и числу рабочих), демо-карьер в `SubsystemBootstrapUtility`.
- [x] §2.3 — `MiningDepositRuntime.InitialAmount`, качество руды по доле добытого (три полосы 20% / 50% / 30% и средние содержания §2.3 — см. XML у `MiningManualFormulas.GetOreContentAverageMultiplier`), мультипликаторы исчерпания по остатку, закрытие узла при нуле; **лес** — `MiningForestRegenerationSystem` +1% запаса в игровой год до `InitialAmount` (`MiningRegenerationTuning`); **рыба / дичь** — `WildRenewableStockState` + доли роста в год (`MiningRegenerationTuning.Fish*` / `WildGame*`) + `WildRenewableGatherSystem` (Interact, приоритет узлов добычи), ресурс `FishCatch`.
- [x] §2.4 — `MiningHazardTuning`, `MiningHazardDailySystem` (вероятности §2.4; уран — дополнительный `RadiationExposureDaily`; учёт в аналитике; детальные травмы — вне объёма).
- [x] Ввод: действие **Interact** (E), удержание для добычи; `ManualMiningGatherSystem` выбирает ближайший узел в радиусе.

### Проверка

- [x] Play: подойти к демо-лесу `(3,0,0)`, удерживать **E** — на складе растёт `Wood`, уменьшается `AmountRemaining` и `DurabilityRemaining` инструмента; у узла задан `InitialAmount` для мультипликатора исчерпания §2.3. Демо **железной руды** `(6,0,2)` — проверка качества руды §2.3.
- [x] После **365** игровых дней (`DayIndex`) — у леса `AmountRemaining` растёт на 1% от капа (если был ниже); у `WildRenewableStockState` — рост рыбы (10% от капа) и дичи (5% от капа) в год.
- [x] Play: демо **OpenQuarry** — на складе растёт `Stone` без ручного ввода (пока есть рабочие на узле).
- [x] Play: демо-узлы **глины** `(-4,0,2)` и **песка** `(-4,0,-2)` — удерживать **E**, на склад попадают `RawClay` / `Sand`.
- [x] Play: демо **каменный карьер** `(9,0,-2)` — удерживать **E**, на склад идёт `Stone` (без полос качества руды §2.3).
- [x] Play: точка **рыбалки** `(0,0,-6)` / **охоты** `(-9,0,0)` — удерживать **E** вне радиуса любого узла добычи — на склад `FishCatch` / `LivestockMeat`, расход биомассы из `WildRenewableStockState`.
- [x] Play: демо-загоны **овец** и **свиней** — суточный выход `LivestockWool` / `LivestockMeat` по `LivestockDailyYield`.
- [x] Play: демо-грядка создаётся в bootstrap — после прохождения этапов до `Harvest` в буфер склада попадает `CropWheat` (формула §1.3).
- [x] Play: демо-загон кур — при смене `GameCalendarState.DayIndex` на новый день на склад добавляется `LivestockEggs` (§1.4).
- [x] Сборка Unity: проект `Colony.Conquest.Core` без ошибок компиляции.

## Заметки / журнал

- 2026-03-23 — Восстановление **леса** §2.3: `MiningForestRegenerationSystem`, `MiningWorldRegenerationState`. **Рыба/дичь** — `WildRenewableGatherSystem` + `FishCatch` в экономике; биомасса в `WildRenewableStockState` (рост в год). Демо: карьер `OpenQuarry` → склад, узлы глины/песка, ручной каменный карьер, точки рыбалки/охоты, овцы/свиньи. Уран §2.4 — доп. риск облучения у узла.
- 2026-03-21 — Уточнена документация полос качества руды §2.3 в `MiningManualFormulas` (ссылка на доли выработки и средние содержания).
- 2026-03-22 — `ManualMiningToolState` / `MiningHazardProcessState` добавляются в `EnsureAnalyticsAndSimulationSingletons` для существующих миров; `MiningHazardConstants` объединён с `MiningHazardTuning`.
- Промышленные методы — только номинальная производительность; здания и рабочие — отдельные задачи.
- 2026-03-22 — Динамическая погода, оросители, полноценные вредители/поля по карте — вне объёма; демо использует фиксированные множители на `CropPlotRuntime`.
