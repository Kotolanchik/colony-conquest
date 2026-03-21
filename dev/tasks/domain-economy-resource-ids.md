# Задача: `domain-economy-resource-ids` — ресурсы по экономической спеке

## Цель

В коде зафиксированы **стабильные идентификаторы** всех ресурсов из §1.2 `spec/economic_system_specification.md` и **каталог** с базовой ценой, категорией, редкостью и эпохой ввода — основа для складов, торговли и производства.

## Спецификации

- [x] `spec/economic_system_specification.md`
- [x] `spec/COLONY_CONQUEST_MASTER_SPECIFICATION.md`

## Связанные ADR

- нет

## Критерии готовности (Definition of Done)

- [x] `ResourceId` : byte — 55 ресурсов + `None`, порядок и значения задокументированы в коде.
- [x] `ResourceCatalog` заполняется из таблиц §1.2; `Get` / `TryGet` / `GetAllNonEmpty`.
- [x] В editor/dev сборке проверка согласованности индекса и `Id` (`RuntimeInitializeOnLoadMethod`).
- [x] `game/Client/README.md` описывает использование и правило расширения.

## Риски и допущения

- Ресурсы из §8 (снабжение армии) и побочные продукты переработки нефти в §2.3 — не в §1.2; при появлении в спеке — отдельная карточка.

## Заметки / журнал

- 2026-03-22 — Реализовано в `ColonyConquest.Economy`.
