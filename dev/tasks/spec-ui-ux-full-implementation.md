# Задача: `spec-ui-ux-full-implementation` — Полная реализация `ui_ux_spec` в ECS

## Цель

Реализовать полный runtime-контур UI/UX по `spec/ui_ux_spec.md`: адаптивные уровни интерфейса по масштабу камеры, hotkeys, HUD-индикаторы ресурсов, лента уведомлений и настройки доступности.

## Спецификации

- [x] `spec/ui_ux_spec.md` — §1, §2, §3, §4, §5, §6, §7
- [x] `spec/COLONY_CONQUEST_MASTER_SPECIFICATION.md` — интеграция интерфейса с подсистемами
- [x] `spec/statistics_analytics_spec.md` — метрики UI
- [x] `spec/events_quests_spec.md` — уведомления о событиях/квестах
- [x] `spec/economic_system_specification.md` — ресурсные и энергетические индикаторы
- [x] `spec/military_system_specification.md` — предупреждения по снабжению и боевой обстановке

## Связанные ADR

- нет

## Критерии готовности (Definition of Done)

- [x] Добавлен full-runtime домен UI:
  - [x] `UiUxSimulationState` + буферы `UiNotificationEntry`, `UiHotkeyBindingEntry`, `UiPanelStateEntry`, `UiResourceIndicatorEntry`.
- [x] Добавлен `UiUxSimulationMath`:
  - [x] маппинг уровня карты → уровень UI,
  - [x] расчёт bands ресурсов и stress/hud-load,
  - [x] правила TTL уведомлений и нормализации accessibility-настроек.
- [x] Добавлен `UiUxBootstrapSystem`:
  - [x] bootstrap hotkey-каталога (general/selection/camera/build/military/time),
  - [x] bootstrap панелей с min/max уровнем камеры.
- [x] Добавлен `UiUxRuntimeSystem`:
  - [x] обработка hotkeys (`Space`, `Tab`, `F1-F4`, `~`, `1-3`, `Esc`),
  - [x] адаптивная видимость панелей по camera level и build mode,
  - [x] генерация уведомлений (critical/important/info/achievement),
  - [x] автопауза на critical и интеграция с audio UI-signals.
- [x] Аналитика:
  - [x] добавлены метрики `Ui*` (`0x94nn`),
  - [x] daily запись через `AnalyticsHooks`.
- [x] README клиента обновлён шагами проверки.

## Риски и допущения

- В рамках runtime-слоя реализованы данные/логика UI, а не конкретные Unity UI-префабы/экраны.
- Горячие клавиши реализованы как системный input-контур в ECS; визуальная локализация подсказок клавиш — отдельный контентный слой.
- Цветовые accessibility-профили представлены в виде runtime-переключателей состояний без отдельного пост-процессинга шейдеров.

## Заметки / журнал

- 2026-03-21 — добавлены `UiUxSimulationComponents`, `UiUxSimulationMath`, `UiUxBootstrapSystem`, `UiUxRuntimeSystem`; реализованы уведомления, adaptive panels, hotkeys и UI-метрики.
