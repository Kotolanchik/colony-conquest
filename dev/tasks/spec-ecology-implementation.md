# Задача: `spec-ecology-implementation` — Полная реализация `ecology_spec` в коде

## Цель

Довести подсистему экологии до полноценного runtime-контура по `spec/ecology_spec.md`: источники загрязнения, меры защиты, климатические эффекты, восстановление природы, интеграции с экономикой/поселенцами и события.

## Спецификации

- [x] `spec/ecology_spec.md` — §1, §2.1–§2.3, §3, §4.1–§4.3, §5, §6, §7
- [x] `spec/COLONY_CONQUEST_MASTER_SPECIFICATION.md` — интеграция экологии с остальными доменами
- [x] `spec/economic_system_specification.md` — связь с промышленной/энергетической нагрузкой
- [x] `spec/settler_simulation_system_spec.md` — влияние загрязнения на здоровье/настроение поселенцев
- [x] `spec/statistics_analytics_spec.md` — экологические метрики в аналитике

## Связанные ADR

- нет

## Критерии готовности (Definition of Done)

- [x] Введён runtime-синглтон экологии:
  - [x] `EcologySimulationState`,
  - [x] `EcologyMitigationState`,
  - [x] `EcologyDisasterState`,
  - [x] буферы источников `EcologyAirSourceEntry`, `EcologyWaterSourceEntry`, `EcologySoilSourceEntry`.
- [x] Добавлены формулы `EcologySimulationMath`:
  - [x] эффективность очистки воздуха/воды/почвы,
  - [x] era-мультипликаторы загрязнения,
  - [x] климат (GHG, температура, погодный риск),
  - [x] устойчивое развитие и восстановление природы.
- [x] Реализован bootstrap `EcologySimulationBootstrapSystem` с демо-настройками источников/мер.
- [x] Реализован daily-контур `EcologySimulationDailySystem`:
  - [x] расчёт загрязнения от источников и экономики,
  - [x] применение мер защиты и восстановления,
  - [x] обновление индикаторов экосистемы и суммарного загрязнения,
  - [x] климатическая динамика и риск катастроф,
  - [x] генерация story events (`eco-catastrophe`, `eco-protest`, `climate-anomaly`),
  - [x] запись `Ecology*` метрик в `AnalyticsHooks`.
- [x] Интеграции:
  - [x] `EcologyPollutionSummarySystem` обновляется после полного ecology daily,
  - [x] `SettlerSimulationDailySystem` учитывает `PollutionLevelBand` для здоровья/настроения,
  - [x] `AnalyticsSnapshotUpdateSystem` учитывает экологические множители при social health/happiness.
- [x] README клиента обновлён шагами проверки экологии.

## Риски и допущения

- Загрязнение моделируется агрегированно по колонии (без spatial-grid загрязнения по тайлам).
- Климатическая часть реализована как компактный индекс (`GreenhouseGasIndex`, `TemperatureAnomalyC`, `ExtremeWeatherRisk01`) без отдельной погодной системы.
- Меры экологии представлены уровнями политики/инфраструктуры, а не строительством отдельных объектов UI.

## Заметки / журнал

- 2026-03-21 — добавлены `EcologySimulationComponents`, `EcologySimulationMath`, `EcologySimulationBootstrapSystem`, `EcologySimulationDailySystem`; интеграции с settlers/analytics и story events.
