# Задача: `domain-defensive-structure-ids-stub` — типы оборонительных сооружений

## Цель

Закрепить классификацию полевых и стационарных укреплений по `spec/defensive_structures_spec.md` §1.2 для данных строительства и боя.

## Спецификации

- [x] `spec/defensive_structures_spec.md` — §1.2
- [x] `spec/COLONY_CONQUEST_MASTER_SPECIFICATION.md`

## Связанные ADR

- нет

## Критерии готовности (Definition of Done)

- [x] `DefensiveStructureKindId` — полевые, укреплённые, высокотехнологичные (эпоха 5) из диаграммы §1.2.
- [x] Проверка: сопоставление с перечислением в спеке.

## Заметки / журнал

- 2026-03-24 — `game/Client/Assets/_Project/Scripts/Defense/DefensiveStructureKindId.cs`.
- 2026-03-21 — Заготовка закрыта полной реализацией: `dev/tasks/spec-defensive-structures-implementation.md`.
