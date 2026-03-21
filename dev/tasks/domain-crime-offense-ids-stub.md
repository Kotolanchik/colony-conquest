# Задача: `domain-crime-offense-ids-stub` — виды преступлений

## Цель

Закрепить перечень категорий преступлений по §2.1–2.3 `spec/crime_justice_spec.md` для данных инцидентов и будущей судебной симуляции.

## Спецификации

- [x] `spec/crime_justice_spec.md` — §2.1–2.3
- [x] `spec/COLONY_CONQUEST_MASTER_SPECIFICATION.md`

## Связанные ADR

- нет

## Критерии готовности (Definition of Done)

- [x] `ColonyConquest.Justice.CrimeOffenseKindId` — покрытие основных строк §2.1 (мелкие), §2.2 (серьёзные, без детализации отдельных статей, не вынесенных в enum), §2.3 (особо тяжкие).
- [x] Проверка: сопоставление с таблицами спеки; отдельные статьи из §2.2 при необходимости — расширение enum отдельной задачей.

## Заметки / журнал

- 2026-03-21 — `game/Client/Assets/_Project/Scripts/Justice/CrimeOffenseKindId.cs`.
- 2026-03-21 — Заготовка расширена до полной реализации в задаче `spec-crime-justice-implementation`.
