# Задача: `spec-religion-cults-implementation` — Полная реализация `religion_cults_spec` в коде

## Цель

Закрыть базовую реализацию религии/культов из `spec/religion_cults_spec.md`: сводные состояния веры, формулы динамики веры и напряжения, конверсия, радикализация культов, машина состояний священной войны, события и метрики.

## Спецификации

- [x] `spec/religion_cults_spec.md` — §4, §5, §7–§11
- [x] `spec/events_quests_spec.md` — публикация событий в общий пайплайн
- [x] `spec/COLONY_CONQUEST_MASTER_SPECIFICATION.md` — интеграционные связи с дипломатией/социумом

## Связанные ADR

- нет

## Критерии готовности (Definition of Done)

- [x] ECS-модель религии: `ReligionSimulationState`, `ReligiousConflictState`, `CultActivityState`, `HolyWarState`, `ReligionFactionPolicyState`.
- [x] Bootstrap: `ReligionBootstrapSystem` создаёт необходимые синглтоны.
- [x] `ReligionDailySimulationSystem`:
  - [x] формула суточного изменения веры;
  - [x] модель вероятности конверсии;
  - [x] расчёт `TensionScore`;
  - [x] обновление риска радикализации культов;
  - [x] state machine священной войны (`TriggerDetected` → `Resolution`).
- [x] Публикация религиозных событий в `GameEventQueueEntry`.
- [x] Метрики `Religion*` добавлены в `AnalyticsMetricIds` и пишутся ежедневно.
- [x] Документация проверки добавлена в `game/Client/README.md`.

## Риски и допущения

- Модель реализована на уровне агрегатов колонии (без поселенца-агента на сущность).
- Дипломатические последствия войны сведены к событиям и метрикам; территориальные изменения — отдельная задача военной/дипломатической подсистемы.

## Заметки / журнал

- 2026-03-21 — реализованы компоненты, bootstrap и daily-система религии; подключены аналитика и очередь событий.
