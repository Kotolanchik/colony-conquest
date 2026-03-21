# Задача: `domain-settler-components-stub` — ECS-схема поселенца

## Цель

В коде зафиксированы типы компонентов из `spec/settler_simulation_system_spec.md` §1.1–1.7 для дальнейшей симуляции и систем без переписывания контрактов.

## Спецификации

- [x] `spec/settler_simulation_system_spec.md`
- [x] `spec/COLONY_CONQUEST_MASTER_SPECIFICATION.md`

## Связанные ADR

- нет

## Критерии готовности (Definition of Done)

- [x] Компоненты и вспомогательные структуры в `game/Client/Assets/_Project/Scripts/Settlers/`, namespace `ColonyConquest.Settlers`.
- [x] Отклонение от строки спеки задокументировано в коде: `SkillSet.Skills` — `FixedList256Bytes<Skill>` (вместо `FixedList128Bytes`) из‑за размера 20 навыков.
- [x] `MoodModifier.Description` — `FixedString32Bytes` (аналог FixedString32 из спеки).
- [x] Теги §1.7 — только `IEnableableComponent`, как в спеке.
- [x] `README` клиента обновлён; канбан — карточка в **done**.

## Риски и допущения

- Дальнейшие системы должны учитывать лимиты `FixedList*` и выравнивание полей при Burst.

## Заметки / журнал

- 2026-03-22 — Реализовано.
