# Задача: `domain-diplomacy-trade-contracts-stub` — сделки, товары и союзы

## Цель

Закрепить таксономию торговли и союзов по `spec/diplomacy_trade_spec.md` §3.2–3.3 для данных дипломатии и контрактов.

## Спецификации

- [x] `spec/diplomacy_trade_spec.md` — §3.2–3.3
- [x] `spec/COLONY_CONQUEST_MASTER_SPECIFICATION.md`

## Связанные ADR

- нет

## Критерии готовности (Definition of Done)

- [x] `TradeDealKindId` — таблица типов сделок §3.2.
- [x] `TradeGoodsCategoryId` — категории товаров §3.2.
- [x] `DiplomaticAllianceKindId` — типы союзов §3.3.
- [x] Проверка: построчное сопоставление с таблицами спеки.

## Заметки / журнал

- 2026-03-24 — `game/Client/Assets/_Project/Scripts/Diplomacy/TradeDealKindId.cs`, `TradeGoodsCategoryId.cs`, `DiplomaticAllianceKindId.cs`.
