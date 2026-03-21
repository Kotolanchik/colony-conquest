# Задача: `spec-comfort-housing-implementation` — Полная реализация `comfort_housing_spec` в коде

## Цель

Реализовать в ECS базовый контур жилья и уюта по `spec/comfort_housing_spec.md`: данные жилых блоков, очередь расселения, расчёт уюта и эффектов, износ/обслуживание, аварийные события и телеметрию.

## Спецификации

- [x] `spec/comfort_housing_spec.md` — §6, §8–§12
- [x] `spec/construction_system_spec.md` — связь с жилищным контуром по инфраструктуре
- [x] `spec/COLONY_CONQUEST_MASTER_SPECIFICATION.md` — интеграция с симуляцией поселенцев и экономики

## Связанные ADR

- нет

## Критерии готовности (Definition of Done)

- [x] ECS-модель жилья: `HousingUnitRuntime`, `HousingComfortSnapshot`, `HousingColonyState`, `HousingAssignmentRequestEntry`, `BarracksPolicyData`.
- [x] `HousingMath`:
  - [x] формула уюта (веса качества жилья/удобств/окружения/декора + модификаторы);
  - [x] score расселения;
  - [x] уровни перенаселения;
  - [x] расчёт следующего состояния износа.
- [x] Bootstrap: `HousingBootstrapSystem` создаёт демо-дома, барак и очередь на расселение.
- [x] `HousingAssignmentSystem` обрабатывает очередь переселения раз в игровой день.
- [x] `HousingDailyComfortSystem` раз в день считает уют/штрафы, износ, аварии инфраструктуры и публикует события.
- [x] Метрики `Housing*` добавлены в `AnalyticsMetricIds` и пишутся системой.
- [x] Документация проверки добавлена в `game/Client/README.md`.

## Риски и допущения

- Расселение работает по домохозяйствам в очереди, без личных сущностей поселенцев.
- Эффекты бараков реализованы на уровне параметров данных; прямой бафф боевых отрядов вынесен в отдельную задачу.

## Заметки / журнал

- 2026-03-21 — добавлены `HousingComponents`, `HousingMath`, `HousingBootstrapSystem`, `HousingAssignmentSystem`, `HousingDailyComfortSystem`; подключены события и аналитика.
