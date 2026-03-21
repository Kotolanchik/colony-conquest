# Задача: tech-spec-unity6-align — Согласование техспеки с Unity 6

**Связи в канбане:** см. `dev/kanban.json`.

---

## Цель

Убрать противоречие между текстом `technical_architecture_specification.md` / мастер-спеки §5.1 (ранее «2023.2 LTS») и фактическим `game/Client` (Unity 6, ADR-002).

## Спецификации

- [x] `spec/technical_architecture_specification.md` — §1.3
- [x] `spec/COLONY_CONQUEST_MASTER_SPECIFICATION.md` — §5.1
- [x] `docs/decisions/002-unity6-editor-baseline.md`

## Связанные ADR

- `docs/decisions/002-unity6-editor-baseline.md`

## Критерии готовности (Definition of Done)

- [x] В техспеке явно указаны Unity 6, `ProjectVersion.txt`, ссылка на ADR
- [x] В мастер-спеке таблица §5.1 отражает Unity 6 и отсылки к манифесту
- [x] ADR обновлён в части «синхронизация со спеками»
- [x] `dev/spec_cross_review_report.md` — блок «Стек» актуализирован
- [x] `python tools/spec_index.py` при необходимости (изменения в spec)

## Заметки / журнал

- 2026-03-21 — Выполнено.
