# Kanban — Colony & Conquest

**Источник истины для агента:** `dev/kanban.json`. Этот файл — зеркало для чтения; при переносе карточек обновляй оба.

**Отслеживаемость:** бэклог и поток здесь → детали в `dev/tasks/<id>.md` (шаблон `dev/TASK_CARD_TEMPLATE.md`) → устойчивые решения в `docs/decisions/`. Поля JSON: `task_file`, `adr_refs`, **`spec_refs` (обязательно для задач, затрагивающих дизайн/код)**. Подбор спек: `dev/BACKLOG_SPEC_INDEX.md`.

**Definition of Done:** см. навык `colony-kanban` и чеклист в `dev/TASK_CARD_TEMPLATE.md`.

**Обновлено:** 2026-03-21

---

## Backlog

_(пусто)_

## Ready

_(пусто)_

## In progress

_(пусто)_

## Review

_(пусто)_

## Done

- **repo-dev-process** — Настроены rules, skills, AGENTS, канбан, структура, индекс спеков  
  - Labels: process  
  - ADR: `docs/decisions/001-adr-and-task-traceability.md`

- **init-unity-dots** — Инициализировать Unity + DOTS проект в game/Client  
  - Spec: `spec/technical_architecture_specification.md`  
  - Task: `dev/tasks/init-unity-dots.md`  
  - ADR: `docs/decisions/002-unity6-editor-baseline.md`  
  - Labels: setup, engine

