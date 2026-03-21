using Unity.Entities;

namespace ColonyConquest.Ecology
{
    /// <summary>
    /// Текущие значения индикаторов §1.2 <c>spec/ecology_spec.md</c> (0 = плохо, 1 = хорошо).
    /// Нагрузка агрохимии влияет на индикаторы ежедневно (система моста в Core).
    /// </summary>
    public struct ColonyEcologyIndicatorsState : IComponentData
    {
        public float AirQuality01;
        public float WaterQuality01;
        public float SoilFertilityIndicator01;
        public float ForestCover01;
        public float Biodiversity01;
        public uint LastAgrochemicalBridgeDayIndex;
    }
}
