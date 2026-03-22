using Unity.Collections;
using Unity.Entities;

namespace ColonyConquest.Ecology
{
    /// <summary>Маркер полноценной runtime-симуляции экологии.</summary>
    public struct EcologySimulationSingleton : IComponentData
    {
    }

    /// <summary>Сводное состояние экологии, климата и устойчивого развития.</summary>
    public struct EcologySimulationState : IComponentData
    {
        public uint LastProcessedDay;

        public float AirPollutionUnitsPerDay;
        public float WaterPollutionUnitsPerDay;
        public float SoilContaminationUnitsPerDay;

        public float GreenhouseGasIndex;
        public float TemperatureAnomalyC;
        public float SeaLevelRiseMeters;
        public float ExtremeWeatherRisk01;

        public float EcosystemHealth01;
        public float SustainableDevelopment01;

        public uint EcologicalEventsTotal;
        public uint CatastrophesTotal;
    }

    /// <summary>Уровни внедрения мер защиты окружающей среды (§4).</summary>
    public struct EcologyMitigationState : IComponentData
    {
        /// <summary>0 none, 1 trees, 2 filters, 3 electrofilters, 4 clean-energy switch.</summary>
        public byte AirCleanupLevel;

        /// <summary>0 none, 1 septic, 2 treatment, 3 biological, 4 membrane.</summary>
        public byte WaterCleanupLevel;

        /// <summary>0 none, 1 composting, 2 sideration, 3 phytoremediation, 4 nanoremediation.</summary>
        public byte SoilRestorationLevel;

        /// <summary>0..1, интенсивность лесовосстановления.</summary>
        public float ReforestationIntensity01;

        /// <summary>0..1, интенсивность защиты животных/заповедников.</summary>
        public float WildlifeProtection01;

        /// <summary>0..1, доля улавливания CO2.</summary>
        public float CarbonCapture01;

        /// <summary>Флаг геоинженерии эпохи 5.</summary>
        public byte GeoengineeringEnabled;
    }

    /// <summary>Источник загрязнения воздуха (§2.1).</summary>
    public struct EcologyAirSourceEntry : IBufferElementData
    {
        public EcologyAirPollutionSourceId SourceId;
        public ushort Count;
        public float ActiveHoursPerDay;
        public float Utilization01;
    }

    /// <summary>Источник загрязнения воды (§2.2).</summary>
    public struct EcologyWaterSourceEntry : IBufferElementData
    {
        public EcologyWaterPollutionSourceId SourceId;
        public ushort Count;
        public float ActiveHoursPerDay;
        public float SpillChancePerDay01;
    }

    /// <summary>Факторы деградации/восстановления почвы (§2.3, §4.3).</summary>
    public struct EcologySoilSourceEntry : IBufferElementData
    {
        public EcologySoilImpactSourceId SourceId;
        public float Intensity01;
    }

    /// <summary>Последний зафиксированный тип катастрофы для UI/событий.</summary>
    public struct EcologyDisasterState : IComponentData
    {
        public byte LastDisasterType;
        public uint LastDisasterDay;
    }
}
