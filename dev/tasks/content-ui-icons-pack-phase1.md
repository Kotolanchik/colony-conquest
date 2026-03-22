# Задача: `content-ui-icons-pack-phase1` — Пак UI-иконок и атласов (Phase 1)

## Цель

Собрать единый набор UI-иконок (юниты/здания/ресурсы/уведомления/действия), подключить atlas-пайплайн и заполнить `UiIconCatalog`.

## Спецификации

- [x] `spec/ui_ux_spec.md`
- [x] `spec/statistics_analytics_spec.md`
- [x] `spec/events_quests_spec.md`
- [x] `spec/COLONY_CONQUEST_MASTER_SPECIFICATION.md`

## Связанные ADR

- нет

## Критерии готовности (Definition of Done)

- [ ] Подготовлены master-иконки (style-consistent) и runtime-экспорт в `Assets/_Project/Art/UI/Icons/`.
- [ ] Сформирован atlas для HUD/панелей.
- [ ] Заполнен `UiIconCatalog.asset` (`iconId -> Sprite + kind + tint`).
- [ ] Проверка в Play Mode: UI-запросы к иконкам резолвятся через каталог.
- [ ] Утверждены правила нейминга иконок и состояния (normal/hover/disabled).

## Риски и допущения

- Финальный арт-стиль может корректироваться после UX playtests.
- Для дальтонизма возможны отдельные варианты палитры/иконок.

## Заметки / журнал

- 2026-03-21 — задача заведена как следующий блок после внедрения каталогов презентации.
