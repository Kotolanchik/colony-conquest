# Задача: `spec-audio-design-full-implementation` — Полная реализация `audio_design_spec` в ECS

## Цель

Реализовать полный runtime-контур аудио по `spec/audio_design_spec.md`: adaptive music, ingest SFX-событий, 3D-параметры и бюджет производительности аудио.

## Спецификации

- [x] `spec/audio_design_spec.md` — §1, §2, §3, §4, §5, §6, §7
- [x] `spec/COLONY_CONQUEST_MASTER_SPECIFICATION.md` — интеграция аудио с доменными системами
- [x] `spec/statistics_analytics_spec.md` — метрики аудио
- [x] `spec/ui_ux_spec.md` — интерфейсные сигналы и accessibility settings
- [x] `spec/military_system_specification.md` — боевая интенсивность и погодные модификаторы
- [x] `spec/global_map_spec.md` — биомный профиль окружения
- [x] `spec/technology_tree_spec.md` — тематика музыки по эпохам

## Связанные ADR

- нет

## Критерии готовности (Definition of Done)

- [x] Добавлен full-runtime домен Audio:
  - [x] `AudioSimulationState` + буферы `AudioActiveEmitterEntry`, `AudioMusicTransitionEntry`.
- [x] Добавлен `AudioSimulationMath`:
  - [x] тематический профиль по эпохам,
  - [x] adaptive music intensity/level/crossfade,
  - [x] SFX priority/lifetime, 3D attenuation/occlusion/reverb,
  - [x] оценки latency/memory.
- [x] Добавлен `AudioSimulationBootstrapSystem`:
  - [x] singleton с бюджетами `64` голосов / `32` 3D-источника.
- [x] Добавлен `AudioSimulationRuntimeSystem`:
  - [x] ingest `AudioBusPendingEntry` в runtime-эмиттеры,
  - [x] приоритизация/дроп низкоприоритетных событий при переполнении,
  - [x] адаптивная музыка по бою/кризису/времени суток/масштабу камеры,
  - [x] синхронизация с UI sound-signals и weather/biome контекстом.
- [x] Legacy `AudioBusDrainSystem` переведён в fallback-режим при активном full-audio runtime.
- [x] Аналитика:
  - [x] добавлены метрики `Audio*` (`0x95nn`),
  - [x] daily запись через `AnalyticsHooks`.
- [x] README клиента обновлён шагами проверки.

## Риски и допущения

- FMOD/Wwise интеграция остаётся внешним слоем; текущая реализация задаёт ECS-контур принятия решений и маршрутизации логических аудио-событий.
- Голоса/диалоги и локализационные пакеты представлены агрегированным runtime-state, без отдельных content-banks.
- Реалистичная геометрическая окклюзия (raycast по препятствиям) заменена lightweight-моделью для стабильного perf.

## Заметки / журнал

- 2026-03-21 — добавлены `AudioSimulationComponents`, `AudioSimulationMath`, `AudioSimulationBootstrapSystem`, `AudioSimulationRuntimeSystem`; `AudioBusDrainSystem` переведён в fallback; добавлены `Audio*` метрики.
