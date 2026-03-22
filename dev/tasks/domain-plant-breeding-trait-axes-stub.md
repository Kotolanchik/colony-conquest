# Задача: `domain-plant-breeding-trait-axes-stub` — Оси характеристик селекции растений

## Цель

В коде зафиксированы семь осей характеристик из таблицы §1.2 `spec/plant_breeding_spec.md` для будущих структур генотипа и скрещивания без симуляции мутаций.

## Спецификации

- [x] `spec/plant_breeding_spec.md` — §1.2
- [x] `spec/COLONY_CONQUEST_MASTER_SPECIFICATION.md` — агро/селекция в общей картине

## Связанные ADR

- нет

## Критерии готовности (Definition of Done)

- [x] `ColonyConquest.PlantBreeding.PlantTraitAxisId` — урожайность, скорость роста, засуха, холод, вредители, пищевая ценность, вкус (None + 7 осей).
- [x] Проверка: имена и порядок соответствуют строкам таблицы §1.2.

## Заметки / журнал

- 2026-03-21 — `Assets/_Project/Scripts/PlantBreeding/PlantTraitAxisId.cs`.
- 2026-03-24 — `PlantTraitAxisTuning` — диапазоны §1.2 (мин/макс % по осям), `NormalizeTo01` для будущей селекции.
- 2026-03-21 — Заготовка расширена до полной реализации в задаче `spec-plant-breeding-implementation`.
