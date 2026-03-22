# Задача: `spec-events-quests-full-implementation` — Полная реализация `events_quests_spec` в ECS

## Цель

Реализовать полноценный runtime-контур событий и квестов по `spec/events_quests_spec.md`: AI Director, каталог и выбор событий, active/cooldown/history цикл, процедурная генерация квестов, персональные истории и интеграции с ключевыми подсистемами.

## Спецификации

- [x] `spec/events_quests_spec.md` — §1, §2, §3, §4, §5, §6, §7
- [x] `spec/COLONY_CONQUEST_MASTER_SPECIFICATION.md` — интеграция контуров
- [x] `spec/statistics_analytics_spec.md` — метрики событий/квестов
- [x] `spec/settler_simulation_system_spec.md` — персональные истории и социальные эффекты
- [x] `spec/military_system_specification.md` — военные события/потери/напряжение
- [x] `spec/economic_system_specification.md` — экономические шоки и награды
- [x] `spec/technology_tree_spec.md` — технологические события и эпохальные условия

## Связанные ADR

- нет

## Критерии готовности (Definition of Done)

- [x] Добавлен full-runtime домен Story:
  - [x] `StorySimulationState` + буферы `StoryEventDefinitionEntry`, `StoryEventCooldownEntry`, `StoryActiveEventEntry`, `StoryEventHistoryEntry`,
  - [x] квесты `QuestRecordEntry`,
  - [x] персональные арки `PersonalStoryArcEntry`.
- [x] Добавлен `EventsQuestSimulationMath`:
  - [x] фильтр событий по эпохе/условиям,
  - [x] расчёт веса выбора по AI Director policy + tension,
  - [x] severity/сложность/награды квестов,
  - [x] прогресс квестов и архетипы персональных историй.
- [x] Добавлен `EventsQuestBootstrapSystem`:
  - [x] каталог событий по категориям (природные/военные/социальные/экономические/технологические/глобальные),
  - [x] буферы cooldown/active/history/quests/arcs.
- [x] Добавлен `EventsQuestDailySystem`:
  - [x] импорт external событий из `GameEventQueueEntry`,
  - [x] AI Director weighted выбор и запуск событий,
  - [x] immediate/ongoing эффекты по категориям,
  - [x] процедурная генерация и суточный прогресс квестов,
  - [x] персональные арки и story beats.
- [x] Интеграции:
  - [x] Settlers/Military/Economy/Technology/Ecology/WorldMap/Diplomacy состояния читаются/модифицируются по эффектам событий,
  - [x] `StoryEventPipelineSystem` переведён в fallback-режим при активном full-story runtime.
- [x] Аналитика:
  - [x] добавлены метрики `Events*`, `Quests*`, `StoryArc*`, `StoryTension01` (`0x93nn`),
  - [x] daily запись метрик через `AnalyticsHooks`.
- [x] README клиента обновлён шагами проверки.

## Риски и допущения

- Детальные сюжетные ветвления (UI-диалоги/мультивыбор игрока) представлены агрегированными runtime-ветками.
- Процедурные квесты реализованы шаблонным генератором без отдельного контент-пайплайна локализации.
- Исторические арки эпох заданы как runtime-каталог дефиниций, а не жёстко скриптованные кампании.

## Заметки / журнал

- 2026-03-21 — добавлены `EventsQuestSimulationComponents`, `EventsQuestSimulationMath`, `EventsQuestBootstrapSystem`, `EventsQuestDailySystem`, новые метрики и документация.
