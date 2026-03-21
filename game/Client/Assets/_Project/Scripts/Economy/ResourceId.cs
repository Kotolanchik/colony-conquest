namespace ColonyConquest.Economy
{
    /// <summary>
    /// Стабильные идентификаторы ресурсов из §1.2 <c>spec/economic_system_specification.md</c> (полный список по эпохам).
    /// Порядок значений фиксирован для сохранений и сети; новые ресурсы добавлять только в конец с новой версией данных.
    /// </summary>
    public enum ResourceId : byte
    {
        None = 0,

        // --- Эпоха 1: Основание ---
        /// <summary>Железная руда</summary>
        IronOre = 1,
        /// <summary>Медная руда</summary>
        CopperOre = 2,
        /// <summary>Камень</summary>
        Stone = 3,
        /// <summary>Древесина</summary>
        Wood = 4,
        /// <summary>Уголь</summary>
        Coal = 5,
        /// <summary>Золотая руда</summary>
        GoldOre = 6,
        /// <summary>Железный слиток</summary>
        IronIngot = 7,
        /// <summary>Медный слиток</summary>
        CopperIngot = 8,
        /// <summary>Золотой слиток</summary>
        GoldIngot = 9,
        /// <summary>Доски</summary>
        Planks = 10,
        /// <summary>Кирпич</summary>
        Brick = 11,
        /// <summary>Угольный кокс</summary>
        CoalCoke = 12,
        /// <summary>Сталь (базовая)</summary>
        SteelBasic = 13,
        /// <summary>Порох</summary>
        Gunpowder = 14,
        /// <summary>Ткань</summary>
        Cloth = 15,
        /// <summary>Кожа</summary>
        Leather = 16,

        // --- Эпоха 2: Индустриализация ---
        /// <summary>Нефть</summary>
        Oil = 17,
        /// <summary>Оловянная руда</summary>
        TinOre = 18,
        /// <summary>Свинцовая руда</summary>
        LeadOre = 19,
        /// <summary>Никелевая руда</summary>
        NickelOre = 20,
        /// <summary>Резина (натуральная)</summary>
        NaturalRubber = 21,
        /// <summary>Сталь (промышленная)</summary>
        SteelIndustrial = 22,
        /// <summary>Чугун</summary>
        CastIron = 23,
        /// <summary>Латунь</summary>
        Brass = 24,
        /// <summary>Бронза</summary>
        Bronze = 25,
        /// <summary>Нефтепродукты</summary>
        PetroleumProducts = 26,
        /// <summary>Динамит</summary>
        Dynamite = 27,
        /// <summary>Синтетический порох</summary>
        SyntheticGunpowder = 28,

        // --- Эпоха 3: Первая мировая ---
        /// <summary>Вольфрамовая руда</summary>
        TungstenOre = 29,
        /// <summary>Хромитовая руда</summary>
        ChromiteOre = 30,
        /// <summary>Молибденовая руда</summary>
        MolybdenumOre = 31,
        /// <summary>Сталь (легированная)</summary>
        SteelAlloyed = 32,
        /// <summary>Сталь (бронебойная)</summary>
        SteelArmorPiercing = 33,
        /// <summary>Алюминий</summary>
        Aluminum = 34,
        /// <summary>Синтетическая резина</summary>
        SyntheticRubber = 35,
        /// <summary>Химические реагенты</summary>
        ChemicalReagents = 36,
        /// <summary>Отравляющие вещества</summary>
        PoisonGas = 37,

        // --- Эпоха 4: Вторая мировая ---
        /// <summary>Урановая руда</summary>
        UraniumOre = 38,
        /// <summary>Титановая руда</summary>
        TitaniumOre = 39,
        /// <summary>Марганцевая руда</summary>
        ManganeseOre = 40,
        /// <summary>Пластик (бакелит)</summary>
        PlasticBakelite = 41,
        /// <summary>Пластик (современный)</summary>
        PlasticModern = 42,
        /// <summary>Синтетическое топливо</summary>
        SyntheticFuel = 43,
        /// <summary>Высокооктановый бензин</summary>
        HighOctaneGasoline = 44,
        /// <summary>Композитные материалы</summary>
        CompositeMaterials = 45,
        /// <summary>Ядерный материал</summary>
        NuclearMaterial = 46,

        // --- Эпоха 5: Современность / будущее ---
        /// <summary>Редкоземельные металлы</summary>
        RareEarthMetals = 47,
        /// <summary>Платиновая группа</summary>
        PlatinumGroup = 48,
        /// <summary>Кремний (чистый)</summary>
        PureSilicon = 49,
        /// <summary>Графен</summary>
        Graphene = 50,
        /// <summary>Углеродное волокно</summary>
        CarbonFiber = 51,
        /// <summary>Керамические композиты</summary>
        CeramicComposites = 52,
        /// <summary>Суперсплавы</summary>
        Superalloys = 53,
        /// <summary>Квантовые материалы</summary>
        QuantumMaterials = 54,
        /// <summary>Антиматерия</summary>
        Antimatter = 55,

        // --- Сельхоз / добыча: <c>spec/agriculture_mining_spec.md</c> §1.1, §1.4 (после промышленного блока, см. ADR при смене порядка) ---
        /// <summary>Пшеница (урожай)</summary>
        CropWheat = 56,
        /// <summary>Ячмень</summary>
        CropBarley = 57,
        /// <summary>Овёс</summary>
        CropOat = 58,
        /// <summary>Рожь</summary>
        CropRye = 59,
        /// <summary>Кукуруза</summary>
        CropCorn = 60,
        /// <summary>Картофель</summary>
        CropPotato = 61,
        /// <summary>Овощи</summary>
        CropVegetables = 62,
        /// <summary>Фрукты</summary>
        CropFruits = 63,
        /// <summary>Яйца</summary>
        LivestockEggs = 64,
        /// <summary>Молоко</summary>
        LivestockMilk = 65,
        /// <summary>Шерсть</summary>
        LivestockWool = 66,
        /// <summary>Сырое мясо</summary>
        LivestockMeat = 67,

        // --- Доп. сырьё и изделия эпохи 1 (цепочки §2.2.1–2.2.4, значения только в конце) ---
        /// <summary>Сера (сырьё, порох и химия)</summary>
        Sulfur = 68,
        /// <summary>Селитра (сырьё)</summary>
        Saltpeter = 69,
        /// <summary>Инструменты (базовые, эпоха 1)</summary>
        Epoch1Tools = 70,
        /// <summary>Мушкет (готовое оружие эпохи 1)</summary>
        MusketFirearm = 71,
        /// <summary>Пика (готовое оружие эпохи 1)</summary>
        PikeWeapon = 72,

        // --- Добыча agriculture_mining_spec §2.1 (после цепочек эпохи 1) ---
        /// <summary>Глина (сырьё)</summary>
        RawClay = 73,
        /// <summary>Песок</summary>
        Sand = 74,
        /// <summary>Серебряная руда</summary>
        SilverOre = 75,
        /// <summary>Алмазы (сырьё)</summary>
        RawDiamonds = 76,
        /// <summary>Гелий-3</summary>
        Helium3 = 77,

        // --- Промежуточные продукты эпохи 2 (экономика §2.3) ---
        /// <summary>Негашёная известь (для конвертера и химии)</summary>
        Quicklime = 78,
        /// <summary>Стальной прокат (эпоха 2)</summary>
        SteelRolledPlate = 79,

        /// <summary>Рыба (улов; §2.3 возобновление запасов водоёмов)</summary>
        FishCatch = 80,

        /// <summary>Каменное ядро (артиллерия эпохи 1)</summary>
        StoneRoundShot = 81,
        /// <summary>Бронзовая пушка (эпоха 1)</summary>
        BronzeCannonEpoch1 = 82,

        /// <summary>Калиброванный стальной прокат (эпоха 2) — §2.3.1</summary>
        CalibratedSteelPlate = 83,
        /// <summary>Военная униформа (эпоха 1) — §2.2.3</summary>
        MilitaryUniformEpoch1 = 84,
        /// <summary>Фанера (эпоха 1) — §2.2.2</summary>
        PlywoodEpoch1 = 85,

        // --- Военное производство эпохи 2 — §2.3.4 ---
        /// <summary>Винтовка затворная (эпоха 2)</summary>
        MilitaryRifleEpoch2 = 86,
        /// <summary>Револьвер (эпоха 2)</summary>
        RevolverEpoch2 = 87,
        /// <summary>Фугасный артиллерийский снаряд (эпоха 2)</summary>
        ArtilleryShellExplosiveEpoch2 = 88,

        /// <summary>Серебряный слиток (переработка)</summary>
        SilverIngot = 89,
        /// <summary>Ручная граната (эпоха 3) — §2.4.3</summary>
        HandGrenadeWW1 = 90,
        /// <summary>Противопехотная мина (эпоха 3) — §2.4.3</summary>
        LandMineWW1 = 91,

        /// <summary>Химический артиллерийский снаряд (эпоха 3) — §2.4.3</summary>
        ChemicalArtilleryShellWW1 = 92,

        /// <summary>Винтовка с магазином (эпоха 3) — §2.4.3</summary>
        MilitaryRifleMagazineWW1 = 93,
    }
}
