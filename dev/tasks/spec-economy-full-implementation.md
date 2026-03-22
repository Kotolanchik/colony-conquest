# Задача: `spec-economy-full-implementation` — Полная реализация `economic_system_specification` в коде

## Цель

Довести экономику до полного runtime-контура по `spec/economic_system_specification.md`: производство, энергия, логистика, склады, военные режимы, снабжение армии, циклы экономики и интеграция с аналитикой/исследованиями.

## Спецификации

- [x] `spec/economic_system_specification.md` — §2.1–§2.6, §3, §4, §5, §6, §7, §8, §9.2, §10.2–§10.5
- [x] `spec/COLONY_CONQUEST_MASTER_SPECIFICATION.md` — интеграция экономики с военной и технологической подсистемами
- [x] `spec/statistics_analytics_spec.md` — запись экономических метрик

## Связанные ADR

- нет

## Критерии готовности (Definition of Done)

- [x] Введён полный singleton экономики: `EconomySimulationState` + состояния энергии, логистики, складов, военного производства, снабжения.
- [x] Реализованы буферы runtime-модели:
  - [x] `EconomyProductionFacilityEntry`,
  - [x] `EconomyPowerGeneratorEntry`,
  - [x] `EconomyTransportRouteEntry`,
  - [x] `EconomyWarehouseEntry`.
- [x] Добавлены формулы в `EconomySimulationMath`:
  - [x] эффективность здания (§3.3),
  - [x] логистическая эффективность (§5.3, §10.3.5),
  - [x] потери передачи энергии (§4.2),
  - [x] обработка склада (§6.4),
  - [x] исследовательские очки от экономики (§9.2).
- [x] Добавлен bootstrap полной экономики `EconomySimulationBootstrapSystem` (демо-площадки, генераторы, маршруты, склады).
- [x] Добавлена суточная full-система `EconomySimulationDailySystem`:
  - [x] экономические фазы (§10.2),
  - [x] энергобаланс и накопители (§4),
  - [x] производство по рецептам с multipliers (workers/energy/wear/skill/master/upgrade),
  - [x] логистическая и складская обработка (§5, §6),
  - [x] режимы/приоритеты военного производства (§7.1–§7.3),
  - [x] снабжение армии и adequacy (§8),
  - [x] детектор bottleneck (§10.5).
- [x] `EconomyWorkshopProductionSystem` переведён в fallback-режим (отключается при наличии полного экономического контура).
- [x] Расширены `AnalyticsMetricIds` метриками `Economy*` (runtime) и включена запись в `AnalyticsHooks`.
- [x] `AnalyticsSnapshotUpdateSystem` синхронизирует инфляцию/безработицу/внешнюю торговлю из `EconomySimulationState`.
- [x] README обновлён шагами проверки полной экономики.

## Риски и допущения

- Логистика/склады/энергосеть реализованы агрегированно (без геометрии сети и тайлового pathfinding).
- Военное снабжение реализовано через ресурсные пулы существующего stockpile без отдельного UI цепочек конвоев.
- Полный контент Эпохи 5 описан через агрегированные мощности/режимы и существующий набор `ResourceId`.

## Заметки / журнал

- 2026-03-21 — добавлены `EconomySimulationComponents`, `EconomySimulationMath`, `EconomySimulationBootstrapSystem`, `EconomySimulationDailySystem`; расширены метрики и analytics bridge.
