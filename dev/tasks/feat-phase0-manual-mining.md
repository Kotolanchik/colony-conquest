# Задача: `feat-phase0-manual-mining` — Ручная добыча фазы 0 (ECS)

## Цель

Связать узлы месторождений с экономическими `ResourceId`, склад в ECS и минимальный цикл «рядом с узлом → списание запаса → зачисление на склад» по `spec/agriculture_mining_spec.md` §2.2 и дорожной карте мастер-спеки §6.1 (фаза 0).

## Спецификации

- [x] `spec/agriculture_mining_spec.md` — §2.1–2.3
- [x] `spec/economic_system_specification.md` — `ResourceId`
- [x] `spec/COLONY_CONQUEST_MASTER_SPECIFICATION.md` — §6.1 фаза 0

## Связанные ADR

- нет

## Критерии готовности (Definition of Done)

- [x] `MiningDepositPrimaryResource.TryGetPrimaryResource` — маппинг типа месторождения на `ResourceId` (без маппинга — `false`, см. комментарии в коде).
- [x] Синглтон склада: `ResourceStockpileSingleton` + буфер `ResourceStockEntry`, создание в `SubsystemBootstrapUtility`.
- [x] Демо-узел леса в `(3,0,0)`, запас 500; игрок по умолчанию у `(0,0,0)` — радиус сбора 4 ед.
- [x] Система ручной добычи (ClientSimulation), после `PlayerMoveFromInputSystem` — эволюционировала в `ManualMiningGatherSystem` + формулы §2.2–2.3 (см. `dev/tasks/spec-agriculture-mining-implementation.md`).
- [x] Проверка: Play, подойти к демо-узлу (WASD к +X), удерживать **E (Interact)**; склад и запас узла меняются.

## Заметки / журнал

- 2026-03-21 — Скорость 2 ед/с для наглядности; спека §2.2 даёт ед/час — масштаб игры задаётся позже.
- 2026-03-22 — Полная реализация спеки: карточка **spec-agriculture-mining-implementation**; ввод Interact, тиры кирок, усталость, качество руды, исчерпание.
