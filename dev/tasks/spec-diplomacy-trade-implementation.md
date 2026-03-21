# Задача: `spec-diplomacy-trade-implementation` — Полная реализация `diplomacy_trade_spec` в коде

## Цель

Реализовать рабочий ECS-контур дипломатии и торговли по `spec/diplomacy_trade_spec.md`: отношения фракций, торговые сделки с формулой цены, союзы, решение ИИ о войне и публикация событий/метрик.

## Спецификации

- [x] `spec/diplomacy_trade_spec.md` — §2, §3.2, §3.3, §7, §9
- [x] `spec/COLONY_CONQUEST_MASTER_SPECIFICATION.md` — интеграция дипломатии с экономикой и военной частью

## Связанные ADR

- нет

## Критерии готовности (Definition of Done)

- [x] ECS-модель дипломатии: `DiplomacySimulationState`, `FactionProfileEntry`, `DiplomaticRelationEntry`, `TradeDealEntry`, `DiplomaticAllianceEntry`, `ActiveWarEntry`.
- [x] Математика: `DiplomacyMath` (дельта отношений, цена сделки, шанс войны ИИ, совместимость идеологий).
- [x] Bootstrap: `DiplomacyBootstrapSystem` создаёт демо-фракции, отношения, сделку и союз.
- [x] Симуляция: `DiplomacyDailySystem` обновляет отношения, обрабатывает торговлю и союзы, создаёт войны по условиям §9.2.
- [x] Интеграция событий: ключевые моменты (`trade-complete`, `war-declared`) публикуются в `GameEventQueueEntry`.
- [x] Метрики: добавлены и заполняются `DiplomacyAverageRelations`, `DiplomacyTradeProfitDaily`, `DiplomacyActiveAlliances`, `DiplomacyWarsDeclaredTotal`.
- [x] README обновлён проверочными шагами.

## Риски и допущения

- Реализован агрегированный дипломатический слой без полноценного UI переговоров и без маршрутизации физ. караванов.
- Военные цели и мирные договоры представлены упрощённым runtime-полем `WarGoal`.

## Заметки / журнал

- 2026-03-21 — добавлены `DiplomacySimulationComponents`, `DiplomacyMath`, `DiplomacyBootstrapSystem`, `DiplomacyDailySystem`, расширен `FactionIdeologyId` (Isolationism, Imperialism).
