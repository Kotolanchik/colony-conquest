# Задача: `spec-technology-tree-implementation` — Полная реализация `technology_tree_spec` в коде

## Цель

Реализовать runtime-контур дерева технологий по `spec/technology_tree_spec.md`: каталог технологий по эпохам/веткам, активное исследование, разблокировки, расчёт прогресса эпох и переходы между эпохами, события и телеметрию.

## Спецификации

- [x] `spec/technology_tree_spec.md` — §1.2, §1.3, §2.1–2.2, §3–§7
- [x] `spec/COLONY_CONQUEST_MASTER_SPECIFICATION.md` — связь прогрессии с экономикой/военной системой

## Связанные ADR

- нет

## Критерии готовности (Definition of Done)

- [x] ECS runtime дерева: `TechTreeSimulationState`, `TechUnlockedEntry`.
- [x] Каталог технологий: `TechDefinitionId`, `TechTreeCatalog` (репрезентативные технологии эпох 1–5 с prerequisite/стоимостью).
- [x] Bootstrap: `TechTreeBootstrapSystem`.
- [x] Суточная симуляция: `TechTreeDailySystem`:
  - [x] накопление пула исследований;
  - [x] выбор активной технологии;
  - [x] разблокировка технологий;
  - [x] прогресс и переходы эпох по порогам 50/60/70/80%.
- [x] `ColonyTechResearchDailySystem` обновлён: выдаёт «сырой» доход, распределение выполняет дерево технологий.
- [x] Интеграция: события `tech-unlocked`, `era-transition` в `GameEventQueueEntry`.
- [x] Метрики `TechActiveResearchId`, `TechResearchPoolPoints`, `TechEraTransitionsTotal`.
- [x] README обновлён шагами проверки.

## Риски и допущения

- Каталог технологий покрывает репрезентативное подмножество из спеки; расширение до полного контентного перечня может идти отдельной задачей data-pass.
- Требования по зданиям/ресурсам/населению сведены к prerequisite-графу и эпохальным порогам в текущем runtime.

## Заметки / журнал

- 2026-03-21 — добавлены `TechDefinitionId`, `TechTreeComponents`, `TechTreeCatalog`, `TechTreeBootstrapSystem`, `TechTreeDailySystem`; обновлён `ColonyTechResearchDailySystem`.
