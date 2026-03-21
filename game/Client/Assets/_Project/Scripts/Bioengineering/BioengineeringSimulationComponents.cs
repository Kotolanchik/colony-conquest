using Unity.Collections;
using Unity.Entities;

namespace ColonyConquest.Bioengineering
{
    /// <summary>Маркер сущности-симуляции биоинженерии.</summary>
    public struct BioengineeringSimulationSingleton : IComponentData
    {
    }

    /// <summary>Агрегированное состояние лаборатории биоинженерии.</summary>
    public struct BioengineeringSimulationState : IComponentData
    {
        public uint LastProcessedDay;
        public uint LastProcedureId;
        public uint ProceduresCompletedTotal;
        public uint ProcedureFailuresTotal;
        public float MedicalStaffSkill01;
        public float FacilityQuality01;
    }

    public enum BioengineeringProcedureType : byte
    {
        ProsthesisInstallation = 0,
        StimulantAdministration = 1,
        GeneTherapy = 2,
        Cloning = 3,
        NeuroInterface = 4,
        Detoxification = 5
    }

    /// <summary>Агрегированная запись пациента (демо-runtime без полноценной симуляции поселенца).</summary>
    public struct BioPatientEntry : IBufferElementData
    {
        public uint PatientId;
        public float Health01;
        public float Stamina01;
        public float Strength01;
        public float Mobility01;
        public float Accuracy01;

        public CyberneticProsthesisKindId ArmProsthesis;
        public CyberneticProsthesisKindId LegProsthesis;
        public byte HasArtificialHeart;
        public byte HasArtificialLungs;
        public byte HasCyberEyes;
        public NeuroInterfaceKindId NeuroInterfaceKind;

        public StimulantDependencyLevel DependencyLevel;
        public byte StimulantUsesRecent;
        public StimulantKindId ActiveStimulantKind;
        public float ActiveStimulantMinutesRemaining;
        public byte WithdrawalDaysRemaining;
        public byte InDetox;
    }

    /// <summary>Заявка на медицинскую/биотех-процедуру.</summary>
    public struct BioengineeringProcedureEntry : IBufferElementData
    {
        public uint ProcedureId;
        public uint PatientId;
        public BioengineeringProcedureType Type;

        public CyberneticProsthesisKindId ProsthesisKind;
        public StimulantKindId StimulantKind;
        public GeneTherapyApplicationKindId GeneTherapyKind;
        public CloningProcedureKindId CloningKind;
        public NeuroInterfaceKindId NeuroInterfaceKind;

        public float BaseDurationDays;
        public float RemainingDays;
        public byte IsCompleted;
        public FixedString64Bytes DebugName;
    }
}
