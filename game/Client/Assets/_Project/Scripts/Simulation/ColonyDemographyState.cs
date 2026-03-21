using Unity.Entities;

namespace ColonyConquest.Simulation
{
    /// <summary>
    /// Базовые демографические величины колонии; детальная симуляция рождений/смертей — settlers.
    /// </summary>
    public struct ColonyDemographyState : IComponentData
    {
        public uint Population;
        public uint BirthsThisYear;
        public uint DeathsThisYear;
        public uint LastProcessedYearIndex;
    }
}
