namespace ColonyConquest.Economy
{
    /// <summary>Рецепты производства — <c>spec/economic_system_specification.md</c> §2.2–2.5 (демо-набор эпох 1–4).</summary>
    public enum ProductionRecipeId : byte
    {
        None = 0,

        /// <summary>Плавка: 2 железной руды + 1 уголь → 1 слиток, 30 с.</summary>
        SmeltIronOre = 1,
        /// <summary>Кузница: 3 слитка железа + 2 угля → 1 сталь, 45 с. (значение 2 — стабильно для сохранений)</summary>
        ProduceSteelBasic = 2,
        /// <summary>Пилорама: 3 дерева → 2 доски, 20 с. (значение 3 — стабильно для сохранений)</summary>
        SawPlanks = 3,

        /// <summary>Плавка: 2 медной руды + 1 уголь → 1 слиток, 30 с.</summary>
        SmeltCopperOre = 4,
        /// <summary>Плавка: 2 золотой руды + 1 уголь → 1 слиток, 30 с.</summary>
        SmeltGoldOre = 5,
        /// <summary>Коксование: 2 угля → 1 кокс (упрощённо), 45 с.</summary>
        CokeCoal = 6,
        /// <summary>Обжиг: 2 камня + 1 уголь → 1 кирпич, 40 с.</summary>
        BakeBrick = 7,
        /// <summary>Кузница: 1 сталь + 1 дерево → 1 инструмент, 60 с.</summary>
        ForgeEpoch1Tools = 8,
        /// <summary>Ткачество: 4 пшеницы (лен/хлопок) → 2 ткани, 40 с.</summary>
        WeaveClothFromWheat = 9,
        /// <summary>Дубление: 2 мяса + 1 уголь → 1 кожа (абстракция дубления), 60 с.</summary>
        TanLeather = 10,
        /// <summary>Порох: 1 сера + 2 угля + 1 селитра → 1 порох, 90 с. §2.2.4</summary>
        CraftGunpowder = 11,
        /// <summary>Мушкет: 2 стали + 1 дерево → 1 мушкет, 180 с. §2.2.4</summary>
        CraftMusket = 12,
        /// <summary>Пика: 1 сталь + 1 дерево → 1 пика, 60 с. §2.2.4</summary>
        CraftPike = 13,

        // --- Эпоха 2: §2.3 ---
        /// <summary>Доменная плавка: 5 руды + 3 кокса → 3 чугуна, 60 с. §2.3.1</summary>
        BlastFurnaceCastIron = 14,
        /// <summary>Конвертер: 3 чугуна + 1 известь → 2 промышленной стали, 45 с. §2.3.1</summary>
        BessemerSteelIndustrial = 15,
        /// <summary>Прокат: 2 стали → 4 проката, 30 с. §2.3.1</summary>
        RollingMillSteelPlate = 16,
        /// <summary>НПЗ: 10 нефти → 10 нефтепродуктов (смесь), 120 с. §2.3.2 (упрощённо)</summary>
        RefineryCrudeToPetroleum = 17,
        /// <summary>Химзавод: 2 серы + 4 угля + 2 селитры → 2 динамита, 90 с. §2.3.3 (масштаб от 5:10:5)</summary>
        CraftDynamiteIndustrial = 18,
        /// <summary>Обжиг: 2 камня + 1 уголь → 1 известь, 40 с. (сырьё для конвертера)</summary>
        CalcinateQuicklime = 19,

        /// <summary>Сплав: 2 медных слитка + 1 оловянная руда → 1 бронза (Cu+Sn), 60 с. §1.2 эпоха 2 сырьё</summary>
        SmeltBronzeAlloy = 20,
        /// <summary>Ядро: 2 камня → 1 каменное ядро, 30 с. §2.2.4</summary>
        CraftStoneRoundShot = 21,
        /// <summary>Пушка: 6 бронзы + 4 дерева → 1 пушка, 600 с. §2.2.4</summary>
        CraftBronzeCannon = 22,

        /// <summary>Калибровка: 4 проката → 6 калиброванных, 40 с. §2.3.1</summary>
        CalibrateSteelPlate = 23,
        /// <summary>Швейная: 1 ткань + 1 инструмент → 1 униформа, 60 с. §2.2.3</summary>
        SewUniformEpoch1 = 24,
        /// <summary>Столярная: 2 доски + 1 инструмент → 1 фанера, 30 с. §2.2.2</summary>
        CraftPlywoodEpoch1 = 25,

        /// <summary>Винтовка: 3 пром. стали + 2 дерева → 1 винтовка, 240 с. §2.3.4</summary>
        CraftBoltActionRifleEpoch2 = 26,
        /// <summary>Револьвер: 2 пром. стали + 1 дерево → 1 револьвер, 180 с. §2.3.4</summary>
        CraftRevolverEpoch2 = 27,
        /// <summary>Фугасный снаряд: 2 пром. стали + 1 динамит → 1 снаряд, 120 с. §2.3.4</summary>
        CraftExplosiveShellEpoch2 = 28,

        // --- Эпоха 3: §2.4.1 легированные стали ---
        /// <summary>Электропечь: 5 пром. стали + 1 вольфрамовая руда → 4 бронебойной стали, 90 с.</summary>
        ElectricFurnaceArmorPiercing = 29,
        /// <summary>Электропечь: 5 пром. стали + 1 хромит + 1 никелевая руда → 4 легированной стали, 120 с.</summary>
        ElectricFurnaceAlloyedSteel = 30,

        /// <summary>Плавка: 2 серебряной руды + 1 уголь → 1 серебряный слиток, 30 с.</summary>
        SmeltSilverOre = 31,
        /// <summary>Граната: 2 пром. стали + 1 динамит → 2 гранаты, 60 с. §2.4.3 (масштаб от 1+0.5)</summary>
        CraftHandGrenadeWW1 = 32,
        /// <summary>Мина: 2 пром. стали + 1 динамит → 1 мина, 120 с. §2.4.3</summary>
        CraftLandMineWW1 = 33,

        /// <summary>Химзавод: 5 нефти + 1 катализатор (химреагенты) → 2 синтетической резины, 60 с. §2.3.3</summary>
        CraftSyntheticRubberEpoch2 = 34,
        /// <summary>Химснаряд: 2 пром. стали + 2 химреагента → 1 снаряд, 180 с. §2.4.3</summary>
        CraftChemicalArtilleryShellWW1 = 35,

        /// <summary>Химзавод (упрощ.): 8 нефти + 2 химреагента → 4 бакелита, 180 с. §2.5.1</summary>
        CraftPlasticBakeliteEpoch4 = 36,
        /// <summary>Винтовка с магазином: 4 пром. стали + 2 дерева → 1, 300 с. §2.4.3</summary>
        CraftMagazineRifleWW1 = 37,
        /// <summary>Переработка: 3 урановой руды + 2 угля → 1 ядерного материала, 120 с. (демо)</summary>
        SmeltUraniumToNuclearMaterial = 38,
    }
}
