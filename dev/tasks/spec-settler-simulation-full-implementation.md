# Задача: `spec-settler-simulation-full-implementation` — Полная реализация `settler_simulation_system_spec` в коде

## Цель

Довести подсистему поселенцев до полноценного runtime-контура по `spec/settler_simulation_system_spec.md`: генерация персонажей, ежедневная симуляция нужд/психологии/физиологии, рост навыков, уровни автономии, социальные связи, демография и интеграции с экономикой/правосудием/аналитикой.

## Спецификации

- [x] `spec/settler_simulation_system_spec.md` — §1.1–§1.7, §2, §3, §4, §5, §6, §7, §8.2, §9.2, §9.4
- [x] `spec/COLONY_CONQUEST_MASTER_SPECIFICATION.md` — интеграция поселенцев с остальными доменами
- [x] `spec/statistics_analytics_spec.md` — метрики `Settler*` и заполнение social snapshot
- [x] `spec/economic_system_specification.md` — потребление продовольствия из `ResourceStockEntry`

## Связанные ADR

- нет

## Критерии готовности (Definition of Done)

- [x] Добавлены недостающие типы поселенцев из спеки:
  - [x] `LifecycleState`, `SettlerStats`,
  - [x] `BodyPart`, `InjuryType`,
  - [x] `CommandStyle`, `CommanderAI`.
- [x] Введена runtime-модель подсистемы:
  - [x] `SettlerSimulationSingleton`,
  - [x] `SettlerSimulationState`,
  - [x] `SettlerAutonomyPolicyState`,
  - [x] `SettlerRuntimeId` / `SettlerRuntimeState`,
  - [x] `SkillUsageTracker`,
  - [x] буфер `SettlerRelationshipEdge`.
- [x] Реализованы формулы в `SettlerSimulationMath`:
  - [x] штрафы нужд для настроения,
  - [x] накопление/восстановление стресса,
  - [x] пороги и риск ментальных срывов,
  - [x] эффективность труда,
  - [x] XP-формулы роста/деградации навыков,
  - [x] тайминги автономии.
- [x] Реализован генератор персонажа `SettlerCharacterGenerator` (§2):
  - [x] identity/appearance,
  - [x] traits с несовместимостями и весами,
  - [x] aptitudes,
  - [x] skills + bonus points,
  - [x] mental conditions.
- [x] Реализована фабрика `SettlerEntityFactory` с полным набором ECS-компонентов и enableable-тегов.
- [x] Добавлен bootstrap `SettlerSimulationBootstrapSystem` (singleton + стартовая популяция + стартовые social bonds).
- [x] Добавлен daily-контур `SettlerSimulationDailySystem`:
  - [x] обновление нужд/настроения/стресса/физиологии,
  - [x] травмы/инфекции/смерть,
  - [x] рост и деградация навыков,
  - [x] автономия/AI-параметры,
  - [x] демография и рождение новых поселенцев,
  - [x] story events и `AnalyticsHooks`.
- [x] Интеграции:
  - [x] потребление пищи из `ResourceStockEntry`,
  - [x] обновление `ColonyDemographyState`,
  - [x] влияние на `CrimeJusticeState`,
  - [x] апдейт `ColonyTechProgressState.ScientistsCount`.
- [x] Расширены `AnalyticsMetricIds` блоком `Settler*` (0x8Fnn).
- [x] `AnalyticsSnapshotUpdateSystem` получает happiness/health/education и population из `SettlerSimulationState`.
- [x] README клиента обновлён шагами проверки подсистемы поселенцев.

## Риски и допущения

- Социальный граф реализован в упрощённом runtime-формате (буфер `SettlerRelationshipEdge`) без отдельного глобального hash-map.
- Рождения/прирост населения агрегированы (подход "популяционного шага"), без полного семейного дерева.
- Медицинская часть реализует суточную физиологическую модель (инфекции/раны/критические пороги) без процедурного госпиталя/операционной UI.

## Заметки / журнал

- 2026-03-21 — добавлены runtime-компоненты/формулы/генератор/factory/bootstrap/daily для `Settlers`, расширена аналитика и документация.
