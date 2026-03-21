# Задача: `domain-diplomacy-faction-ids-stub` — типы фракций и идеологии

## Цель

Закрепить идентификаторы типов фракций (§1.2) и идеологий (§2.3) из `spec/diplomacy_trade_spec.md` для данных и будущей симуляции отношений.

## Спецификации

- [x] `spec/diplomacy_trade_spec.md` — §1.2, §2.3
- [x] `spec/COLONY_CONQUEST_MASTER_SPECIFICATION.md`

## Связанные ADR

- нет

## Критерии готовности (Definition of Done)

- [x] `ColonyConquest.Diplomacy.FactionKindId` — строки таблицы §1.2.
- [x] `ColonyConquest.Diplomacy.FactionIdeologyId` — строки таблицы §2.3.
- [x] Проверка: имена enum построчно с таблицами в спеке.

## Заметки / журнал

- 2026-03-21 — `game/Client/Assets/_Project/Scripts/Diplomacy/FactionKindId.cs`, `FactionIdeologyId.cs`.
