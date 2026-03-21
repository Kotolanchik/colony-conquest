namespace ColonyConquest.Analytics
{
    /// <summary>
    /// Стабильные идентификаторы метрик для <see cref="AnalyticsRecordEntry.MetricId"/> и телеметрии.
    /// Покрытие разделов <c>spec/statistics_analytics_spec.md</c> §2–8.
    /// </summary>
    public static class AnalyticsMetricIds
    {
        // --- §2 Демография (0x1nnn) ---
        public const uint DemographyPopulation = 0x1001;
        public const uint DemographyBirthRatePer1000 = 0x1002;
        public const uint DemographyDeathRatePer1000 = 0x1003;
        public const uint DemographyNaturalGrowth = 0x1004;
        public const uint DemographyImmigration = 0x1005;
        public const uint DemographyEmigration = 0x1006;
        public const uint DemographyLifeExpectancy = 0x1007;

        // --- §3 Экономика (0x2nnn) ---
        public const uint EconomyGdp = 0x2001;
        public const uint EconomyGdpPerCapita = 0x2002;
        public const uint EconomyGdpGrowthPercent = 0x2003;
        public const uint EconomyInflation = 0x2004;
        public const uint EconomyUnemployment = 0x2005;
        public const uint EconomyExportVolume = 0x2006;
        public const uint EconomyImportVolume = 0x2007;
        public const uint EconomyTradeBalance = 0x2008;
        public const uint EconomyPrimarySectorShare = 0x2009;
        public const uint EconomySecondarySectorShare = 0x200A;
        public const uint EconomyTertiarySectorShare = 0x200B;

        // --- §4 Военная статистика (0x3nnn) ---
        public const uint MilitaryActiveArmy = 0x3001;
        public const uint MilitaryReserve = 0x3002;
        public const uint MilitaryDraftAgePool = 0x3003;
        public const uint MilitaryBudgetPercentGdp = 0x3004;
        public const uint MilitaryBattlesTotal = 0x3005;
        public const uint MilitaryBattlesWon = 0x3006;
        public const uint MilitaryBattlesLost = 0x3007;
        public const uint MilitaryBattlesDraw = 0x3008;
        public const uint MilitaryCasualtiesFriendlyKilled = 0x3009;
        public const uint MilitaryCasualtiesFriendlyWounded = 0x300A;
        public const uint MilitaryCasualtiesFriendlyMia = 0x300B;
        public const uint MilitaryEquipmentDestroyedFriendly = 0x300C;
        public const uint MilitaryCasualtiesEnemyKilled = 0x300D;
        public const uint MilitaryEnemyEquipmentDestroyed = 0x300E;
        public const uint MilitaryTerritoryCapturedKm2 = 0x300F;

        // --- §5 Технологии (0x4nnn) ---
        public const uint TechResearchPointsPerDay = 0x4001;
        public const uint TechTechnologiesUnlocked = 0x4002;
        public const uint TechCurrentEraProgress01 = 0x4003;
        public const uint TechResearchInstitutions = 0x4004;
        public const uint TechScientistsCount = 0x4005;

        // --- §6 Социальные показатели и ИЧР (0x5nnn) ---
        public const uint SocialHappiness01 = 0x5001;
        public const uint SocialHealth01 = 0x5002;
        public const uint SocialEducation01 = 0x5003;
        public const uint SocialSecurity01 = 0x5004;
        public const uint SocialEcology01 = 0x5005;
        public const uint SocialHumanDevelopmentIndex = 0x5006;

        // --- §7 Глобальный рейтинг (0x6nnn) ---
        public const uint GlobalRankPopulation = 0x6001;
        public const uint GlobalRankGdp = 0x6002;
        public const uint GlobalRankMilitary = 0x6003;
        public const uint GlobalRankTechnology = 0x6004;

        // --- §8 Достижения (0x7nnn) ---
        public const uint AchievementsUnlockedTotal = 0x7001;

        // --- Интеграция / события / UI (0x8nnn–0x9nnn) ---
        public const uint IntegrationStoryEventBase = 0x8000;
        public const uint IntegrationConstructionModeActive = 0x8101;

        // --- Селекция растений (0x82nn) ---
        public const uint PlantBreedingCyclesTotal = 0x8201;
        public const uint PlantBreedingSuccessTotal = 0x8202;
        public const uint PlantBreedingGmoIncidentTotal = 0x8203;
        public const uint PlantBreedingLineStabilityAvg = 0x8204;
        public const uint PlantBreedingMutationPositiveShare = 0x8205;

        // --- Религия и культы (0x83nn) ---
        public const uint ReligionFaithLevelAvg = 0x8301;
        public const uint ReligionConversionEventsTotal = 0x8302;
        public const uint ReligionTensionAvg = 0x8303;
        public const uint ReligionHolyWarsStartedTotal = 0x8304;
        public const uint ReligionCultRadicalizationCases = 0x8305;

        // --- Жильё и комфорт (0x84nn) ---
        public const uint HousingCapacityTotal = 0x8401;
        public const uint HousingOccupancyRatio = 0x8402;
        public const uint HousingComfortScoreAvg = 0x8403;
        public const uint HousingIncidentsTotal = 0x8404;
        public const uint HousingOvercrowdingHouseholds = 0x8405;
    }
}
