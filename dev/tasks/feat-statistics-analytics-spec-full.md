# Задача: `feat-statistics-analytics-spec-full` — Реализация слоя данных статистики и аналитики

## Цель

По `spec/statistics_analytics_spec.md` реализован **клиентский слой данных**: таксономия метрик (§2–8), агрегированный снимок показателей, демография/техпрогресс как входы, расчёт ИЧР §6.2, запись телеметрии в буфер с поддержкой нескольких миров Netcode. Визуализация графиков и внешний бэкенд не входят (UI/экспорт — отдельные задачи).

## Спецификации

- [x] `spec/statistics_analytics_spec.md` — §1–8 (структура показателей и формулы, применённые в коде)
- [x] `spec/COLONY_CONQUEST_MASTER_SPECIFICATION.md` — интеграция метрик

## Связанные ADR

- нет

## Критерии готовности (Definition of Done)

- [x] `AnalyticsMetricIds` — покрытие разделов §2–§8 + интеграционные id.
- [x] `AnalyticsLocalSnapshot` + вложенные блоки — все группы показателей из спеки (значения обновляет `AnalyticsSnapshotUpdateSystem`).
- [x] `ColonyDemographyState`, `ColonyTechProgressState` — синглтоны; bootstrap и миграция в `SubsystemBootstrapUtility`.
- [x] ИЧР: `(Education01 + Health01 + Income01) / 3`, где `Income01 = saturate(GdpPerCapita / 2000)` (нормализация до ввода денежной симуляции).
- [x] ВВП и структура секторов: оценка по складу × `ResourceCatalog.BasePrice`, категории ресурсов → первичный/вторичный/третичный сектор.
- [x] `AnalyticsHooks.Record` — запись во все `World` с `AnalyticsServiceSingleton`.
- [x] Проверка: Play → в **Entities Hierarchy** (ClientWorld/ServerWorld) сущность с `AnalyticsServiceSingleton` содержит `AnalyticsLocalSnapshot`; после тика поля `Economy.Gdp`, `Social.HumanDevelopmentIndex` осмысленны; буфер `AnalyticsRecordEntry` пополняется при событии сюжета (тик 1) и при **B** (режим строительства).

## Риски и допущения

- Рождаемость/смертность по годам без полной демо-симуляции: накопители готовы, ставки 0 до системы поселенцев.
- Торговля, инфляция, бои: нули до соответствующих систем.
- Достижения: счётчик 0 до каталога достижений.

## Заметки / журнал

- 2026-03-21 — Реализация в `Assets/_Project/Scripts/Analytics/`, `Simulation/ColonyDemographyState.cs`, `ColonyTechProgressState.cs`.
- 2026-03-24 — `Social.Security01` = saturate(армия / (население×2%)) из `BattleUnitTag` и `ColonyDemographyState.Population` (согласовано с осями AI Director).
