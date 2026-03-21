# Задача: `domain-agriculture-mining-stub` — Идентификаторы культур и месторождений (заготовка)

## Цель

В коде зафиксированы перечисления **культур эпохи 1** и **типов месторождений** по `spec/agriculture_mining_spec.md` без симуляции полей и урожая.

## Спецификации

- [x] `spec/agriculture_mining_spec.md` — §1.1 культуры, §2.1 месторождения

## Связанные ADR

- нет

## Критерии готовности (Definition of Done)

- [x] `ColonyConquest.Agriculture.CropKindId` — перечень культур эпохи 1 из таблицы.
- [x] `ColonyConquest.Agriculture.MiningDepositKindId` — сжатый типовой ряд месторождений (лес, карьер, руды, нефть, эпохи 5).
- [x] Проверка: в IDE/редакторе поиск по символам `CropKindId` / `MiningDepositKindId` — ссылки на спеку в `///` комментариях.

## Заметки / журнал

- 2026-03-21 — Файлы в `Assets/_Project/Scripts/Agriculture/`.
