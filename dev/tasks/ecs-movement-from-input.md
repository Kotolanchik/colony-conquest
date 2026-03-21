# Задача: `ecs-movement-from-input` — движение по InputCommandState.Move

## Цель

Система `PlayerMoveFromInputSystem` сдвигает все сущности с `PlayerMoveTargetTag` в плоскости XZ по вектору из синглтона `InputCommandState` (заполняется `InputGatherSystem`).

## Спецификации

- [x] `spec/technical_architecture_specification.md`
- [x] `spec/ui_ux_spec.md`
- [x] `spec/COLONY_CONQUEST_MASTER_SPECIFICATION.md`

## Связанные ADR

- нет / поведение прототипное

## Критерии готовности (Definition of Done)

- [x] `PlayerMoveFromInputSystem` в группе симуляции, `UpdateAfter(InputGatherSystem)`
- [x] Скорость как константа в системе (5 единиц/сек)
- [x] Канбан и README обновлены

## Риски и допущения

- Камера не следует за объектом — отдельная задача.

## Заметки / журнал

- 2026-03-21 — Реализовано.
