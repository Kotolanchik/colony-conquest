# Задача: `domain-ecology-indicators-stub` — индикаторы экологии и уровни загрязнения

## Цель

Зафиксировать в коде типы идентификаторов индикаторов экосистемы и полосы суммарного загрязнения по `spec/ecology_spec.md` без симуляции источников и восстановления.

## Спецификации

- [x] `spec/ecology_spec.md` — §1.2 индикаторы, §3 уровни загрязнения
- [x] `spec/COLONY_CONQUEST_MASTER_SPECIFICATION.md`

## Связанные ADR

- нет

## Критерии готовности (Definition of Done)

- [x] `EcologyIndicatorKind` — пять индикаторов (воздух, вода, почва, лес, биоразнообразие).
- [x] `PollutionLevelBand` — пять полос 0–100 (Чисто … Критическое).
- [x] Проверка: поиск по символам в IDE; соответствие подписей спеке §1.2 и §3.

## Заметки / журнал

- 2026-03-21 — `Assets/_Project/Scripts/Ecology/`.
- 2026-03-24 — `EcologyPollutionMath` (сводный индекс загрязнения 0–100, полосы `PollutionLevelBand`, множители §3.1–3.3), `ColonyPollutionSummaryState`, `EcologyPollutionSummarySystem`; интеграция урожая через `CropGrowthSimulationSystem` и `EcologyPollutionMath.GetCropYieldMultiplier`.
- 2026-03-24 — Идентификаторы источников §2.1–2.3: `EcologyAirPollutionSourceId`, `EcologyWaterPollutionSourceId`, `EcologySoilImpactSourceId`.
- 2026-03-24 — `EcologyPollutionSourceRates`: номиналы §2.1 воздух, §2.2 вода (поток и разливы).
- 2026-03-24 — `EcologySoilImpactRates.GetAnnualFertilityDelta01` — §2.3 почва (навоз, удобрения, пестициды, металлы, нефть).
