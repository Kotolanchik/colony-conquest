# Задача: `spec-bioengineering-implementation` — Полная реализация `bioengineering_spec` в коде

## Цель

Реализовать ECS runtime биоинженерии по `spec/bioengineering_spec.md`: пациенты, процедуры (протезы, стимуляторы, генная терапия, клонирование, нейроинтерфейсы), риски/успех, зависимость и детокс.

## Спецификации

- [x] `spec/bioengineering_spec.md` — §2.2–§2.3, §3.1–§3.2, §4.1–§4.2, §5.1, §6.1–§6.2
- [x] `spec/COLONY_CONQUEST_MASTER_SPECIFICATION.md` — интеграция биотеха с медицинским контуром

## Связанные ADR

- нет

## Критерии готовности (Definition of Done)

- [x] ECS runtime: `BioengineeringSimulationState`, буферы `BioPatientEntry` и `BioengineeringProcedureEntry`.
- [x] Формулы: `BioengineeringSimulationMath` (длительности процедур, вероятность успеха, уровни зависимости/withdrawal).
- [x] Bootstrap: `BioengineeringBootstrapSystem` с демо-пациентами и тремя типами процедур.
- [x] Суточная симуляция: `BioengineeringDailySystem`:
  - [x] уменьшение `RemainingDays` у процедур и разрешение исхода,
  - [x] применение результатов процедур к пациенту,
  - [x] эффекты/откат стимуляторов,
  - [x] обновление зависимости и детокса.
- [x] Story events для `bio-success` / `bio-failure`.
- [x] Метрики `Bioengineering*` добавлены и пишутся ежедневно.
- [x] README обновлён шагами проверки.

## Риски и допущения

- Реализация агрегированная: без отдельной ECS-схемы органов/операционных команд и без UI-лечения.
- Детальные риски (инфекция/рак/взлом/рецидив) сведены к вероятностной модели процедур на этапе runtime-прототипа.

## Заметки / журнал

- 2026-03-21 — добавлены `BioengineeringSimulationComponents`, `BioengineeringSimulationMath`, `BioengineeringBootstrapSystem`, `BioengineeringDailySystem`.
