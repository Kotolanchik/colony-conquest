# Задача: `domain-entertainment-ids-stub` — категории и виды развлечений

## Цель

Закрепить таксономию досуга по `spec/entertainment_spec.md` §1.2 для данных зданий, событий и настроения.

## Спецификации

- [x] `spec/entertainment_spec.md` — §1.2
- [x] `spec/COLONY_CONQUEST_MASTER_SPECIFICATION.md`

## Связанные ADR

- нет

## Критерии готовности (Definition of Done)

- [x] `EntertainmentCategoryId` — четыре блока §1.2 (спорт, искусство, игры, социальные).
- [x] `EntertainmentActivityKindId` — репрезентативный набор подтипов из каждого блока (без полного перечня всех видов спорта/искусства).
- [x] Проверка: сопоставление имён с диаграммой §1.2.

## Заметки / журнал

- 2026-03-21 — `EntertainmentCategoryId.cs`, `EntertainmentActivityKindId.cs` в `game/Client/Assets/_Project/Scripts/Entertainment/`.
- 2026-03-21 — Заготовка расширена до полной реализации в задаче `spec-entertainment-implementation`.
