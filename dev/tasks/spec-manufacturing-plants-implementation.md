# Задача: `spec-manufacturing-plants-implementation` — Полная реализация `manufacturing_plants_spec` в коде

## Цель

Реализовать полноценный runtime-контур производственных заводов по `spec/manufacturing_plants_spec.md`: военное/гражданское/тяжёлое производство, очереди заказов, режим «мечи или плуги», интеграции с ресурсами, энергией, эпохами технологий и рабочей силой.

## Спецификации

- [x] `spec/manufacturing_plants_spec.md` — §1, §2.1–§2.4, §3.1–§3.4, §4.1–§4.2, §5, §6
- [x] `spec/COLONY_CONQUEST_MASTER_SPECIFICATION.md` — интеграция подсистем
- [x] `spec/economic_system_specification.md` — склады/ресурсы/энергия/производственный контур
- [x] `spec/technology_tree_spec.md` — эпохальные ограничения производства
- [x] `spec/settler_simulation_system_spec.md` — рабочая сила и влияние эффективности труда
- [x] `spec/statistics_analytics_spec.md` — метрики `Manufacturing*`

## Связанные ADR

- нет

## Критерии готовности (Definition of Done)

- [x] Добавлена runtime-модель manufacturing:
  - [x] `ManufacturingSimulationState`,
  - [x] `ManufacturingPlantRuntimeEntry`,
  - [x] `ManufacturingProductionOrderEntry`,
  - [x] `ManufacturingProductStockEntry`,
  - [x] enum-модели категорий/заводов/продукции/приоритетов/политики.
- [x] Добавлены формулы `ManufacturingSimulationMath`:
  - [x] policy-мультипликаторы мир/частичная/тотальная/экономия,
  - [x] штраф переоснастки 7 дней,
  - [x] эффективность завода (workers/automation/condition/energy/workerQuality),
  - [x] дефиниции продуктов по таблицам спеки (военные/гражданские/тяжёлые).
- [x] Реализован bootstrap `ManufacturingSimulationBootstrapSystem` с демо-заводами и стартовыми заказами.
- [x] Реализован daily-контур `ManufacturingSimulationDailySystem`:
  - [x] обработка очереди заказов,
  - [x] списание входных ресурсов и выпуск продукции,
  - [x] era-блокировки по технологиям,
  - [x] ресурсные блокировки,
  - [x] события `manufacturing-*`,
  - [x] политика «мечи или плуги» с retooling penalty.
- [x] Интеграции:
  - [x] `ResourceStockEntry` (входы/выходы),
  - [x] `EconomyEnergyState` (энергодефицит),
  - [x] `EconomyMilitaryIndustryState`/`EconomySimulationState` (агрегаты military share/output),
  - [x] `ColonyTechProgressState` (эпохи),
  - [x] `SettlerSimulationState` (качество рабочей силы).
- [x] Добавлены метрики `Manufacturing*` в `AnalyticsMetricIds` и запись в `AnalyticsHooks`.
- [x] README клиента обновлён шагами проверки manufacturing runtime.

## Риски и допущения

- Продукция, не имеющая прямого `ResourceId` (например, танки/дроны/мебель/электроника), хранится в `ManufacturingProductStockEntry` как виртуальный выпуск.
- Цепочки глубокой поузловой сборки (корпус/башня/двигатель) агрегированы в `ManufacturingProductDefinition` с часовыми нормами.
- Визуальный/интерактивный UI конвейеров и конфиг заказов игроком — отдельная задача.

## Заметки / журнал

- 2026-03-21 — добавлены `ManufacturingSimulationComponents`, `ManufacturingSimulationMath`, `ManufacturingSimulationBootstrapSystem`, `ManufacturingSimulationDailySystem`, аналитика и документация.
