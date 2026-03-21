# Задача: `tech-netcode-spike` — Spike: Netcode for Entities (подключение, пустой сценарий)

## Цель

Пакет `com.unity.netcode` подключён к `game/Client`, сборка ссылается на `Unity.NetCode`; включён `ClientServerBootstrap` с авто-подключением и минимальным сценарием «go in game» (RPC + `NetworkStreamInGame`). Существующие системы помечены `WorldSystemFilter` под клиент/сервер.

## Спецификации

- [x] `spec/technical_architecture_specification.md` — сеть Netcode for Entities
- [x] `spec/COLONY_CONQUEST_MASTER_SPECIFICATION.md` — согласованность стека

## Связанные ADR

- нет (версии пакетов зафиксированы в `Packages/manifest.json`)

## Критерии готовности (Definition of Done)

- [x] В `manifest.json` добавлен `com.unity.netcode` (1.5.1).
- [x] `Colony.Conquest.Core.asmdef` ссылается на `Unity.NetCode`.
- [x] `ColonyNetcodeBootstrap` + `GoInGameSpike` (RPC, клиент/сервер).
- [x] Системы ввода/движения — `ClientSimulation`; бенчмарк фазы 0 — `ServerSimulation`; `GameBootstrapSystem` — клиент и сервер.
- [x] Проверка: открыть проект в Unity 6, дождаться разрешения пакетов, **Play** в сцене Bootstrap — в консоли нет ошибок компиляции; **Window → Entities → Worlds** — видны `ClientWorld` и `ServerWorld`.

## Риски и допущения

- Замер Phase 0 выполняется на серверном мире: лог бенчмарка идёт из серверной симуляции; визуализация дрейфа на клиенте в этом spike не требуется.
- Синхронизация трансформов игрока между клиентом и сервером и ghost-префабы — отдельные задачи.

## Заметки / журнал

- 2026-03-21 — Реализация в `Assets/_Project/Scripts/Netcode/`, правки фильтров в существующих системах.
- 2026-03-21 — `NetcodeSpikeState` + `NetcodeSpikeStateSyncSystem`: `TransportReady = 1` при появлении `NetworkStreamInGame` (помимо `ColonyNetcodeBootstrap`, `GoInGameSpike`).
