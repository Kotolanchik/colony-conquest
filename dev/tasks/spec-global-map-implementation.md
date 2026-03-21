# Задача: `spec-global-map-implementation` — Полная реализация `global_map_spec` в коде

## Цель

Реализовать runtime-контур глобальной карты по `spec/global_map_spec.md`: состояние мира, ресурсные узлы, территориальный контроль, стратегическое перемещение армий, открытие чанков и особых мест, а также связь масштаба карты с технологической эпохой.

## Спецификации

- [x] `spec/global_map_spec.md` — §1.2, §2, §3, §4.3, §5.1, §7, §8
- [x] `spec/COLONY_CONQUEST_MASTER_SPECIFICATION.md` — интеграция карты с дипломатией/экономикой/технологиями
- [x] `spec/technology_tree_spec.md` — привязка масштаба к эпохе

## Связанные ADR

- нет

## Критерии готовности (Definition of Done)

- [x] ECS runtime карты: `WorldMapSimulationState`, `WorldResourceNodeEntry`, `TerritoryControlEntry`, `DiscoveredChunkEntry`, `StrategicArmyEntry`, `SpecialSiteEntry`.
- [x] Добавлены типы: `WorldMapResourceKindId`, `StrategicMovementMode`.
- [x] Формулы/утилиты: `WorldMapSimulationMath` (влияние территорий, скорости и штрафы движения).
- [x] Bootstrap: `WorldMapBootstrapSystem` создаёт демо-карту, территорию, ресурсы, армии и особые места.
- [x] Суточная симуляция: `WorldMapDailySimulationSystem`:
  - [x] открытие чанков,
  - [x] пересчёт влияния и спорных зон,
  - [x] движение стратегических армий,
  - [x] присвоение контроля ресурсных узлов,
  - [x] открытие special sites и события.
- [x] `WorldMapFocusState` расширен полем `ActiveScale`; `WorldMapScaleFromTechSystem` выбирает масштаб по эпохе.
- [x] Метрики `WorldMap*` добавлены и пишутся ежедневно.
- [x] README обновлён шагами проверки.

## Риски и допущения

- Генерация мира реализована как детерминированный прототип с демо-данными, без полного off-line пайплайна высот/климата.
- Стратегические войны/осады представлены через движение и территориальный контроль; тактическая боёвка подключается отдельными системами.

## Заметки / журнал

- 2026-03-21 — добавлены `WorldMapSimulationComponents`, `WorldMapSimulationMath`, `WorldMapBootstrapSystem`, `WorldMapDailySimulationSystem`, `WorldMapScaleFromTechSystem`; `WorldMapFocusState` расширен `ActiveScale`.
