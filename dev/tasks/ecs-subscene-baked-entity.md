# Задача: `ecs-subscene-baked-entity` — SubScene: тестовая сущность с LocalTransform

## Цель

В `SubScenes/GameSubScene` есть объект с authoring, который после baking даёт сущность с `LocalTransform` и маркером `PlayerMoveTargetTag`.

## Спецификации

- [x] `spec/technical_architecture_specification.md`
- [x] `spec/COLONY_CONQUEST_MASTER_SPECIFICATION.md`

## Связанные ADR

- `docs/decisions/002-unity6-editor-baseline.md` (стек редактора)

## Критерии готовности (Definition of Done)

- [x] Authoring + Baker в `PlayerMoveTargetAuthoring.cs`; тег в `PlayerMoveTargetTag.cs`
- [x] Корневой объект `TestMoveTarget` в `GameSubScene.unity`
- [x] `Colony.Conquest.Core.asmdef` → `Unity.Entities.Hybrid` для Baker

## Риски и допущения

- После открытия в Unity может потребоваться пересчёт baking SubScene.

## Заметки / журнал

- 2026-03-21 — Реализовано вручную в репозитории (YAML сцены + скрипты).
