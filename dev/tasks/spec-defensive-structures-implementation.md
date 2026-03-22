# Задача: `spec-defensive-structures-implementation` — Полная реализация `defensive_structures_spec` в коде

## Цель

Реализовать ECS runtime оборонительных сооружений по `spec/defensive_structures_spec.md`: строительство укреплений под огнём, учёт эпохальных ограничений для high-tech сооружений, ежедневный урон/разрушение и телеметрию.

## Спецификации

- [x] `spec/defensive_structures_spec.md` — §1.2, §2, §3, §4, §5.1–§5.2, §6
- [x] `spec/COLONY_CONQUEST_MASTER_SPECIFICATION.md` — интеграция с военной и технологической подсистемами
- [x] `spec/technology_tree_spec.md` — эпохальные ограничения на щиты/силовые поля/автотурели

## Связанные ADR

- нет

## Критерии готовности (Definition of Done)

- [x] ECS runtime: `DefensiveSimulationState`, буферы `DefensiveConstructionOrderEntry` и `DefensiveStructureRuntimeEntry`.
- [x] Формулы: `DefensiveSimulationMath` (скорость стройки под огнём, HP/defense/energy параметры).
- [x] Bootstrap: `DefensiveBootstrapSystem` с демо-заказами и активным укреплением.
- [x] Суточная симуляция: `DefensiveDailySystem`:
  - [x] прогресс стройки (с under-fire модификаторами),
  - [x] блокировка high-tech заказов до эпохи 5,
  - [x] расчёт урона сооружениям и удаление разрушенных,
  - [x] учёт энергопитания high-tech защит.
- [x] Story events для `defense-built` / `defense-destroyed` / `defense-era-lock`.
- [x] Метрики `Defense*` добавлены и пишутся ежедневно.
- [x] README обновлён шагами проверки.

## Риски и допущения

- Тактическая боёвка и взаимодействие с конкретными боевыми юнитами реализованы агрегированно через `IncomingDamagePressure`.
- Параметры HP/урона/энергии даны как runtime-тюнинг, который будет уточняться баланс-пассом.

## Заметки / журнал

- 2026-03-21 — добавлены `DefensiveSimulationComponents`, `DefensiveSimulationMath`, `DefensiveBootstrapSystem`, `DefensiveDailySystem`.
