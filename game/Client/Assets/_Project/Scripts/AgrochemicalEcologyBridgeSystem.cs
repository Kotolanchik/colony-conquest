using ColonyConquest.Agriculture;
using ColonyConquest.Ecology;
using ColonyConquest.Simulation;
using Unity.Entities;
using Unity.Mathematics;

namespace ColonyConquest.Core
{
    /// <summary>
    /// Переносит <see cref="ColonyAgrochemicalLoadState.ChemicalLoad01"/> в индикаторы экологии §1.5 и §3 интеграции agriculture_mining ↔ ecology.
    /// </summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(CropCareDailySystem))]
    [UpdateAfter(typeof(ColonyCalendarAdvanceSystem))]
    public partial struct AgrochemicalEcologyBridgeSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ColonyEcologyIndicatorsState>();
            state.RequireForUpdate<ColonyAgrochemicalLoadState>();
            state.RequireForUpdate<GameCalendarState>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var day = SystemAPI.GetSingleton<GameCalendarState>().DayIndex;
            ref var eco = ref SystemAPI.GetSingletonRW<ColonyEcologyIndicatorsState>().ValueRW;
            if (eco.LastAgrochemicalBridgeDayIndex == day)
                return;

            eco.LastAgrochemicalBridgeDayIndex = day;
            var L = SystemAPI.GetSingleton<ColonyAgrochemicalLoadState>().ChemicalLoad01;

            eco.SoilFertilityIndicator01 = math.clamp(eco.SoilFertilityIndicator01 - 0.028f * L, 0.05f, 1f);
            eco.WaterQuality01 = math.clamp(eco.WaterQuality01 - 0.022f * L, 0.05f, 1f);
            eco.Biodiversity01 = math.clamp(eco.Biodiversity01 - 0.032f * L, 0.05f, 1f);
            eco.AirQuality01 = math.clamp(eco.AirQuality01 - 0.016f * L, 0.05f, 1f);
            eco.ForestCover01 = math.clamp(eco.ForestCover01 - 0.012f * L, 0.05f, 1f);

            const float recovery = 0.0022f;
            eco.SoilFertilityIndicator01 = math.min(1f, eco.SoilFertilityIndicator01 + recovery);
            eco.WaterQuality01 = math.min(1f, eco.WaterQuality01 + recovery);
            eco.Biodiversity01 = math.min(1f, eco.Biodiversity01 + recovery);
            eco.AirQuality01 = math.min(1f, eco.AirQuality01 + recovery);
            eco.ForestCover01 = math.min(1f, eco.ForestCover01 + recovery);
        }
    }
}
