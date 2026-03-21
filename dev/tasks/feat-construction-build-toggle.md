# Задача: `feat-construction-build-toggle` — Ввод ToggleBuild и переключение режима призрака

## Цель

Действие **ToggleBuild** (B) в `ColonyInputActions` и в `InputActionsJson` синхронно с ассетом; в кадре симуляции `InputCommandState.ToggleBuildPressed` отражает нажатие; `ConstructionBuildModeToggleSystem` переключает `ConstructionGhostState.Active`, при первом включении выставляет `ConstructionBlueprintId.EarthHut`, вызывает `AudioBusStub` и `AnalyticsHooks`. `ConstructionGhostCursorSystem` обновляет `AnchorWorld` от позиции игрока.

## Спецификации

- [x] `spec/ui_ux_spec.md` — ввод
- [x] `spec/construction_system_spec.md` — режим строительства (контекст)

## Связанные ADR

- нет

## Критерии готовности (Definition of Done)

- [x] `Settings/ColonyInputActions.inputactions` и `InputActionsJson.ColonyJson` содержат **ToggleBuild** на `<Keyboard>/b`.
- [x] `ConstructionBuildModeToggleSystem` после `InputGatherSystem`; `ConstructionGhostCursorSystem` после `PlayerMoveFromInputSystem`.
- [x] Проверка: Play на **Bootstrap**, нажать **B** — в Entities Hierarchy синглтон `ConstructionGhostState`: `Active` 0/1, при активном режиме `AnchorWorld` следует за направлением WASD относительно `TestMoveTarget`.

## Заметки / журнал

- 2026-03-21 — Единая система переключения: `ConstructionBuildModeToggleSystem` + аудио/аналитика; курсор — `ConstructionGhostCursorSystem`.
- 2026-03-22 — Bootstrap `ConstructionBlueprintId.None`; при первом входе в режим выставляется `EarthHut`.
