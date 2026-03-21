# Задача: init-unity-dots — Инициализация Unity + DOTS в game/Client

**Связи в канбане:** `task_file`, `spec_refs`, `adr_refs` — см. `dev/kanban.json`.

---

## Цель

Иметь открываемый в Unity Hub проект с DOTS (Entities), URP, Input System и Unity Physics, с минимальной ECS-системой подтверждения загрузки.

## Спецификации

- [x] `spec/technical_architecture_specification.md` — стек Unity + DOTS (версия редактора зафиксирована в ADR-002)

## Связанные ADR

- `docs/decisions/002-unity6-editor-baseline.md`

## Критерии готовности (Definition of Done)

- [x] `game/Client` содержит `Packages/manifest.json`, `ProjectSettings/`, `Assets/_Project/`
- [x] Сборка asmdef `Colony.Conquest.Core` с системой `GameBootstrapSystem`
- [x] `ProjectSettings/ProjectVersion.txt` задаёт целевой Editor
- [x] После первого открытия в Hub: разрешение пакетов, Play Mode → лог bootstrap
- [x] Индекс спеков не требовался (спеки не менялись по сути; при правке тех. спеки — `python tools/spec_index.py`)

## Риски и допущения

- Целевая версия редактора — **Unity 6** (см. ADR-002), не 2023.2 LTS из оригинального текста спеки.

## Заметки / журнал

- 2026-03-21 — Каркас основан на официальном EntitiesSamples (GitHub); добавлены Input System, Physics, собственный код.
