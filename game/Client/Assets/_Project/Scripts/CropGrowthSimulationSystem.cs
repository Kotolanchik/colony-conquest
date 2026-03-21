using ColonyConquest.Agriculture;
using ColonyConquest.Ecology;
using ColonyConquest.Economy;
using ColonyConquest.Simulation;
using Unity.Entities;

namespace ColonyConquest.Core
{
    /// <summary>Цикл §1.2 и сбор урожая по формуле §1.3 <c>spec/agriculture_mining_spec.md</c>; <c>spec/ecology_spec.md</c> §3.2 — множитель урожая от полосы загрязнения.</summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(CropCareDailySystem))]
    [UpdateAfter(typeof(EcologyPollutionSummarySystem))]
    public partial struct CropGrowthSimulationSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            ulong now = SystemAPI.GetSingleton<SimulationRootState>().SimulationTick;
            var stockBuf = SystemAPI.GetSingletonBuffer<ResourceStockEntry>();
            var dayIndex = SystemAPI.GetSingleton<GameCalendarState>().DayIndex;

            foreach (var plotRw in SystemAPI.Query<RefRW<CropPlotRuntime>>().WithAll<CropPlotTag>())
            {
                ref var p = ref plotRw.ValueRW;
                if (p.PhaseStartTick == 0)
                {
                    p.PhaseStartTick = now;
                    continue;
                }

                var need = CropGrowthTuning.TicksForPhase(p.Phase);
                if (now - p.PhaseStartTick < need)
                    continue;

                if (p.Phase == CropGrowthPhase.Harvest)
                {
                    var baseY = CropCatalog.GetBaseYieldPerTile(p.Crop);
                    var fertBonus = FertilizerTuning.GetYieldBonus(p.ActiveFertilizer);
                    var pest = p.PestDamage;
                    if (p.ActiveFertilizer == FertilizerKindId.Pesticides)
                        pest *= 0.2f;

                    var weed = p.WeedPressure01;
                    if (p.ActiveFertilizer == FertilizerKindId.Herbicides)
                        weed *= 0.1f;

                    var cropDef = CropCatalog.Get(p.Crop);
                    var season = SeasonCycle.GetSeasonFromDayIndex(dayIndex);
                    var seasonal = CropYieldMath.SeasonalModifier(season, cropDef.PreferredSeason);
                    var waterMul = CropWaterTuning.GetYieldMultiplier(p.WaterSupply);

                    var amount = CropYieldMath.ComputeHarvest(
                        baseY,
                        p.SoilFertility,
                        fertBonus,
                        p.FarmerSkillLevel,
                        p.WeatherModifier,
                        seasonal,
                        pest,
                        weed,
                        waterMul);

                    amount *= CropCatalog.GetNutritionMultiplier(p.Crop);

                    if (SystemAPI.HasSingleton<ColonyPollutionSummaryState>())
                        amount *= EcologyPollutionMath.GetCropYieldMultiplier(SystemAPI.GetSingleton<ColonyPollutionSummaryState>().Band);

                    if (CropKindResource.TryGetHarvestResource(p.Crop, out var res))
                        ResourceStockpileOps.Add(ref stockBuf, res, amount);

                    p.Phase = CropGrowthPhase.Preparation;
                    p.PhaseStartTick = now;
                }
                else
                {
                    var next = (CropGrowthPhase)((byte)p.Phase + 1);
                    p.Phase = next;
                    p.PhaseStartTick = now;
                    if (next == CropGrowthPhase.Growth)
                        p.LastCareGameDayIndex = uint.MaxValue;
                }
            }
        }
    }
}
