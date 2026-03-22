# Задача: `feat-presentation-runtime-resolver` — Runtime resolver для prefab/icon/vfx по bridge-запросам

## Цель

Перевести presentation bridge из режима "очередь + drain" в рабочий runtime-резолв:
юниты/здания спавнятся из каталогов, иконки резолвятся по id, VFX запускаются через pool.

## Спецификации

- [x] `spec/ui_ux_spec.md`
- [x] `spec/audio_design_spec.md`
- [x] `spec/military_system_specification.md`
- [x] `spec/construction_system_spec.md`
- [x] `spec/COLONY_CONQUEST_MASTER_SPECIFICATION.md`

## Связанные ADR

- нет

## Критерии готовности (Definition of Done)

- [x] Добавлен `PresentationRuntimeResolverService` (Mono) с ссылками на каталоги.
- [x] Добавлен `PresentationBridgeResolveSystem` (ECS) для обработки request buffers через resolver service.
- [x] Добавлен пуллинг VFX и TTL-возврат в пул.
- [x] Добавлен icon feed runtime-резолв (`iconId -> Sprite`) через `UiIconCatalog`.
- [x] `PresentationBridgeDrainSystem` работает как fallback только при отсутствии resolver service.
- [x] Обновлён README с инструкцией подключения resolver в сцене.

## Риски и допущения

- Сервис требует назначения каталогов в сцене; без этого запросы будут дропаться (и считаться fallback-логикой).
- Для production нужно дополнительно добавить полноценный UI-рендер icon feed и контроль очистки stale unit/building instances.

## Заметки / журнал

- 2026-03-21 — реализован runtime resolver + bridge system + fallback схема.
