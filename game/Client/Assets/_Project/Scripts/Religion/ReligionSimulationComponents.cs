using Unity.Entities;

namespace ColonyConquest.Religion
{
    /// <summary>Сводные показатели веры в колонии (без индивидуальной симуляции каждого поселенца).</summary>
    public struct ReligionSimulationState : IComponentData
    {
        public uint LastProcessedDay;
        public float FaithLevelAvg;
        public float FanaticismAvg;
        public float DoubtAvg;

        public float TempleVisit;
        public float SermonExposure;
        public float PersonalCrisis;
        public float CommunityCohesion;
        public float EducationPressure;
        public float ProsperitySecularPull;
    }

    /// <summary>Параметры межконфессионального напряжения и радикализации.</summary>
    public struct ReligiousConflictState : IComponentData
    {
        public float IncidentWeight;
        public float IdeologyDistance;
        public float ResourceCompetition;
        public float PropagandaPressure;
        public float LawEnforcement;
        public float DialoguePrograms;
        public float TensionScore;
    }

    /// <summary>Активность деструктивных культов.</summary>
    public struct CultActivityState : IComponentData
    {
        public float RecruitmentRate;
        public float HiddenCells;
        public float RadicalizationRisk;
    }

    /// <summary>Стадии подготовки/ведения священной войны.</summary>
    public enum HolyWarPhase : byte
    {
        None = 0,
        TriggerDetected = 1,
        DoctrineApproval = 2,
        UltimatumPhase = 3,
        Mobilization = 4,
        HolyWarActive = 5,
        Resolution = 6
    }

    /// <summary>Состояние машинки священной войны для одной фракции.</summary>
    public struct HolyWarState : IComponentData
    {
        public HolyWarPhase Phase;
        public byte DayInPhase;
        public float Preparedness;
        public uint TargetFaction;
        public byte CasusBelli;
        public ushort ActiveDaysRemaining;
    }

    /// <summary>Политические веса религиозного поведения AI-фракции.</summary>
    public struct ReligionFactionPolicyState : IComponentData
    {
        public ReligionArchetypeId Archetype;
        public float MissionaryWeight;
        public float ToleranceWeight;
        public float CoercionWeight;
        public float SecularizationWeight;
    }
}
