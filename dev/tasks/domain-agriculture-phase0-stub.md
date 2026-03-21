# Задача: `domain-agriculture-phase0-stub` — Животноводство (ID) + компонент месторождения

## Цель

Расширить заготовку домена агро/добычи по `spec/agriculture_mining_spec.md`: перечень **животных §1.4** и **компонент узла месторождения** для будущей ручной добычи (фаза 0 дорожной карты).

## Спецификации

- [x] `spec/agriculture_mining_spec.md` — §1.4, §2.3

## Связанные ADR

- нет

## Критерии готовности (Definition of Done)

- [x] `ColonyConquest.Agriculture.LivestockKindId` — виды из таблицы §1.4 (эпохи в комментариях).
- [x] `ColonyConquest.Agriculture.MiningDepositRuntime` — `IComponentData`: вид месторождения + остаток запаса.
- [x] Проверка: в IDE поиск по символам; сборка `Colony.Conquest.Core` без ошибок.

## Заметки / журнал

- 2026-03-21 — файлы в `Assets/_Project/Scripts/Agriculture/`.
