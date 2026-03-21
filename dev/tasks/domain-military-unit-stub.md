# Задача: `domain-military-unit-stub` — минимальная ECS-схема боевого юнита

## Цель

Зафиксировать в коде типы и компоненты боевого юнита по §1.3 и §10.3 `spec/military_system_specification.md` без реализации боя и ИИ.

## Спецификации

- [x] `spec/military_system_specification.md`
- [x] `spec/technical_architecture_specification.md`

## Связанные ADR

- нет

## Критерии готовности (Definition of Done)

- [x] Пространство имён `ColonyConquest.Military` с `CommandHierarchy`, `MilitaryOrder`, `CombatStats`, `MilitaryAIState`, `MilitaryVisualState`, перечислениями и `BattleUnitTag`.
- [x] Демо-сущность с `BattleUnitTag` + `CombatStats` + `LocalTransform` создаётся в `SubsystemBootstrapUtility` (позиция (10,0,10)).
- [x] Сборка без ошибок; в Play — в Hierarchy мира симуляции видна сущность с тегом (поиск по компонентам).

## Риски и допущения

- Позиция/скорость юнита на уровне мира по-прежнему через `LocalTransform` / физику — дублирование `UnitBase` из спеки не вводилось.

## Заметки / журнал

- 2026-03-21 — Заготовка в `Assets/_Project/Scripts/Military/MilitaryUnitComponents.cs`.
