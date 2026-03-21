# Задача: `domain-technology-tree-ids-stub` — Идентификаторы эпох и веток дерева технологий

## Цель

В коде зафиксированы перечисления эпох (5 ступеней) и веток развития из `spec/technology_tree_spec.md` для каталогов технологий, сохранений и UI без реализации очков исследования.

## Спецификации

- [x] `spec/technology_tree_spec.md` — §1.2 эпохи, §1.3 ветки
- [x] `spec/COLONY_CONQUEST_MASTER_SPECIFICATION.md` — интеграция прогрессии

## Связанные ADR

- нет

## Критерии готовности (Definition of Done)

- [x] `ColonyConquest.Technology.TechEraId` — значения 1–5 с именами, согласованными со спекой.
- [x] `ColonyConquest.Technology.TechBranchId` — военная, экономическая, научная, социальная, инфраструктурная.
- [x] Проверка: поиск по проекту `TechEraId` / `TechBranchId` — XML-комментарии ссылаются на разделы спеки.

## Заметки / журнал

- 2026-03-21 — `Assets/_Project/Scripts/Technology/`.
- 2026-03-24 — `TechResearchTuning` (модификатор эпохи §2.2), `ColonyTechResearchDailySystem`, поля в `ColonyTechProgressState`, миграция `LastResearchDayIndex` для старых суток без взрыва очков.
- 2026-03-21 — Заготовка расширена до полной реализации в задаче `spec-technology-tree-implementation`.
