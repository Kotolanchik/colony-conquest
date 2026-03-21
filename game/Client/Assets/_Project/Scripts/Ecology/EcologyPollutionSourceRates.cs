using Unity.Burst;

namespace ColonyConquest.Ecology
{
    /// <summary>
    /// Номинальные интенсивности §2.1–2.2 <c>spec/ecology_spec.md</c> (для будущих симуляторов источников).
    /// Непрерывные источники — единицы загрязнения на игровой час; разливы — за событие.
    /// </summary>
    [BurstCompile]
    public static class EcologyPollutionSourceRates
    {
        public static float GetAirPollutionUnitsPerGameHour(EcologyAirPollutionSourceId id)
        {
            return id switch
            {
                EcologyAirPollutionSourceId.CampfireOrFurnace => 1f,
                EcologyAirPollutionSourceId.BlastFurnace => 10f,
                EcologyAirPollutionSourceId.SteamEngine => 5f,
                EcologyAirPollutionSourceId.CoalFiredPowerPlant => 50f,
                EcologyAirPollutionSourceId.OilRefinery => 30f,
                EcologyAirPollutionSourceId.ChemicalPlant => 40f,
                EcologyAirPollutionSourceId.NuclearPowerPlant => 0f,
                EcologyAirPollutionSourceId.ThermonuclearReactor => 0f,
                _ => 0f,
            };
        }

        public static float GetWaterPollutionUnitsPerGameHour(EcologyWaterPollutionSourceId id)
        {
            return id switch
            {
                EcologyWaterPollutionSourceId.UntreatedSewage => 10f,
                EcologyWaterPollutionSourceId.MineDrainage => 20f,
                EcologyWaterPollutionSourceId.IndustrialEffluent => 50f,
                EcologyWaterPollutionSourceId.OilSpill => 0f,
                EcologyWaterPollutionSourceId.RadioactiveEffluent => 0f,
                _ => 0f,
            };
        }

        public static float GetWaterPollutionUnitsPerSpillEvent(EcologyWaterPollutionSourceId id)
        {
            return id switch
            {
                EcologyWaterPollutionSourceId.OilSpill => 100f,
                EcologyWaterPollutionSourceId.RadioactiveEffluent => 500f,
                _ => 0f,
            };
        }
    }
}
