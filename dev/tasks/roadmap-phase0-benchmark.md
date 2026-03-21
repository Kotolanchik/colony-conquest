# Задача: `roadmap-phase0-benchmark` — замер ECS фазы 0 (§6.1)

## Цель

Воспроизводимый замер нагрузки на ECS в духе дорожной карты: ориентир **1000 сущностей / 60 FPS** (`spec/COLONY_CONQUEST_MASTER_SPECIFICATION.md` §6.1).

## Спецификации

- [x] `spec/COLONY_CONQUEST_MASTER_SPECIFICATION.md`
- [x] `spec/technical_architecture_specification.md`

## Связанные ADR

- `docs/decisions/002-unity6-editor-baseline.md` (стек клиента)

## Критерии готовности (Definition of Done)

- [x] Включение замера явное: `BenchmarkPhase0Tuning.Enabled` (по умолчанию выключено).
- [x] Спавн сетки сущностей с `LocalTransform` + `BenchmarkDriftTag`; синтетическая нагрузка в `BenchmarkDriftSystem`.
- [x] Отчёт в консоль с интервалом; кэшированный `EntityQuery` в `BenchmarkReportSystem`.
- [x] Описание в `game/Client/README.md`.

## Риски и допущения

- Показатель консоли — ориентир, не замена Profiler; целевая платформа и сцена влияют на FPS.

## Заметки / журнал

- 2026-03-22 — Реализовано в `Colony.Conquest.Core`.
