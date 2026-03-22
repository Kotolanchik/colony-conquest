using Unity.Entities;

namespace ColonyConquest.Entertainment
{
    /// <summary>Маркер сущности развлечений.</summary>
    public struct EntertainmentSimulationSingleton : IComponentData
    {
    }

    /// <summary>Сводные параметры досуга колонии.</summary>
    public struct EntertainmentSimulationState : IComponentData
    {
        public uint LastProcessedDay;
        public float BaseMood;
        public float Diversity;
        public float Quality;
        public float Availability;
        public float FinalMood;
        public float ProductivityModifier;
        public float StressReduction;
        public float GamblingRisk;
        public byte GamblingPolicy; // 0 banned, 1 legal, 2 state monopoly
    }

    /// <summary>Состояние праздников и культурного расписания.</summary>
    public struct EntertainmentFestivalState : IComponentData
    {
        public uint LastFestivalDay;
        public float HolidayMoodBonus;
    }
}
