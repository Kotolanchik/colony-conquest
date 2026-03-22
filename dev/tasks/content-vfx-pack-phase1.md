# Задача: `content-vfx-pack-phase1` — Базовый VFX-пак и pooling (Phase 1)

## Цель

Подготовить первый комплект VFX (бой/строительство/UI/погода), заполнить `VfxCatalog` и верифицировать производительность по budget-политике.

## Спецификации

- [x] `spec/audio_design_spec.md`
- [x] `spec/ui_ux_spec.md`
- [x] `spec/military_system_specification.md`
- [x] `spec/construction_system_spec.md`
- [x] `spec/ecology_spec.md`
- [x] `spec/COLONY_CONQUEST_MASTER_SPECIFICATION.md`

## Связанные ADR

- нет

## Критерии готовности (Definition of Done)

- [ ] Добавлены VFX-префабы в `Assets/_Project/Art/VFX/` по ключевым категориям.
- [ ] Заполнен `VfxCatalog.asset` (kind/prefab/default lifetime/prewarm pool).
- [ ] Подключён базовый pooling-профиль для часто используемых эффектов.
- [ ] Проверка в Play Mode: VFX-запросы корректно резолвятся и не создают аллокационный шторм.
- [ ] Зафиксированы перф-наблюдения (кадровая стабильность, пиковые bursts).

## Риски и допущения

- Качество VFX может требовать итераций между readability и performance.
- На low-tier профиле потребуется отдельная деградация сложных VFX Graph эффектов.

## Заметки / журнал

- 2026-03-21 — задача заведена как следующий блок после внедрения VFX-каталога и bridge-контрактов.
