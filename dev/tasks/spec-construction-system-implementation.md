# Задача: `spec-construction-system-implementation` — Полная реализация `construction_system_spec` в коде

## Цель

Реализовать ECS runtime контура строительства по `spec/construction_system_spec.md`: очередь проектов, этапы строительства, формулу скорости из §5.2, приоритеты, списание ресурсов и ежедневный прогресс с публикацией событий и метрик.

## Спецификации

- [x] `spec/construction_system_spec.md` — §2.2, §3.1, §5.1–§5.2, §8.2, §9.1
- [x] `spec/COLONY_CONQUEST_MASTER_SPECIFICATION.md` — интеграция строительства с экономикой

## Связанные ADR

- нет

## Критерии готовности (Definition of Done)

- [x] ECS runtime: `ConstructionSimulationState`, буфер `ConstructionProjectEntry`, приоритеты и стадии (`ConstructionPriority`, `ConstructionStage`).
- [x] Формулы строительства: `ConstructionRuntimeMath` (включая `workers^0.7`, skill/tools/weather/night penalty).
- [x] Bootstrap: `ConstructionRuntimeBootstrapSystem` с демо-очередью проектов.
- [x] Суточная симуляция: `ConstructionRuntimeDailySystem`:
  - [x] commit материалов из `ResourceStockEntry`,
  - [x] продвижение стадий и прогресса,
  - [x] блокировки по нехватке ресурсов,
  - [x] завершение проектов и события.
- [x] Добавлен отсутствующий `ConstructionModeState`, чтобы существующий ghost-flow был консистентным.
- [x] Метрики `Construction*` добавлены и пишутся ежедневно.
- [x] README обновлён шагами проверки.

## Риски и допущения

- В текущем runtime использован агрегированный формат очереди (без тайловой сетки и pathfinding-логистики).
- Состав материалов упростён до `Wood/Stone/SteelIndustrial` для интеграции со складом.

## Заметки / журнал

- 2026-03-21 — добавлены `ConstructionRuntimeComponents`, `ConstructionRuntimeMath`, `ConstructionRuntimeBootstrapSystem`, `ConstructionRuntimeDailySystem`, `ConstructionModeState`.
