using Unity.Entities;

namespace ColonyConquest.Analytics
{
    /// <summary>
    /// Агрегированный снимок локальной статистики поселения — структура полей по
    /// <c>spec/statistics_analytics_spec.md</c> §2–8 (значения обновляет <see cref="AnalyticsSnapshotUpdateSystem"/>).
    /// </summary>
    public struct AnalyticsLocalSnapshot : IComponentData
    {
        public AnalyticsDemographySnapshot Demography;
        public AnalyticsEconomySnapshot Economy;
        public AnalyticsMilitarySnapshot Military;
        public AnalyticsTechnologySnapshot Technology;
        public AnalyticsSocialSnapshot Social;
        public AnalyticsGlobalSnapshot Global;
        public AnalyticsAchievementsSnapshot Achievements;
    }

    /// <summary>§2 Демография.</summary>
    public struct AnalyticsDemographySnapshot
    {
        public float Population;
        public float BirthRatePer1000;
        public float DeathRatePer1000;
        public float NaturalGrowthPer1000;
        public float ImmigrationPerYear;
        public float EmigrationPerYear;
        public float LifeExpectancyYears;
    }

    /// <summary>§3 Экономика.</summary>
    public struct AnalyticsEconomySnapshot
    {
        public float Gdp;
        public float GdpPrevious;
        public float GdpPerCapita;
        public float GdpGrowthPercent;
        public float InflationPercent;
        public float UnemploymentRate01;
        public float ExportVolume;
        public float ImportVolume;
        public float TradeBalance;
        public float PrimarySectorShare01;
        public float SecondarySectorShare01;
        public float TertiarySectorShare01;
    }

    /// <summary>§4 Военная статистика.</summary>
    public struct AnalyticsMilitarySnapshot
    {
        public float ActiveArmy;
        public float Reserve;
        public float DraftAgePool;
        public float MilitaryBudgetPercentGdp;
        public float BattlesTotal;
        public float BattlesWon;
        public float BattlesLost;
        public float BattlesDraw;
        public float CasualtiesFriendlyKilled;
        public float CasualtiesFriendlyWounded;
        public float CasualtiesFriendlyMia;
        public float EquipmentDestroyedFriendly;
        public float CasualtiesEnemyKilled;
        public float EnemyEquipmentDestroyed;
        public float TerritoryCapturedKm2;
    }

    /// <summary>§5 Технологии.</summary>
    public struct AnalyticsTechnologySnapshot
    {
        public float ResearchPointsPerDay;
        public float TechnologiesUnlocked;
        public float CurrentEraProgress01;
        public float ResearchInstitutions;
        public float ScientistsCount;
        public float ResearchPointsAccumulated;
    }

    /// <summary>§6 Социальные показатели; ИЧР — §6.2, диапазон 0–1.</summary>
    public struct AnalyticsSocialSnapshot
    {
        public float Happiness01;
        public float Health01;
        public float Education01;
        public float Security01;
        public float Ecology01;
        public float HumanDevelopmentIndex;
    }

    /// <summary>§7 Глобальный рейтинг (в одиночной сессии места 1).</summary>
    public struct AnalyticsGlobalSnapshot
    {
        public uint RankByPopulation;
        public uint RankByGdp;
        public uint RankByMilitaryPower;
        public uint RankByTechnology;
    }

    /// <summary>§8 Достижения.</summary>
    public struct AnalyticsAchievementsSnapshot
    {
        public uint UnlockedTotal;
    }
}
