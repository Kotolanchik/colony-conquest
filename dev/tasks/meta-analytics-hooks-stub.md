# Задача: `meta-analytics-hooks-stub` — точки телеметрии

## Цель

Ввести области аналитики по `spec/statistics_analytics_spec.md` §1.2 и заглушки записи метрик.

## Спецификации

- [x] `spec/statistics_analytics_spec.md`

## Связанные ADR

- нет

## Критерии готовности (Definition of Done)

- [x] `AnalyticsDomain`, `AnalyticsRecordEntry` в буфере синглтона, `AnalyticsHooks.Record` / `RecordCounter` пишут в буфер (кап 1024).
- [x] Проверка: Play — после первого кадра симуляции в буфере есть записи от `StoryEventPipelineSystem` и при нажатии **B** от строительства.

## Риски и допущения

- Схема metricId и интеграция с бэкендом — отдельная задача.

## Заметки / журнал

- 2026-03-21 — `Assets/_Project/Scripts/Analytics/AnalyticsHooks.cs`.
