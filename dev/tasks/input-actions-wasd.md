# Задача: input-actions-wasd — Ввод WASD в ECS

**Связи в канбане:** см. `dev/kanban.json`.

---

## Цель

Подключить **Input System** с картой **Gameplay / Move (Vector2, WASD)** и записывать результат в синглтон **`InputCommandState`** для дальнейших систем (камера, юниты).

## Спецификации

- [x] `spec/ui_ux_spec.md` — ввод (общий контекст)

## Критерии готовности (Definition of Done)

- [x] `Colony.Conquest.Core` ссылается на сборку `Unity.InputSystem`
- [x] `Settings/ColonyInputActions.inputactions` и синхронная строка `InputActionsJson.ColonyJson`
- [x] `InputGatherSystem` (SystemBase) + `InputCommandState` (IComponentData)

## Заметки / журнал

- 2026-03-21 — Реализовано; при первом импорте Unity может создать `.meta` для `.inputactions`.
