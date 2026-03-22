using ColonyConquest.Core;
using ColonyConquest.Economy;
using ColonyConquest.Ecology;
using ColonyConquest.Military;
using ColonyConquest.Settlers;
using ColonyConquest.Simulation;
using Unity.Entities;
using Unity.Mathematics;

namespace ColonyConquest.Analytics
{
    /// <summary>
    /// Заполняет <see cref="AnalyticsLocalSnapshot"/> из данных мира; ИЧР по формуле §6.2
    /// <c>spec/statistics_analytics_spec.md</c> (три компонента 0–1, среднее).
    /// </summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(ColonyCalendarAdvanceSystem))]
    public partial struct AnalyticsSnapshotUpdateSystem : ISystem
    {
        private EntityQuery _battleUnitsQuery;

        public void OnCreate(ref SystemState state)
        {
            _battleUnitsQuery = state.GetEntityQuery(ComponentType.ReadOnly<BattleUnitTag>());
            state.RequireForUpdate<AnalyticsLocalSnapshot>();
            state.RequireForUpdate<SimulationRootState>();
            state.RequireForUpdate<ColonyDemographyState>();
            state.RequireForUpdate<ColonyTechProgressState>();
            state.RequireForUpdate<GameCalendarState>();
        }

        public void OnUpdate(ref SystemState state)
        {
            ref var demo = ref SystemAPI.GetSingletonRW<ColonyDemographyState>().ValueRW;
            var cal = SystemAPI.GetSingleton<GameCalendarState>();
            var tech = SystemAPI.GetSingleton<ColonyTechProgressState>();

            var yearIndex = cal.DayIndex / 365u;
            if (yearIndex != demo.LastProcessedYearIndex)
            {
                demo.BirthsThisYear = 0;
                demo.DeathsThisYear = 0;
                demo.LastProcessedYearIndex = yearIndex;
            }

            float population = math.max(1f, demo.Population);
            if (SystemAPI.HasSingleton<SettlerSimulationState>())
                population = math.max(1f, SystemAPI.GetSingleton<SettlerSimulationState>().PopulationAlive);

            float stockPrimary = 0f;
            float stockSecondary = 0f;
            float stockTertiary = 0f;

            if (SystemAPI.HasSingleton<ResourceStockpileSingleton>())
            {
                var stock = SystemAPI.GetSingletonBuffer<ResourceStockEntry>(ref state);
                for (var i = 0; i < stock.Length; i++)
                {
                    var entry = stock[i];
                    if (!ResourceCatalog.TryGet(entry.Id, out var def) || def.BasePrice <= 0)
                        continue;
                    var v = entry.Amount * def.BasePrice;
                    switch (def.Category)
                    {
                        case ResourceCategory.Raw:
                        case ResourceCategory.Material:
                            stockPrimary += v;
                            break;
                        case ResourceCategory.Processed:
                        case ResourceCategory.Component:
                            stockSecondary += v;
                            break;
                        case ResourceCategory.FinalProduct:
                            stockTertiary += v;
                            break;
                    }
                }
            }

            var totalSector = stockPrimary + stockSecondary + stockTertiary;
            float pShare = 0.33f;
            float sShare = 0.33f;
            float tShare = 0.34f;
            if (totalSector > 1e-3f)
            {
                pShare = stockPrimary / totalSector;
                sShare = stockSecondary / totalSector;
                tShare = stockTertiary / totalSector;
            }

            var gdp = totalSector;
            ref var snap = ref SystemAPI.GetSingletonRW<AnalyticsLocalSnapshot>().ValueRW;
            var prevGdp = snap.Economy.Gdp;
            snap.Economy.GdpPrevious = prevGdp;
            snap.Economy.Gdp = gdp;
            snap.Economy.GdpPerCapita = gdp / population;
            snap.Economy.GdpGrowthPercent = prevGdp > 1e-3f ? (gdp - prevGdp) / prevGdp * 100f : 0f;
            if (SystemAPI.HasSingleton<EconomySimulationState>())
            {
                var eco = SystemAPI.GetSingleton<EconomySimulationState>();
                snap.Economy.InflationPercent = eco.InflationPercent;
                snap.Economy.UnemploymentRate01 = eco.Unemployment01;
                snap.Economy.ExportVolume = eco.ExportVolume;
                snap.Economy.ImportVolume = eco.ImportVolume;
                snap.Economy.TradeBalance = eco.TradeBalance;
            }
            else
            {
                snap.Economy.InflationPercent = 0f;
                snap.Economy.UnemploymentRate01 = 0.05f;
                snap.Economy.ExportVolume = 0f;
                snap.Economy.ImportVolume = 0f;
                snap.Economy.TradeBalance = 0f;
            }
            snap.Economy.PrimarySectorShare01 = pShare;
            snap.Economy.SecondarySectorShare01 = sShare;
            snap.Economy.TertiarySectorShare01 = tShare;

            snap.Demography.Population = population;
            var br = demo.Population > 0 ? demo.BirthsThisYear / (float)demo.Population * 1000f : 0f;
            var dr = demo.Population > 0 ? demo.DeathsThisYear / (float)demo.Population * 1000f : 0f;
            snap.Demography.BirthRatePer1000 = br;
            snap.Demography.DeathRatePer1000 = dr;
            snap.Demography.NaturalGrowthPer1000 = br - dr;
            snap.Demography.ImmigrationPerYear = 0f;
            snap.Demography.EmigrationPerYear = 0f;
            snap.Demography.LifeExpectancyYears = 40f;

            var army = _battleUnitsQuery.CalculateEntityCount();
            snap.Military.ActiveArmy = army;
            snap.Military.Reserve = 0f;
            snap.Military.DraftAgePool = math.max(0f, population - army);
            snap.Military.MilitaryBudgetPercentGdp = gdp > 1e-3f ? 5f : 0f;
            snap.Military.BattlesTotal = 0f;
            snap.Military.BattlesWon = 0f;
            snap.Military.BattlesLost = 0f;
            snap.Military.BattlesDraw = 0f;
            snap.Military.CasualtiesFriendlyKilled = 0f;
            snap.Military.CasualtiesFriendlyWounded = 0f;
            snap.Military.CasualtiesFriendlyMia = 0f;
            snap.Military.EquipmentDestroyedFriendly = 0f;
            snap.Military.CasualtiesEnemyKilled = 0f;
            snap.Military.EnemyEquipmentDestroyed = 0f;
            snap.Military.TerritoryCapturedKm2 = 0f;

            snap.Technology.ResearchPointsPerDay = tech.ResearchPointsPerDay;
            snap.Technology.TechnologiesUnlocked = tech.TechnologiesUnlocked;
            snap.Technology.CurrentEraProgress01 = tech.CurrentEraProgress01;
            snap.Technology.ResearchInstitutions = tech.ResearchInstitutions;
            snap.Technology.ScientistsCount = tech.ScientistsCount;
            snap.Technology.ResearchPointsAccumulated = tech.ResearchPointsAccumulated;

            var happiness = 0.5f;
            var health = 0.55f;
            var education = 0.5f;
            if (SystemAPI.HasSingleton<SettlerSimulationState>())
            {
                var settlers = SystemAPI.GetSingleton<SettlerSimulationState>();
                happiness = math.saturate((settlers.AverageMood + 100f) / 200f);
                health = settlers.AverageHealth01;
                education = math.max(education, settlers.EducationIndex01);
            }
            var security = math.saturate((float)army / math.max(1f, population * 0.02f));
            var ecology = 0.5f;
            if (SystemAPI.HasSingleton<ColonyEcologyIndicatorsState>())
            {
                var e = SystemAPI.GetSingleton<ColonyEcologyIndicatorsState>();
                ecology = (e.AirQuality01 + e.WaterQuality01 + e.SoilFertilityIndicator01 + e.ForestCover01 +
                            e.Biodiversity01) * 0.2f;
            }
            var income01 = math.saturate(snap.Economy.GdpPerCapita / 2000f);
            snap.Social.Happiness01 = happiness;
            snap.Social.Health01 = health;
            snap.Social.Education01 = education;
            snap.Social.Security01 = security;
            snap.Social.Ecology01 = ecology;
            snap.Social.HumanDevelopmentIndex = (education + health + income01) / 3f;

            snap.Global.RankByPopulation = 1;
            snap.Global.RankByGdp = 1;
            snap.Global.RankByMilitaryPower = 1;
            snap.Global.RankByTechnology = 1;

            snap.Achievements.UnlockedTotal = 0;
        }
    }
}
