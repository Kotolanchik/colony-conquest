# Задача: `spec-entertainment-implementation` — Полная реализация `entertainment_spec` в коде

## Цель

Реализовать базовый runtime-контур развлечений по `spec/entertainment_spec.md`: итоговое настроение, модификатор продуктивности, стресс-редукция, риск азартных игр, праздники и интеграция с преступностью.

## Спецификации

- [x] `spec/entertainment_spec.md` — §1.2, §5.2, §6, §7
- [x] `spec/COLONY_CONQUEST_MASTER_SPECIFICATION.md` — связь социума, настроения и производительности
- [x] `spec/crime_justice_spec.md` — эффект доступности развлечений на преступность

## Связанные ADR

- нет

## Критерии готовности (Definition of Done)

- [x] ECS-модель развлечений: `EntertainmentSimulationState`, `EntertainmentFestivalState`.
- [x] Формулы: `EntertainmentMath` (итоговое настроение, пороги продуктивности, стресс-редукция, риск азартных игр).
- [x] Bootstrap: `EntertainmentBootstrapSystem` создаёт стартовые параметры досуга.
- [x] Симуляция: `EntertainmentDailySystem` пересчитывает настроение/продуктивность/стресс ежедневно, генерирует праздничные события.
- [x] Интеграция: `EntertainmentDailySystem` обновляет `CrimeJusticeState.EntertainmentAccess01`.
- [x] Метрики `Entertainment*` добавлены и пишутся ежедневно.
- [x] README обновлён проверочными шагами.

## Риски и допущения

- Развлечения моделируются агрегированно, без индивидуальных предпочтений каждого поселенца.
- Праздники реализованы как периодический демо-триггер (каждые 90 игровых дней), без контентного календаря по культуре/религии.

## Заметки / журнал

- 2026-03-21 — добавлены `EntertainmentSimulationComponents`, `EntertainmentMath`, `EntertainmentBootstrapSystem`, `EntertainmentDailySystem`.
