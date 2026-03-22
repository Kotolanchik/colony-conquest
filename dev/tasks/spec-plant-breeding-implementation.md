# Задача: `spec-plant-breeding-implementation` — Полная реализация `plant_breeding_spec` в коде

## Цель

Реализовать игровой контур селекции растений из `spec/plant_breeding_spec.md`: данные генома и линий, очередь скрещиваний, формулы наследования/мутаций/стабильности, риск ГМО, системные события и телеметрию.

## Спецификации

- [x] `spec/plant_breeding_spec.md` — §1.2, §2, §3, §4.3, §6–§10
- [x] `spec/COLONY_CONQUEST_MASTER_SPECIFICATION.md` — интеграция агро-подсистем и событий

## Связанные ADR

- нет

## Критерии готовности (Definition of Done)

- [x] ECS-модель селекции: `PlantBreedingLabState`, `PlantCultivarEntry`, `PlantBreedingWorkOrderEntry`.
- [x] Формулы: `PlantBreedingMath` (наследование по весам родителей, типы мутаций по tier лаборатории, стабильность линии, экориск ГМО).
- [x] Bootstrap: `PlantBreedingBootstrapSystem` создаёт демо-каталог родителей и стартовую заявку.
- [x] Симуляция: `PlantBreedingDailySimulationSystem` обрабатывает заявки раз в игровой день, регистрирует новые сорта, автогенерирует следующую заявку при пустой очереди.
- [x] Интеграция: при высоком риске ГМО публикуется событие в `GameEventQueueEntry`.
- [x] Телеметрия: метрики `PlantBreeding*` добавлены в `AnalyticsMetricIds` и пишутся из системы.
- [x] Документация проверки: шаги добавлены в `game/Client/README.md`.

## Риски и допущения

- Реализован **агрегированный** контур селекции без UI-экранов и без индивидуальных агрономов/лабораторий по районам.
- Текстовые имена сортов в демо — упрощённые; уникальные локализованные названия вынесены в будущий UI/data-pass.

## Заметки / журнал

- 2026-03-21 — добавлены `PlantBreedingComponents`, `PlantBreedingMath`, `PlantBreedingBootstrapSystem`, `PlantBreedingDailySimulationSystem`; обновлены метрики аналитики и README.
