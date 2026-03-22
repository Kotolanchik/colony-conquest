# Задача: `content-units-buildings-visual-pack-phase1` — Пак модельных ассетов юнитов и зданий (Phase 1)

## Цель

Собрать первый production-набор 3D ассетов (юниты + здания) и привязать его к каталогам визуализации:
`UnitVisualCatalog` и `BuildingVisualCatalog`.

## Спецификации

- [x] `spec/ui_ux_spec.md`
- [x] `spec/military_system_specification.md`
- [x] `spec/construction_system_spec.md`
- [x] `spec/manufacturing_plants_spec.md`
- [x] `spec/COLONY_CONQUEST_MASTER_SPECIFICATION.md`

## Связанные ADR

- нет

## Критерии готовности (Definition of Done)

- [ ] Импортированы и настроены ассеты юнитов в `Assets/_Project/Art/Units/`.
- [ ] Импортированы и настроены ассеты зданий в `Assets/_Project/Art/Buildings/`.
- [ ] Для ассетов заданы LOD, pivot, scale и базовые материалы.
- [ ] Созданы/обновлены `UnitVisualCatalog.asset` и `BuildingVisualCatalog.asset`.
- [ ] Проверка в Play Mode: bridge принимает запросы и корректно резолвит prefab-entries из каталогов.
- [ ] Обновлены README/гайд по pipeline при изменениях.

## Риски и допущения

- Реальные художественные ассеты могут создаваться во внешних DCC/AI-инструментах; в репозиторий попадает уже подготовленный импорт.
- Для массовых юнитов может потребоваться дополнительная оптимизация материалов/атласов.

## Заметки / журнал

- 2026-03-21 — задача заведена как следующий блок после внедрения presentation bridge-контуров.
