# Задача: `feat-presentation-bridge-scaffold` — Каркас presentation-пайплайна (контракты + каталоги + папки контента)

## Цель

Подготовить базовый production-ready каркас для визуального контента (3D/иконки/VFX), не ломая ECS-симуляцию:
структуру папок ассетов, ScriptableObject-каталоги и bridge-контракты между gameplay и presentation.

## Спецификации

- [x] `spec/ui_ux_spec.md`
- [x] `spec/audio_design_spec.md`
- [x] `spec/construction_system_spec.md`
- [x] `spec/military_system_specification.md`
- [x] `spec/COLONY_CONQUEST_MASTER_SPECIFICATION.md`

## Связанные ADR

- нет

## Критерии готовности (Definition of Done)

- [x] Добавлены папки контента `Art/Units`, `Art/Buildings`, `Art/UI/Icons`, `Art/VFX`.
- [x] Добавлены каталоги SO: `UnitVisualCatalog`, `BuildingVisualCatalog`, `UiIconCatalog`, `VfxCatalog`.
- [x] Добавлены ECS bridge-контракты и очередь запросов (`PresentationBridge*`).
- [x] Добавлен bootstrap и drain для базовой обработки/очистки запросов.
- [x] Добавлен API `PresentationBridgeBus` для enqueue из gameplay-систем.
- [x] Пример интеграции: `ConstructionBuildModeToggleSystem` публикует icon/vfx-запросы.
- [x] README обновлён инструкцией «где создавать контент и как подключать».

## Риски и допущения

- Каркас не включает финальные художественные ассеты; это отдельный content-поток.
- Для полноценного рендера bridge нужно расширить до Mono/Presentation-рантайма, который резолвит каталоги в prefab/sprite/vfx.

## Заметки / журнал

- 2026-03-21 — базовый каркас добавлен; следующий шаг вынесен в ready-карточки content-phase1.