- **spec-cross-review** — Первичный кросс-ревью всех spec/*.md против мастер-документа  
  - Spec: `spec/COLONY_CONQUEST_MASTER_SPECIFICATION.md`  
  - Task: `dev/tasks/spec-cross-review.md`  
  - Отчёт: `dev/spec_cross_review_report.md`  
  - Labels: design, process

- **tech-spec-unity6-align** — Согласовать техспеку и мастер §5.1 с Unity 6 (ADR-002)  
  - Spec: `spec/technical_architecture_specification.md`, мастер §5.1  
  - Task: `dev/tasks/tech-spec-unity6-align.md`  
  - ADR: `docs/decisions/002-unity6-editor-baseline.md`  
  - Labels: design, engine

- **feat-ecs-simulation-root** — ECS: синглтон SimulationRootState + тик в GameBootstrapSystem  
  - Spec: `spec/technical_architecture_specification.md`  
  - Task: `dev/tasks/feat-ecs-simulation-root.md`  
  - Labels: engine, gameplay

- **scene-bootstrap-subscene** — Сцена Bootstrap + SubScene (DOTS) и запись в Build Settings  
  - Spec: `spec/technical_architecture_specification.md`  
  - Task: `dev/tasks/scene-bootstrap-subscene.md`  
  - Labels: engine, scene

- **input-actions-wasd** — Input System: базовый .inputactions и чтение в ECS/бридже  
  - Spec: `spec/ui_ux_spec.md`  
  - Task: `dev/tasks/input-actions-wasd.md`  
  - Labels: engine, input

- **ecs-subscene-baked-entity** — SubScene: одна тестовая сущность с LocalTransform после baking  
  - Spec: `spec/technical_architecture_specification.md`, `spec/COLONY_CONQUEST_MASTER_SPECIFICATION.md`  
  - Task: `dev/tasks/ecs-subscene-baked-entity.md`  
  - Labels: engine, ecs, scene

- **ecs-movement-from-input** — Система движения: InputCommandState.Move → сдвиг LocalTransform (масштаб/скорость из констант)  
  - Spec: `spec/technical_architecture_specification.md`, `spec/ui_ux_spec.md`, `spec/COLONY_CONQUEST_MASTER_SPECIFICATION.md`  
  - Task: `dev/tasks/ecs-movement-from-input.md`  
  - Labels: engine, gameplay, input

- **roadmap-phase0-benchmark** — Прототип фазы 0: замер производительности ECS (цель из дорожной карты §6)  
  - Spec: `spec/COLONY_CONQUEST_MASTER_SPECIFICATION.md`, `spec/technical_architecture_specification.md`  
  - Task: `dev/tasks/roadmap-phase0-benchmark.md`  
  - Labels: engine, perf

- **domain-settler-components-stub** — Заготовка набора ECS-компонентов поселенца (без полной симуляции)  
  - Spec: `spec/settler_simulation_system_spec.md`, `spec/COLONY_CONQUEST_MASTER_SPECIFICATION.md`  
  - Task: `dev/tasks/domain-settler-components-stub.md`  
  - Labels: gameplay, design

- **domain-economy-resource-ids** — Идентификаторы ресурсов и перечень в коде по экономической спеке  
  - Spec: `spec/economic_system_specification.md`, `spec/COLONY_CONQUEST_MASTER_SPECIFICATION.md`  
  - Task: `dev/tasks/domain-economy-resource-ids.md`  
  - Labels: gameplay, data

- **domain-military-unit-stub** — Минимальная ECS-схема боевого юнита (без полного боя)  
  - Spec: `spec/military_system_specification.md`, `spec/technical_architecture_specification.md`  
  - Task: `dev/tasks/domain-military-unit-stub.md`  
  - Labels: gameplay, combat

- **tech-audio-bus-stub** — Заготовка аудио-событий / шины под звуковую спеку  
  - Spec: `spec/audio_design_spec.md`  
  - Task: `dev/tasks/tech-audio-bus-stub.md`  
  - Labels: audio, engine

- **world-map-chunk-stub** — Заготовка чанка/тайла глобальной карты (данные + границы)  
  - Spec: `spec/global_map_spec.md`, `spec/COLONY_CONQUEST_MASTER_SPECIFICATION.md`  
  - Task: `dev/tasks/world-map-chunk-stub.md`  
  - Labels: gameplay, world

- **meta-events-pipeline-stub** — Заготовка конвейера событий/квестов (очередь + типы из спеки)  
  - Spec: `spec/events_quests_spec.md`, `spec/COLONY_CONQUEST_MASTER_SPECIFICATION.md`  
  - Task: `dev/tasks/meta-events-pipeline-stub.md`  
  - Labels: gameplay, scripting

- **meta-analytics-hooks-stub** — Точки телеметрии/статистики по спеке аналитики  
  - Spec: `spec/statistics_analytics_spec.md`  
  - Task: `dev/tasks/meta-analytics-hooks-stub.md`  
  - Labels: engine, telemetry

- **domain-construction-ghost-stub** — Заготовка потока «режим строительства / призрак» (без полной сетки)  
  - Spec: `spec/construction_system_spec.md`, `spec/ui_ux_spec.md`  
  - Task: `dev/tasks/domain-construction-ghost-stub.md`  
  - Labels: gameplay, ui

- **feat-construction-build-toggle** — Ввод ToggleBuild (B), переключение `ConstructionGhostState`, чертёж по умолчанию, аудио/аналитика  
  - Spec: `spec/ui_ux_spec.md`, `spec/construction_system_spec.md`  
  - Task: `dev/tasks/feat-construction-build-toggle.md`  
  - Labels: engine, input, gameplay

- **tech-netcode-spike** — Spike: Netcode for Entities (подключение, пустой сценарий)  
  - Spec: `spec/technical_architecture_specification.md`, `spec/COLONY_CONQUEST_MASTER_SPECIFICATION.md`  
  - Task: `dev/tasks/tech-netcode-spike.md`  
  - Labels: engine, network

- **domain-agriculture-mining-stub** — Заготовка CropKindId и MiningDepositKindId по agriculture_mining_spec  
  - Spec: `spec/agriculture_mining_spec.md`, `spec/COLONY_CONQUEST_MASTER_SPECIFICATION.md`  
  - Task: `dev/tasks/domain-agriculture-mining-stub.md`  
  - Labels: gameplay, data

- **domain-agriculture-phase0-stub** — Агро фазы 0: LivestockKindId + MiningDepositRuntime  
  - Spec: `spec/agriculture_mining_spec.md`  
  - Task: `dev/tasks/domain-agriculture-phase0-stub.md`  
  - Labels: gameplay, data

- **domain-technology-tree-ids-stub** — Заготовка TechEraId и TechBranchId по technology_tree_spec  
  - Spec: `spec/technology_tree_spec.md`, `spec/COLONY_CONQUEST_MASTER_SPECIFICATION.md`  
  - Task: `dev/tasks/domain-technology-tree-ids-stub.md`  
  - Labels: gameplay, data

- **domain-bioengineering-prosthesis-ids-stub** — Заготовка CyberneticProsthesisKindId и StimulantKindId по bioengineering_spec  
  - Spec: `spec/bioengineering_spec.md`, `spec/COLONY_CONQUEST_MASTER_SPECIFICATION.md`  
  - Task: `dev/tasks/domain-bioengineering-prosthesis-ids-stub.md`  
  - Labels: gameplay, data

- **domain-plant-breeding-trait-axes-stub** — Заготовка PlantTraitAxisId по plant_breeding_spec §1.2  
  - Spec: `spec/plant_breeding_spec.md`, `spec/COLONY_CONQUEST_MASTER_SPECIFICATION.md`  
  - Task: `dev/tasks/domain-plant-breeding-trait-axes-stub.md`  
  - Labels: gameplay, data

- **feat-simulation-calendar-stub** — Игровой календарь: день и время суток от SimulationTick  
  - Spec: `spec/COLONY_CONQUEST_MASTER_SPECIFICATION.md`, `spec/technical_architecture_specification.md`  
  - Task: `dev/tasks/feat-simulation-calendar-stub.md`  
  - Labels: engine, gameplay

- **domain-ecology-indicators-stub** — Заготовка индикаторов экологии и полос загрязнения  
  - Spec: `spec/ecology_spec.md`, `spec/COLONY_CONQUEST_MASTER_SPECIFICATION.md`  
  - Task: `dev/tasks/domain-ecology-indicators-stub.md`  
  - Labels: gameplay, data

- **feat-phase0-manual-mining** — Фаза 0: ручная добыча — склад ECS, маппинг месторождение→ResourceId, демо-узел  
  - Spec: `spec/agriculture_mining_spec.md`, `spec/economic_system_specification.md`, мастер §6.1  
  - Task: `dev/tasks/feat-phase0-manual-mining.md`  
  - Labels: gameplay, ecs, data  
  - Примечание: расширено карточкой **spec-agriculture-mining-implementation**

- **spec-agriculture-mining-implementation** — Полная реализация `spec/agriculture_mining_spec` в коде (формулы + ручная добыча)  
  - Spec: `spec/agriculture_mining_spec.md`, `spec/economic_system_specification.md`  
  - Task: `dev/tasks/spec-agriculture-mining-implementation.md`  
  - Labels: gameplay, ecs, design  
  - Примечание: `CropGrowthSimulationSystem`, `LivestockDailyProductionSystem`, bootstrap (грядка, загон, `InitialAmount` у узла)

- **spec-economy-production-workshop** — Экономика: §2.1 эффективность + §2.2–2.3 цепочки (демо-цех)  
  - Spec: `spec/economic_system_specification.md`, `spec/COLONY_CONQUEST_MASTER_SPECIFICATION.md`  
  - Task: `dev/tasks/spec-economy-production-workshop.md`  
  - Labels: gameplay, ecs, data  
  - Примечание: эпоха 1–3 + §2.4.1 легирование; стройка §2.2 эпоха 2 — сетки в `ConstructionBlueprintFootprints`

- **feat-statistics-analytics-spec-full** — Слой данных статистики и аналитики по statistics_analytics_spec (снимок, метрики, ИЧР)  
  - Spec: `spec/statistics_analytics_spec.md`, `spec/COLONY_CONQUEST_MASTER_SPECIFICATION.md`  
  - Task: `dev/tasks/feat-statistics-analytics-spec-full.md`  
  - Labels: gameplay, data, telemetry

- **feat-ai-director-policy-stub** — AI Director: политика событий §2.3 + очередь триггеров  
  - Spec: `spec/events_quests_spec.md`, `spec/COLONY_CONQUEST_MASTER_SPECIFICATION.md`  
  - Task: `dev/tasks/feat-ai-director-policy-stub.md`  
  - Labels: gameplay, ecs, scripting

- **domain-diplomacy-faction-ids-stub** — Заготовка FactionKindId и FactionIdeologyId по diplomacy_trade_spec  
  - Spec: `spec/diplomacy_trade_spec.md`, `spec/COLONY_CONQUEST_MASTER_SPECIFICATION.md`  
  - Task: `dev/tasks/domain-diplomacy-faction-ids-stub.md`  
  - Labels: gameplay, data

- **domain-crime-offense-ids-stub** — Заготовка CrimeOffenseKindId по crime_justice_spec §2  
  - Spec: `spec/crime_justice_spec.md`, `spec/COLONY_CONQUEST_MASTER_SPECIFICATION.md`  
  - Task: `dev/tasks/domain-crime-offense-ids-stub.md`  
  - Labels: gameplay, data

- **domain-entertainment-ids-stub** — Заготовка EntertainmentCategoryId / EntertainmentActivityKindId по entertainment_spec §1.2  
  - Spec: `spec/entertainment_spec.md`, `spec/COLONY_CONQUEST_MASTER_SPECIFICATION.md`  
  - Task: `dev/tasks/domain-entertainment-ids-stub.md`  
  - Labels: gameplay, data

- **domain-religion-archetype-stub** — Заготовка ReligionArchetypeId по religion_cults_spec §1.2  
  - Spec: `spec/religion_cults_spec.md`, `spec/COLONY_CONQUEST_MASTER_SPECIFICATION.md`  
  - Task: `dev/tasks/domain-religion-archetype-stub.md`  
  - Labels: gameplay, data

- **domain-politics-doctrine-stub** — Заготовка PoliticalDoctrineId по political_system_spec §1.2–2  
  - Spec: `spec/political_system_spec.md`, `spec/COLONY_CONQUEST_MASTER_SPECIFICATION.md`  
  - Task: `dev/tasks/domain-politics-doctrine-stub.md`  
  - Labels: gameplay, data

- **domain-defensive-structure-ids-stub** — Заготовка DefensiveStructureKindId по defensive_structures_spec §1.2  
  - Spec: `spec/defensive_structures_spec.md`, `spec/COLONY_CONQUEST_MASTER_SPECIFICATION.md`  
  - Task: `dev/tasks/domain-defensive-structure-ids-stub.md`  
  - Labels: gameplay, data, combat

- **domain-diplomacy-trade-contracts-stub** — Торговля и союзы: TradeDeal / TradeGoods / DiplomaticAlliance по diplomacy_trade_spec §3  
  - Spec: `spec/diplomacy_trade_spec.md`, `spec/COLONY_CONQUEST_MASTER_SPECIFICATION.md`  
  - Task: `dev/tasks/domain-diplomacy-trade-contracts-stub.md`  
  - Labels: gameplay, data

- **domain-world-map-scale-stub** — Заготовка WorldMapScaleLevel по global_map_spec §1.2  
  - Spec: `spec/global_map_spec.md`, `spec/COLONY_CONQUEST_MASTER_SPECIFICATION.md`  
  - Task: `dev/tasks/domain-world-map-scale-stub.md`  
  - Labels: gameplay, data, world

- **spec-plant-breeding-implementation** — Полная реализация `spec/plant_breeding_spec` в ECS (геном, скрещивание, риск ГМО)  
  - Spec: `spec/plant_breeding_spec.md`, `spec/COLONY_CONQUEST_MASTER_SPECIFICATION.md`  
  - Task: `dev/tasks/spec-plant-breeding-implementation.md`  
  - Labels: gameplay, ecs, design

- **spec-religion-cults-implementation** — Полная реализация `spec/religion_cults_spec` в ECS (вера, напряжение, священные войны)  
  - Spec: `spec/religion_cults_spec.md`, `spec/events_quests_spec.md`, `spec/COLONY_CONQUEST_MASTER_SPECIFICATION.md`  
  - Task: `dev/tasks/spec-religion-cults-implementation.md`  
  - Labels: gameplay, ecs, design

- **spec-comfort-housing-implementation** — Полная реализация `spec/comfort_housing_spec` в ECS (расселение, уют, аварии)  
  - Spec: `spec/comfort_housing_spec.md`, `spec/COLONY_CONQUEST_MASTER_SPECIFICATION.md`  
  - Task: `dev/tasks/spec-comfort-housing-implementation.md`  
  - Labels: gameplay, ecs, design

- **spec-diplomacy-trade-implementation** — Полная реализация `spec/diplomacy_trade_spec` в ECS (отношения, торговля, союзы, войны)  
  - Spec: `spec/diplomacy_trade_spec.md`, `spec/COLONY_CONQUEST_MASTER_SPECIFICATION.md`  
  - Task: `dev/tasks/spec-diplomacy-trade-implementation.md`  
  - Labels: gameplay, ecs, design

- **spec-crime-justice-implementation** — Полная реализация `spec/crime_justice_spec` в ECS (инциденты, полиция, суд, рецидив)  
  - Spec: `spec/crime_justice_spec.md`, `spec/COLONY_CONQUEST_MASTER_SPECIFICATION.md`, `spec/religion_cults_spec.md`  
  - Task: `dev/tasks/spec-crime-justice-implementation.md`  
  - Labels: gameplay, ecs, design

- **spec-entertainment-implementation** — Полная реализация `spec/entertainment_spec` в ECS (настроение, досуг, праздники, влияние на crime)  
  - Spec: `spec/entertainment_spec.md`, `spec/COLONY_CONQUEST_MASTER_SPECIFICATION.md`, `spec/crime_justice_spec.md`  
  - Task: `dev/tasks/spec-entertainment-implementation.md`  
  - Labels: gameplay, ecs, design

## Blocked

_(пусто)_
