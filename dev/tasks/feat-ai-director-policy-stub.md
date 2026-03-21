# Задача: `feat-ai-director-policy-stub` — политика AI Director (§2.3)

## Цель

Зафиксировать решение о «необходимости» события по порогам измерений (шаг 2 `spec/events_quests_spec.md` §2.3) и связать его с очередью `GameEventQueueEntry`.

## Спецификации

- [x] `spec/events_quests_spec.md` — §2.3
- [x] `spec/COLONY_CONQUEST_MASTER_SPECIFICATION.md`

## Связанные ADR

- нет

## Критерии готовности (Definition of Done)

- [x] `AiDirectorPolicyKind`, `AiDirectorPolicyState` (синглтон).
- [x] `AiDirectorPolicyUpdateSystem` после `AiDirectorDimensionsUpdateSystem`; при смене политики — запись в буфер `GameEventQueueEntry` (`StoryEventKind.Triggered`, тик &gt; 1, чтобы не мешать bootstrap-событию).
- [x] `StoryEventPipelineSystem` выполняется после `AiDirectorPolicyUpdateSystem`.
- [x] Bootstrap в `SubsystemBootstrapUtility` + миграция синглтона при отсутствии.
- [x] Проверка: Play — после изменения измерений (например, искусственно занизить `Stability` в инспекторе на тике &gt; 1) в буфере появляется триггер с `EventDefinitionId` = вид политики; `StoryEventPipelineSystem` снимает запись и шлёт метрику.

## Заметки / журнал

- 2026-03-21 — `Assets/_Project/Scripts/Story/AiDirectorPolicyState.cs`, `Assets/_Project/Scripts/AiDirectorPolicyUpdateSystem.cs`.
