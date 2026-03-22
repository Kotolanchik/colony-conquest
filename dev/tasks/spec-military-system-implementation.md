# Задача: `spec-military-system-implementation` — Полная реализация `military_system_specification` в ECS

## Цель

Реализовать полный runtime-контур военной системы по `spec/military_system_specification.md`: командная иерархия, передача приказов, боевой цикл (мораль/подавление/усталость/урон), медицинский контур, погодно-временные модификаторы, мета-юниты и интеграции с экономикой/производством/картой/аналитикой.

## Спецификации

- [x] `spec/military_system_specification.md` — §1, §2, §3, §4, §5, §6, §7, §8, §9, §10
- [x] `spec/COLONY_CONQUEST_MASTER_SPECIFICATION.md` — интеграция подсистем
- [x] `spec/economic_system_specification.md` — снабжение армии и военный бюджет
- [x] `spec/manufacturing_plants_spec.md` — подпитка резервов военным выпуском
- [x] `spec/global_map_spec.md` — связка со стратегическими армиями
- [x] `spec/statistics_analytics_spec.md` — военные метрики и snapshot

## Связанные ADR

- нет

## Критерии готовности (Definition of Done)

- [x] Добавлен полный набор military runtime-компонентов:
  - [x] `MilitarySimulationState`, `MilitaryEnvironmentState`, `MilitaryCommandRelayState`,
  - [x] буферы `MilitaryFormationEntry`, `MilitaryOperationOrderEntry`, `MilitaryMetaUnitEntry`,
  - [x] `MilitaryUnitRuntimeState`, `MilitaryCoverState`, `WoundedState`,
  - [x] enum-модели уровней командования, типов войск, погоды, ранений, posture/aid/meta-state.
- [x] Добавлен `MilitarySimulationMath`:
  - [x] задержка приказов и связь,
  - [x] мораль и паника,
  - [x] точность/пробитие/подавление,
  - [x] усталость и укрытия,
  - [x] типизация ранений и time-to-death,
  - [x] шаблоны статов юнитов.
- [x] Реализован `MilitarySimulationBootstrapSystem`:
  - [x] singleton и стартовые буферы,
  - [x] иерархия формаций и стартовые приказы,
  - [x] стартовый пул боевых сущностей с runtime-компонентами.
- [x] Реализован `MilitarySimulationDailySystem`:
  - [x] погодно-ночные модификаторы и command relay,
  - [x] прогон приказов/задержек/исполнения,
  - [x] daily combat attrition (morale/suppression/fatigue/readiness),
  - [x] расход ammo/fuel со склада и влияние supply adequacy,
  - [x] wounded/medical loop, потери и исходы боёв,
  - [x] построение `MilitaryMetaUnitEntry` (LOD4+).
- [x] Интеграции:
  - [x] `EconomyArmySupplyState` и `EconomySimulationState`,
  - [x] `ManufacturingSimulationState` (пополнение резерва),
  - [x] `StrategicArmyEntry` (fatigue/carrying supply),
  - [x] `DefensiveStructureRuntimeEntry` (cover bonus),
  - [x] `SettlerSimulationState` (колониальная мораль),
  - [x] `StoryEventQueue` (military события).
- [x] Аналитика:
  - [x] snapshot военного блока заполняется из `MilitarySimulationState`,
  - [x] добавлены `0x92nn` метрики runtime (`MilitaryAverageMorale01`, `MilitaryCombatReadiness01`, ...),
  - [x] суточная запись метрик через `AnalyticsHooks`.
- [x] README клиента обновлён шагами проверки military runtime.

## Риски и допущения

- Боевой цикл реализован как ежедневная агрегированная симуляция attrition, а не покадровый тактический бой.
- Мета-юниты LOD реализованы как runtime-агрегация дальних юнитов в буфер, без отдельного рендера.
- Глубокие детали штурма зданий/тоннелей и детерминированная баллистика остаются для отдельного тактического контура.

## Заметки / журнал

- 2026-03-21 — добавлены полный military runtime ECS-контур, интеграции с экономикой/производством/картой, военная аналитика и документация.
