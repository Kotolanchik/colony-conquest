# Задача: feat-ecs-simulation-root — Корень симуляции ECS (singleton + тик)

**Связи в канбане:** см. `dev/kanban.json`.

---

## Цель

Иметь минимальный игровой контур: одно глобальное состояние симуляции (`SimulationRootState`) и обновление счётчика кадров в `GameBootstrapSystem`.

## Спецификации

- [x] `spec/technical_architecture_specification.md` — ECS / World (общий контекст)

## Связанные ADR

- нет

## Критерии готовности (Definition of Done)

- [x] Синглтон создаётся при старте мира
- [x] `SimulationTick` увеличивается в `OnUpdate`
- [x] `game/Client/README.md` отражает поведение

## Заметки / журнал

- 2026-03-21 — Реализовано в `SimulationRootState.cs` + обновлён `GameBootstrapSystem.cs`.
