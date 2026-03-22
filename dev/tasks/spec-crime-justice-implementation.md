# Задача: `spec-crime-justice-implementation` — Полная реализация `crime_justice_spec` в коде

## Цель

Реализовать ECS-контур преступности и правосудия по `spec/crime_justice_spec.md`: расчёт уровня преступности, генерация инцидентов, раскрываемость полиции, наказания, рецидив, события и метрики.

## Спецификации

- [x] `spec/crime_justice_spec.md` — §1.2, §2, §3, §4.2, §6.3, §7.2
- [x] `spec/COLONY_CONQUEST_MASTER_SPECIFICATION.md` — интеграция с социальными подсистемами
- [x] `spec/religion_cults_spec.md` — влияние религиозности на crime-показатели

## Связанные ADR

- нет

## Критерии готовности (Definition of Done)

- [x] ECS-модель правосудия: `CrimeJusticeState`, `PoliceForceState`, `JusticeCourtState`, `CrimeIncidentStatsState`.
- [x] Формулы: `CrimeJusticeMath` (crime level, police efficiency, solve chance, recidivism).
- [x] Bootstrap: `CrimeJusticeBootstrapSystem` создаёт стартовые состояния.
- [x] Симуляция: `CrimeJusticeDailySystem` ежедневно пересчитывает уровень преступности, генерирует инциденты, применяет наказания и обновляет рецидив.
- [x] Интеграция: используется влияние `EntertainmentAccess01` (из entertainment) и `FaithLevelAvg` (из religion).
- [x] События по преступлениям публикуются в `GameEventQueueEntry`.
- [x] Метрики `Crime*` добавлены и записываются ежедневно.
- [x] README обновлён проверочными шагами.

## Риски и допущения

- На уровне прототипа не моделируется индивидуальный подозреваемый/процессуальные стадии расследования.
- Судебные вердикты и пенитенциарная система представлены агрегированно без отдельной сущности заключённого.

## Заметки / журнал

- 2026-03-21 — добавлены `CrimeJusticeComponents`, `CrimeJusticeMath`, `CrimeJusticeBootstrapSystem`, `CrimeJusticeDailySystem` с cross-domain интеграцией (entertainment + religion).
