# Задача: `meta-events-pipeline-stub` — конвейер событий и квестов

## Цель

Закрепить типы событий и квестов из спеки и элемент буфера очереди для последующего AI Director.

## Спецификации

- [x] `spec/events_quests_spec.md`
- [x] `spec/COLONY_CONQUEST_MASTER_SPECIFICATION.md`

## Связанные ADR

- нет

## Критерии готовности (Definition of Done)

- [x] `StoryEventKind`, `QuestKind`, `GameEventQueueEntry` (`IBufferElementData`), синглтон с буфером в `SubsystemBootstrapUtility`.
- [x] `StoryEventPipelineSystem` на первом тике кладёт тестовое событие, затем снимает из очереди и пишет в `AnalyticsHooks`.
- [x] Проверка: Play — в синглтон-буфере `GameEventQueueEntry` кратковременно есть запись на первом кадре; после обработки очередь пуста.

## Риски и допущения

- AI Director и полный конвейер квестов — отдельные задачи.

## Заметки / журнал

- 2026-03-21 — `Assets/_Project/Scripts/Story/`.
- 2026-03-24 — `AiDirectorDimensionsState` (Wealth, Security, Stability, Progress, Tension 0–100 по §2.2), сидирование в `SubsystemBootstrapUtility`; логика расчёта осей — отдельная задача.
- 2026-03-24 — `AiDirectorDimensionsUpdateSystem` после `AnalyticsSnapshotUpdateSystem`: Wealth/Security/Stability/Progress из снимка; Tension из стабильности, загрязнения (`ColonyPollutionSummaryState` при наличии), бедности; формула tension уточнена (drama = |Stability−50|, без дублирования шкал).
- 2026-03-21 — `AiDirectorPolicyUpdateSystem` + `AiDirectorPolicyState`: пороги §2.3, триггер в очередь при смене политики (tick>1); `StoryEventPipelineSystem` после политики.
- 2026-03-21 — Заготовка superseded полной реализацией `dev/tasks/spec-events-quests-full-implementation.md`.
