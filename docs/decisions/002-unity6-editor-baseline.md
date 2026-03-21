# ADR-002: Базовая версия Unity Editor (Unity 6)

**Статус:** accepted  
**Дата:** 2026-03-21  

## Контекст

В `spec/technical_architecture_specification.md` указан ориентир **Unity 2023.2+ LTS** и DOTS. Для старта кода без локального Unity в CI нужен воспроизводимый каркас проекта; официальный **EntityComponentSystemSamples / EntitiesSamples** на момент интеграции собран под **Unity 6** (`m_EditorVersion: 6000.2.x`).

## Решение

1. Принять **Unity 6 (6000.2.x)** как текущую целевую версию редактора для `game/Client`, с синхронизацией по `ProjectSettings/ProjectVersion.txt`.
2. Проектные настройки и набор пакетов унаследовать от **EntitiesSamples** (Unity Technologies), затем добавить: `com.unity.inputsystem`, `com.unity.physics`, собственный код в `Assets/_Project/`.
3. При необходимости строгого соответствия спеке «только 2023.2 LTS» — отдельная задача: понижение версии редактора и пересборка `Packages/manifest.json` / lock под совместимые версии Entities/URP.

## Последствия

**Плюсы:** рабочий DOTS+URP+Physics+Input из коробки; быстрый вход в разработку.  
**Минусы:** при переходе на другую мажорную версию Unity — пересборка манифеста и регрессия сборки.  
**Синхронизация со спеками:** обновлены `spec/technical_architecture_specification.md` (§1.3), `spec/COLONY_CONQUEST_MASTER_SPECIFICATION.md` (§5.1) и этот ADR; канон версии редактора — `game/Client/ProjectSettings/ProjectVersion.txt`, `game/Client/README.md`.
