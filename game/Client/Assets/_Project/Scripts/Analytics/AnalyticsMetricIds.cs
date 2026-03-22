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

        // --- Дипломатия и торговля (0x85nn) ---
        public const uint DiplomacyAverageRelations = 0x8501;
        public const uint DiplomacyTradeProfitDaily = 0x8502;
        public const uint DiplomacyActiveAlliances = 0x8503;
        public const uint DiplomacyWarsDeclaredTotal = 0x8504;

        // --- Преступность и правосудие (0x86nn) ---
        public const uint CrimeLevelPercent = 0x8601;
        public const uint CrimeIncidentsDaily = 0x8602;
        public const uint CrimeSolveRateDaily = 0x8603;
        public const uint CrimeInmatesCount = 0x8604;
        public const uint CrimeRecidivismCases = 0x8605;

        // --- Развлечения и досуг (0x87nn) ---
        public const uint EntertainmentMoodFinal = 0x8701;
        public const uint EntertainmentProductivityModifier = 0x8702;
        public const uint EntertainmentStressReduction = 0x8703;
        public const uint EntertainmentGamblingRisk = 0x8704;
        public const uint EntertainmentAvailability = 0x8705;

        // --- Дерево технологий (runtime) (0x88nn) ---
        public const uint TechActiveResearchId = 0x8801;
        public const uint TechResearchPoolPoints = 0x8802;
        public const uint TechEraTransitionsTotal = 0x8803;

        // --- Политическая система (0x89nn) ---
        public const uint PoliticsStability01 = 0x8901;
        public const uint PoliticsEconomyModifier = 0x8902;
        public const uint PoliticsHappinessModifier = 0x8903;
        public const uint PoliticsScienceModifier = 0x8904;
        public const uint PoliticsDefenseModifier = 0x8905;

        // --- Глобальная карта (0x8Ann) ---
        public const uint WorldMapDiscoveredChunks = 0x8A01;
        public const uint WorldMapControlledResourceNodes = 0x8A02;
        public const uint WorldMapActiveArmiesMoving = 0x8A03;
        public const uint WorldMapActiveScale = 0x8A04;

        // --- Строительство (runtime) (0x8Bnn) ---
        public const uint ConstructionProjectsBacklog = 0x8B01;
        public const uint ConstructionProjectsCompletedTotal = 0x8B02;
        public const uint ConstructionProjectsBlockedTotal = 0x8B03;
        public const uint ConstructionAverageProgress01 = 0x8B04;
        public const uint ConstructionCompletionsToday = 0x8B05;

        // --- Оборонительные сооружения (0x8Cnn) ---
        public const uint DefenseActiveStructuresCount = 0x8C01;
        public const uint DefenseAverageStructureHp01 = 0x8C02;
        public const uint DefenseStructuresBuiltTotal = 0x8C03;
        public const uint DefenseStructuresDestroyedTotal = 0x8C04;
        public const uint DefenseOrdersBlockedByEraTotal = 0x8C05;
        public const uint DefenseBuildsCompletedToday = 0x8C06;
        public const uint DefenseDestroyedToday = 0x8C07;

        // --- Биоинженерия (0x8Dnn) ---
        public const uint BioengineeringProceduresCompletedTotal = 0x8D01;
        public const uint BioengineeringProcedureFailuresTotal = 0x8D02;
        public const uint BioengineeringDependencyCases = 0x8D03;
        public const uint BioengineeringAveragePatientHealth01 = 0x8D04;
        public const uint BioengineeringActiveStimulantCases = 0x8D05;
        public const uint BioengineeringCompletedToday = 0x8D06;
        public const uint BioengineeringFailedToday = 0x8D07;

        // --- Экономика (full runtime) (0x8Enn) ---
        public const uint EconomyPowerGeneratedKw = 0x8E01;
        public const uint EconomyPowerDemandKw = 0x8E02;
        public const uint EconomyPowerLossPercent = 0x8E03;
        public const uint EconomyLogisticsCapacityTonKm = 0x8E04;
        public const uint EconomyLogisticsRequiredTonKm = 0x8E05;
        public const uint EconomyWarehouseUtilization01 = 0x8E06;
        public const uint EconomyMilitaryProductionShare01 = 0x8E07;
        public const uint EconomyArmySupplyAdequacy01 = 0x8E08;
        public const uint EconomyCurrentCyclePhase = 0x8E09;
        public const uint EconomyResearchPointsFromEconomy = 0x8E0A;
        public const uint EconomyActiveFacilities = 0x8E0B;

        // --- Симуляция поселенцев (0x8Fnn) ---
        public const uint SettlerPopulationAlive = 0x8F01;
        public const uint SettlerAverageMood = 0x8F02;
        public const uint SettlerAverageStress = 0x8F03;
        public const uint SettlerAverageHealth01 = 0x8F04;
        public const uint SettlerAverageWorkEfficiency01 = 0x8F05;
        public const uint SettlerColonyMorale01 = 0x8F06;
        public const uint SettlerFoodSatisfied01 = 0x8F07;
        public const uint SettlerMentalBreaksToday = 0x8F08;
        public const uint SettlerDeathsToday = 0x8F09;
        public const uint SettlerBirthsToday = 0x8F0A;
        public const uint SettlerHungryShare01 = 0x8F0B;
        public const uint SettlerExhaustedShare01 = 0x8F0C;
        public const uint SettlerInfectedShare01 = 0x8F0D;

        // --- Экология и загрязнение (full runtime) (0x90nn) ---
        public const uint EcologyAirQuality01 = 0x9001;
        public const uint EcologyWaterQuality01 = 0x9002;
        public const uint EcologySoilFertility01 = 0x9003;
        public const uint EcologyForestCover01 = 0x9004;
        public const uint EcologyBiodiversity01 = 0x9005;
        public const uint EcologyCombinedPollutionPercent = 0x9006;
        public const uint EcologyGreenhouseGasIndex = 0x9007;
        public const uint EcologyTemperatureAnomalyC = 0x9008;
        public const uint EcologyExtremeWeatherRisk01 = 0x9009;
        public const uint EcologySustainableDevelopment01 = 0x900A;
        public const uint EcologyEventsToday = 0x900B;

        // --- Производственные заводы (full runtime) (0x91nn) ---
        public const uint ManufacturingOrdersBacklog = 0x9101;
        public const uint ManufacturingOrdersCompletedTotal = 0x9102;
        public const uint ManufacturingOrdersBlockedResourcesTotal = 0x9103;
        public const uint ManufacturingOrdersBlockedEraTotal = 0x9104;
        public const uint ManufacturingMilitaryOutputToday = 0x9105;
        public const uint ManufacturingCivilianOutputToday = 0x9106;
        public const uint ManufacturingHeavyOutputToday = 0x9107;
        public const uint ManufacturingEnergyDemandKw = 0x9108;
        public const uint ManufacturingEnergySatisfied01 = 0x9109;
        public const uint ManufacturingPolicy = 0x910A;
        public const uint ManufacturingRetoolingPenalty01 = 0x910B;
        public const uint ManufacturingVirtualStockUnits = 0x910C;
    }
}
