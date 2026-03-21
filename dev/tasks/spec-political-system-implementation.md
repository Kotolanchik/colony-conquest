# Задача: `spec-political-system-implementation` — Полная реализация `political_system_spec` в коде

## Цель

Реализовать ECS-контур политики по `spec/political_system_spec.md`: форма правления, доктрина, группы законов, модификаторы для экономики/науки/обороны/преступности, цикл политических решений и интеграция с соседними подсистемами.

## Спецификации

- [x] `spec/political_system_spec.md` — §2, §3, §4.1, §4.2, §5
- [x] `spec/COLONY_CONQUEST_MASTER_SPECIFICATION.md` — интеграция подсистем
- [x] `spec/diplomacy_trade_spec.md`, `spec/crime_justice_spec.md`, `spec/technology_tree_spec.md` — точки связки

## Связанные ADR

- нет

## Критерии готовности (Definition of Done)

- [x] Добавлена форма правления `GovernmentFormId` (таблица §4.2).
- [x] ECS runtime: `PoliticalSimulationState`, `PoliticalLawState`.
- [x] Формулы и таблицы эффектов: `PoliticalMath` (доктринные модификаторы, цикл решений, уровень демократии).
- [x] Bootstrap: `PoliticalBootstrapSystem`.
- [x] Суточная симуляция: `PoliticalDailySystem`:
  - [x] вычисление модификаторов доктрины/законов;
  - [x] обновление стабильности и эффективности решений;
  - [x] авто-смещение политики при низкой/высокой стабильности;
  - [x] события `policy-shift`.
- [x] Интеграция:
  - [x] влияет на `ColonyTechProgressState` (научный модификатор),
  - [x] влияет на `CrimeJusticeState.PenaltySeverity`,
  - [x] влияет на `DiplomacySimulationState.AverageRelations`.
- [x] Метрики `Politics*` добавлены и пишутся ежедневно.
- [x] README обновлён шагами проверки.

## Риски и допущения

- Политический контур реализован агрегированно (без партий/парламента как отдельных сущностей).
- Нормативные таблицы законов представлены упрощёнными агрегатами для стабильного runtime-прототипа.

## Заметки / журнал

- 2026-03-21 — добавлены `GovernmentFormId`, `PoliticalSimulationComponents`, `PoliticalMath`, `PoliticalBootstrapSystem`, `PoliticalDailySystem`.
