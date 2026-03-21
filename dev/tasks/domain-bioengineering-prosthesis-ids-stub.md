# Задача: `domain-bioengineering-prosthesis-ids-stub` — Идентификаторы протезов, имплантов и стимуляторов

## Цель

Зафиксирован перечень типов протезов/кибер-имплантов по таблице §2.2 и органам §2.1 `spec/bioengineering_spec.md`, а также стимуляторов §3.1 — для данных персонажа и крафта без симуляции операций.

## Спецификации

- [x] `spec/bioengineering_spec.md` — §2.1–2.2, §3.1
- [x] `spec/COLONY_CONQUEST_MASTER_SPECIFICATION.md` — контекст медицины/биотеха

## Связанные ADR

- нет

## Критерии готовности (Definition of Done)

- [x] `ColonyConquest.Bioengineering.CyberneticProsthesisKindId` покрывает строки таблицы §2.2 и искусственные лёгкие из обзора органов §2.1.
- [x] `ColonyConquest.Bioengineering.StimulantKindId` — таблица §3.1.
- [x] Проверка: сопоставление имён enum построчно с таблицами §2.2 и §3.1 в спеке.

## Заметки / журнал

- 2026-03-21 — `Assets/_Project/Scripts/Bioengineering/CyberneticProsthesisKindId.cs`.
- 2026-03-22 — `StimulantKindId.cs` по §3.1.
- 2026-03-24 — Расширение данных: `StimulantDependencyLevel` §3.2; `GeneTherapyApplicationKindId` / `GeneTherapyRiskKindId` §4; `CloningProcedureKindId` §5.1; `NeuroInterfaceKindId` / `NeuroInterfaceRiskKindId` §6 (без симуляции).
- 2026-03-24 — `StimulantEffectTuning` / `StimulantEffectDefinition` — таблица чисел §3.1 по видам; зависимость §3.2 в коде не симулируется.
