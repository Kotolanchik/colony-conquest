# Задача: `spec-economy-production-workshop` — Производство §2.1–2.4 экономической спеки

## Цель

Реализовать **формулу эффективности производства** §2.1, **рецепты эпох 1–3** (включая §2.2–2.4) из `spec/economic_system_specification.md` в виде данных и системы **демо-цеха**, работающей со складом `ResourceStockpileSingleton`.

## Спецификации

- [x] `spec/economic_system_specification.md` — §2.1–2.4 (в т.ч. легированные стали эпохи 3 §2.4.1)
- [x] `spec/COLONY_CONQUEST_MASTER_SPECIFICATION.md` — контекст экономики

## Связанные ADR

- нет

## Критерии готовности (Definition of Done)

- [x] `ProductionEfficiencyMath.ComputeSpeedMultiplier` — множители §2.1 (рабочие, энергия, инструменты, мастерство, износ).
- [x] `ProductionRecipeId`, `ProductionRecipeDefinition`, `ProductionRecipeCatalog` — эпоха 1 (плавка Fe/Cu/Au, доски, кокс, кирпич, сталь, инструменты, ткань, кожа, порох, мушкет, пика) и эпоха 2 (домен, конвертер, прокат, НПЗ, динамит, известь); время и коэффициенты по §2.2–2.3.
- [x] `ProductionRecipeCatalog.SelectionPriorityOrder` — выбор рецепта: сначала готовая продукция и верх цепочек, затем полуфабрикаты, затем плавки (без «застревания» на одной плавке при полном сырье).
- [x] `ResourceId` 68…72: сера, селитра, базовые инструменты, мушкет, пика; 78–79: известь, стальной прокат; строки в `ResourceCatalog`.
- [x] §2.3: доменная плавка → чугун; конвертер (известь) → `SteelIndustrial`; прокат → `SteelRolledPlate`; **калибровка** 4 проката → 6 `CalibratedSteelPlate` (40 с); НПЗ (10 нефти → 10 нефтепродуктов, упрощённо); динамит (масштаб 5:10:5 → 2:4:2); обжиг извести из камня.
- [x] §2.2.2–2.2.3: фанера (2 доски + инструмент); униформа (ткань + инструмент); ресурсы `PlywoodEpoch1`, `MilitaryUniformEpoch1`.
- [x] §2.3.4: винтовка (`CraftBoltActionRifleEpoch2`), револьвер (`CraftRevolverEpoch2`), фугасный снаряд (`CraftExplosiveShellEpoch2`); ресурсы 86–88.
- [x] §2.4: плавка серебра, граната, мина (`SmeltSilverOre`, `CraftHandGrenadeWW1`, `CraftLandMineWW1`) — строки в `ProductionRecipeCatalog`.
- [x] §2.3.3 синтетическая резина (`CraftSyntheticRubberEpoch2`); §2.4.3 химснаряд (`CraftChemicalArtilleryShellWW1`, ресурс `ChemicalArtilleryShellWW1`); сид `ChemicalReagents`.
- [x] §2.5.1 бакелит (`CraftPlasticBakeliteEpoch4`); §2.4.3 винтовка с магазином (`CraftMagazineRifleWW1`, `MilitaryRifleMagazineWW1`); демо-переработка урана (`SmeltUraniumToNuclearMaterial`); сид `UraniumOre`.
- [x] §2.4.1: `ElectricFurnaceArmorPiercing` (5 `SteelIndustrial` + `TungstenOre` → `SteelArmorPiercing`); `ElectricFurnaceAlloyedSteel` (5 `SteelIndustrial` + `ChromiteOre` + `NickelOre` → `SteelAlloyed`).

- [x] §2.2.4: каменное ядро (2 камня); бронзовая пушка (6 бронзы + 4 дерева, 600 с); сплав бронзы: 2 `CopperIngot` + 1 `TinOre` → `Bronze`; `ResourceId` 81–82.
- [x] `ResourceStockpileOps`: `GetAmount`, `HasAtLeast`, `TryConsume`, `TryConsumeRecipe` (до трёх входов).
- [x] `EconomyWorkshopRuntime` + `EconomyWorkshopProductionSystem` — прогресс цикла, списание/выдача на склад; `AssignedWorkers = 4` в демо (ближе к §2.3 по числу рабочих на этапах).
- [x] Bootstrap: сид руды (Fe/Cu/Au), камня, угля, дерева, серы, селитры, пшеницы, мяса, **нефти**, **кокса**; миграция мира без цеха.
- [x] Проверка: Play — на складе появляются продукты цепочек при достаточных входах; сборка без ошибок.

## Заметки / журнал

- Здания в сцене, логистика, энергосеть, отдельные здания под каждый рецепт — отдельные задачи; здесь один виртуальный цех.
- «Ткань»: вход `CropWheat` как абстракция хлопка/льна по таблице §2.2.3; «кожа»: `LivestockMeat` + уголь как упрощение дубления.
- «Кокс»: 2 угля → 1 кокс — упрощение относительно полной цепочки коксования в спеке.
- НПЗ: один агрегированный выход `PetroleumProducts` вместо бензин/керосин/… по §2.3.2; энергия кВт в коде не моделируется.
- 2026-03-24 — Рецепты §2.3.1 калибровка (`CalibrateSteelPlate` → `CalibratedSteelPlate`), §2.2.2 фанера (`CraftPlywoodEpoch1`), §2.2.3 униформа (`SewUniformEpoch1`); сид склада для проверки цепочек.
- 2026-03-24 — §2.3.4 оружие/снаряды эпохи 2; сид `SteelIndustrial` / `Dynamite`.
- 2026-03-21 — §2.5.1 бакелит, §2.4.3 винтовка с магазином, демо-плавка урана → `NuclearMaterial`; сид `UraniumOre`.
- 2026-03-21 — Демо-цех superseded полной реализацией экономики: `dev/tasks/spec-economy-full-implementation.md`.
