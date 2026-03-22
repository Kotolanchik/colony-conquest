# Задача: `tech-audio-bus-stub` — заготовка аудио-событий

## Цель

Дать контракт вызова SFX по категориям из `spec/audio_design_spec.md` §3.1 до подключения FMOD/Wwise.

## Спецификации

- [x] `spec/audio_design_spec.md`

## Связанные ADR

- нет

## Критерии готовности (Definition of Done)

- [x] `AudioSfxCategory`, `AudioBusPendingEntry`, синглтон с буфером, `AudioBusStub.Post` пишет в буфер, `AudioBusDrainSystem` очищает его каждый кадр (воспроизведение — после FMOD/Wwise).
- [x] Проверка: Play, нажать **B** (режим строительства) — в буфере появляются события до drain; внешнего звука нет — намеренно.

## Риски и допущения

- Таблица `eventId` → банк событий — вне этой задачи.

## Заметки / журнал

- 2026-03-21 — `Assets/_Project/Scripts/Audio/`.
- 2026-03-21 — superseded полной реализацией `spec-audio-design-full-implementation` (adaptive music + runtime ingest + бюджеты).
