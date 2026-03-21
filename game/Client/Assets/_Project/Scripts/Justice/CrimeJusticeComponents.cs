using Unity.Entities;

namespace ColonyConquest.Justice
{
    /// <summary>Маркер сущности преступности/правосудия.</summary>
    public struct CrimeJusticeSingleton : IComponentData
    {
    }

    /// <summary>Сводное состояние правопорядка.</summary>
    public struct CrimeJusticeState : IComponentData
    {
        public uint LastProcessedDay;
        public float CrimeLevelPercent;
        public float PovertyPercent;
        public float UnemploymentPercent;
        public float InequalityGini;
        public float IlliteracyPercent;
        public float PoliceCoveragePercent;
        public float CorruptionLevel;
        public float OvercrowdingThousands;
        public float EntertainmentAccess01;
        public float ReligiousPopulationPercent;
        public float PenaltySeverity;
    }

    /// <summary>Состояние полиции и раскрываемости.</summary>
    public struct PoliceForceState : IComponentData
    {
        public float BaseEfficiency;
        public float SkillLevel;
        public float EquipmentLevel;
        public float OfficersPerPopulation;
    }

    /// <summary>Параметры судебной системы и рецидива.</summary>
    public struct JusticeCourtState : IComponentData
    {
        public byte CourtType; // 0 народный, 1 профессиональный, 2 присяжные, 3 военный, 4 религиозный
        public float Fairness01;
        public float Corruption01;
        public float PrisonConditions01;
        public float RehabPrograms01;
        public float OrganizedCrimePressure01;
    }

    /// <summary>Текущая статистика инцидентов и наказаний.</summary>
    public struct CrimeIncidentStatsState : IComponentData
    {
        public ushort IncidentsToday;
        public ushort SolvedToday;
        public ushort InmatesCount;
        public ushort RecidivistsCount;
        public ushort DeathPenaltyCount;
    }
}
