# Задача: `world-map-chunk-stub` — чанк глобальной карты

## Цель

Ввести идентификаторы биомов и координату чанка с границами в мировых метрах по `spec/global_map_spec.md`.

## Спецификации

- [x] `spec/global_map_spec.md`
- [x] `spec/COLONY_CONQUEST_MASTER_SPECIFICATION.md`

## Связанные ADR

- нет

## Критерии готовности (Definition of Done)

- [x] `WorldBiomeId` по §2.1, `MapChunkCoord`, `MapChunkCoord.FromWorldPosition`, `MapChunkBounds.Contains`, `WorldMapChunkMetrics`.
- [x] `WorldMapFocusState` обновляется `WorldMapFocusFromPlayerSystem` по позиции `PlayerMoveTarget`.
- [x] Проверка: Play, двигать WASD — синглтон `WorldMapFocusState.PlayerChunk` / `PreviewBiome` меняются.

## Риски и допущения

- Процедурная генерация и сетка чанков глобального уровня — отдельные задачи.

## Заметки / журнал

- 2026-03-21 — `Assets/_Project/Scripts/World/WorldMapChunk.cs`.
- 2026-03-24 — `WorldMapBiomeSampling.GetBiomeForChunk` (хеш координаты → 12 биомов); `WorldMapFocusFromPlayerSystem` переведён на него вместо суммы координат.
