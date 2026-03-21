# Бэклог и привязка к спекам

Цель: у **каждой** карточки в `dev/kanban.json` в поле **`spec_refs`** перечислены все релевантные документы из `spec/`, чтобы при реализации ничего не «выпало» из дизайна.

## Правила

| Правило | Описание |
|--------|----------|
| Минимум | Всегда хотя бы один файл: `spec/COLONY_CONQUEST_MASTER_SPECIFICATION.md` **или** доменная спека **или** `spec/technical_architecture_specification.md`. |
| Интеграция | Если задача касается связи систем — добавить **мастер** (§4) и **техспеку**. |
| Код без смены дизайна | Всё равно указать спеку-источник требований (что реализуется «по мотивам»). |
| Чистый процесс | Исключения: карточки `labels: ["process"]` без изменения `spec/` — `spec_refs` можно не задавать (редко). |
| Законченность | Карточка в **done** только при готовом результате по цели и проверяемых шагах; см. `.cursor/rules/full-solutions-engineering.mdc`. |

## Быстрый выбор спек по типу задачи

| Тип работы | С чего начать `spec_refs` |
|------------|---------------------------|
| ECS, производительность, сеть, стек | `technical_architecture_specification.md` |
| Видение, интеграция систем | `COLONY_CONQUEST_MASTER_SPECIFICATION.md` |
| Поселенцы | `settler_simulation_system_spec.md` |
| Бой | `military_system_specification.md` |
| Экономика | `economic_system_specification.md` |
| Стройка / оборона / заводы / жильё | `construction_system_spec.md`, `defensive_structures_spec.md`, `manufacturing_plants_spec.md`, `comfort_housing_spec.md` |
| Техи / агро / био | `technology_tree_spec.md`, `agriculture_mining_spec.md`, `plant_breeding_spec.md`, `bioengineering_spec.md` |
| Карта / дипломатия / политика / религия | `global_map_spec.md`, `diplomacy_trade_spec.md`, `political_system_spec.md`, `religion_cults_spec.md` |
| События / криминал / досуг / экология | `events_quests_spec.md`, `crime_justice_spec.md`, `entertainment_spec.md`, `ecology_spec.md` |
| UI / ввод / звук / метрики | `ui_ux_spec.md`, `audio_design_spec.md`, `statistics_analytics_spec.md` |

Полный список файлов — `spec/README.md` и `dev/spec_index.md`.
