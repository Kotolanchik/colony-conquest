using Unity.Collections;
using Unity.Entities;

namespace ColonyConquest.Bioengineering
{
    /// <summary>Создаёт singleton биоинженерии с демо-пациентами и процедурами.</summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct BioengineeringBootstrapSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            if (SystemAPI.HasSingleton<BioengineeringSimulationSingleton>())
                return;

            var em = state.EntityManager;
            var entity = em.CreateEntity();
            em.AddComponent<BioengineeringSimulationSingleton>(entity);
            em.AddComponent(entity, new BioengineeringSimulationState
            {
                LastProcessedDay = uint.MaxValue,
                LastProcedureId = 3,
                ProceduresCompletedTotal = 0,
                ProcedureFailuresTotal = 0,
                MedicalStaffSkill01 = 0.62f,
                FacilityQuality01 = 0.58f
            });

            var patients = em.AddBuffer<BioPatientEntry>(entity);
            patients.Add(new BioPatientEntry
            {
                PatientId = 1,
                Health01 = 0.62f,
                Stamina01 = 0.55f,
                Strength01 = 0.60f,
                Mobility01 = 0.35f,
                Accuracy01 = 0.58f,
                ArmProsthesis = CyberneticProsthesisKindId.None,
                LegProsthesis = CyberneticProsthesisKindId.WoodenLeg,
                HasArtificialHeart = 0,
                HasArtificialLungs = 0,
                HasCyberEyes = 0,
                NeuroInterfaceKind = NeuroInterfaceKindId.None,
                DependencyLevel = StimulantDependencyLevel.None,
                StimulantUsesRecent = 0,
                ActiveStimulantKind = StimulantKindId.None,
                ActiveStimulantMinutesRemaining = 0f,
                WithdrawalDaysRemaining = 0,
                InDetox = 0
            });
            patients.Add(new BioPatientEntry
            {
                PatientId = 2,
                Health01 = 0.75f,
                Stamina01 = 0.68f,
                Strength01 = 0.64f,
                Mobility01 = 0.72f,
                Accuracy01 = 0.63f,
                ArmProsthesis = CyberneticProsthesisKindId.Hook,
                LegProsthesis = CyberneticProsthesisKindId.None,
                HasArtificialHeart = 0,
                HasArtificialLungs = 0,
                HasCyberEyes = 0,
                NeuroInterfaceKind = NeuroInterfaceKindId.None,
                DependencyLevel = StimulantDependencyLevel.Light,
                StimulantUsesRecent = 3,
                ActiveStimulantKind = StimulantKindId.None,
                ActiveStimulantMinutesRemaining = 0f,
                WithdrawalDaysRemaining = 1,
                InDetox = 0
            });
            patients.Add(new BioPatientEntry
            {
                PatientId = 3,
                Health01 = 0.81f,
                Stamina01 = 0.74f,
                Strength01 = 0.70f,
                Mobility01 = 0.77f,
                Accuracy01 = 0.66f,
                ArmProsthesis = CyberneticProsthesisKindId.None,
                LegProsthesis = CyberneticProsthesisKindId.None,
                HasArtificialHeart = 0,
                HasArtificialLungs = 0,
                HasCyberEyes = 0,
                NeuroInterfaceKind = NeuroInterfaceKindId.NeuralinkBasic,
                DependencyLevel = StimulantDependencyLevel.None,
                StimulantUsesRecent = 0,
                ActiveStimulantKind = StimulantKindId.None,
                ActiveStimulantMinutesRemaining = 0f,
                WithdrawalDaysRemaining = 0,
                InDetox = 0
            });

            var procedures = em.AddBuffer<BioengineeringProcedureEntry>(entity);
            procedures.Add(new BioengineeringProcedureEntry
            {
                ProcedureId = 1,
                PatientId = 1,
                Type = BioengineeringProcedureType.ProsthesisInstallation,
                ProsthesisKind = CyberneticProsthesisKindId.MechanicalLeg,
                StimulantKind = StimulantKindId.None,
                GeneTherapyKind = GeneTherapyApplicationKindId.None,
                CloningKind = CloningProcedureKindId.None,
                NeuroInterfaceKind = NeuroInterfaceKindId.None,
                BaseDurationDays = 12f,
                RemainingDays = 6f,
                IsCompleted = 0,
                DebugName = new FixedString64Bytes("install-mech-leg")
            });
            procedures.Add(new BioengineeringProcedureEntry
            {
                ProcedureId = 2,
                PatientId = 2,
                Type = BioengineeringProcedureType.GeneTherapy,
                ProsthesisKind = CyberneticProsthesisKindId.None,
                StimulantKind = StimulantKindId.None,
                GeneTherapyKind = GeneTherapyApplicationKindId.StatImprovement,
                CloningKind = CloningProcedureKindId.None,
                NeuroInterfaceKind = NeuroInterfaceKindId.None,
                BaseDurationDays = 14f,
                RemainingDays = 4f,
                IsCompleted = 0,
                DebugName = new FixedString64Bytes("gene-stat-up")
            });
            procedures.Add(new BioengineeringProcedureEntry
            {
                ProcedureId = 3,
                PatientId = 3,
                Type = BioengineeringProcedureType.StimulantAdministration,
                ProsthesisKind = CyberneticProsthesisKindId.None,
                StimulantKind = StimulantKindId.Buffout,
                GeneTherapyKind = GeneTherapyApplicationKindId.None,
                CloningKind = CloningProcedureKindId.None,
                NeuroInterfaceKind = NeuroInterfaceKindId.None,
                BaseDurationDays = 1f,
                RemainingDays = 1f,
                IsCompleted = 0,
                DebugName = new FixedString64Bytes("buffout-course")
            });
        }
    }
}
