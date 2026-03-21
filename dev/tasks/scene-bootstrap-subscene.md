# Задача: scene-bootstrap-subscene — Сцена Bootstrap + SubScene

**Связи в канбане:** см. `dev/kanban.json`.

---

## Цель

Иметь стартовую сцену с камерой, светом, компонентом **SubScene** (DOTS) и пустой сценой `GameSubScene`; сцена добавлена в **Build Settings**.

## Спецификации

- [x] `spec/technical_architecture_specification.md` — ECS / сцены

## Критерии готовности (Definition of Done)

- [x] `Assets/_Project/Scenes/Bootstrap.unity` — Main Camera, Directional Light, объект с SubScene на `GameSubScene.unity`
- [x] `Assets/_Project/Scenes/SubScenes/GameSubScene.unity` — пустая подсцена (без внешних префабов сэмпла)
- [x] `ProjectSettings/EditorBuildSettings.asset` — Bootstrap в списке сцен
- [x] `.meta` с GUID для сцен и папок

## Заметки / журнал

- 2026-03-21 — Собрано из обрезанного шаблона EntitiesSamples; префаб Warrior удалён. При первом открытии в Unity может пересчитаться baking SubScene.
