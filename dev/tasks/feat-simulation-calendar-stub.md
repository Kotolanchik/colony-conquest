# Задача: `feat-simulation-calendar-stub` — игровой календарь (день / время суток)

## Цель

Связать монотонный `SimulationRootState.SimulationTick` с понятными **игровыми сутками** и индексом дня для будущих смен, погоды и событий.

## Спецификации

- [x] `spec/COLONY_CONQUEST_MASTER_SPECIFICATION.md` — согласованность времени симуляции
- [x] `spec/technical_architecture_specification.md` — ECS-синглтоны

## Связанные ADR

- нет

## Критерии готовности (Definition of Done)

- [x] `GameCalendarTuning.SimulationTicksPerGameMinute` — константа масштаба.
- [x] Синглтон `GameCalendarState` (DayIndex, HourOfDay, MinuteOfHour) создаётся в `SubsystemBootstrapUtility`.
- [x] `ColonyCalendarAdvanceSystem` обновляет календарь после `GameBootstrapSystem`.
- [x] Проверка: Play — в Inspector синглтона `GameCalendarState` растут минуты/часы/дни при увеличении `SimulationTick`.

## Заметки / журнал

- 2026-03-21 — `Assets/_Project/Scripts/Simulation/`.
