# Задача: `domain-construction-ghost-stub` — Заготовка потока «режим строительства / призрак» (без полной сетки)

## Цель

В коде клиента зафиксированы типы и синглтон ECS для режима предпросмотра размещения постройки (призрак): идентификатор чертежа, сетка в клетках, якорь и флаги — без полной сетки мира и без финального UI.

## Спецификации

- [x] `spec/construction_system_spec.md` — этапы и размеры (землянка 2×2 как ориентир)
- [x] `spec/ui_ux_spec.md` — привязка к будущему UI режима строительства

## Связанные ADR

- нет

## Критерии готовности (Definition of Done)

- [x] `ConstructionBlueprintId`, `ConstructionGhostState`, `ConstructionGhostBootstrapSystem` (Initialization), `ConstructionBuildModeToggleSystem`, `ConstructionGhostCursorSystem`.
- [x] Проверка: Play на сцене Bootstrap — **Window → Entities → Hierarchy** — синглтон `ConstructionGhostState`; **B** включает режим, якорь двигается от игрока.

## Риски и допущения

- Рейкаст, привязка к ландшафту и полноценный UI — отдельные задачи.

## Заметки / журнал

- 2026-03-21 — Реализация в `Assets/_Project/Scripts/Construction/`.
- 2026-03-24 — `ConstructionBlueprintId`: эпоха 1 — `Cabin`, `House`, `Manor` (§2.2); `ConstructionZoneKindId` §3.1.
- 2026-03-24 — `ConstructionBlueprintFootprints.GetFootprintCells` → `ConstructionGhostState.FootprintCells` при активном режиме.
- 2026-03-21 — Эпоха 2 §2.2: `WorkerTenement` (4×6), `TownhouseEpoch2` (5×8); синхронизация следа в `ConstructionGhostCursorSystem` (`ConstructionSystems.cs`). Дубликат `ConstructionGhostCursorSystem.cs` удалён (двойной `OnUpdate` в partial).
- 2026-03-21 — Заготовка закрыта полной реализацией: `dev/tasks/spec-construction-system-implementation.md`.
