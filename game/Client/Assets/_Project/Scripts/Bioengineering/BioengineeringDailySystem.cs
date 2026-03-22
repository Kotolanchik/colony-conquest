using ColonyConquest.Analytics;
using ColonyConquest.Core;
using ColonyConquest.Simulation;
using ColonyConquest.Story;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace ColonyConquest.Bioengineering
{
    /// <summary>Суточный контур биоинженерии: процедуры, эффекты стимуляторов, зависимость и детокс.</summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(ColonyCalendarAdvanceSystem))]
    public partial struct BioengineeringDailySystem : ISystem
    {
        private const uint EventProcedureSuccess = 0xE501;
        private const uint EventProcedureFailure = 0xE502;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameCalendarState>();
            state.RequireForUpdate<SimulationRootState>();
            state.RequireForUpdate<BioengineeringSimulationSingleton>();
            state.RequireForUpdate<BioengineeringSimulationState>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var day = SystemAPI.GetSingleton<GameCalendarState>().DayIndex;
            ref var sim = ref SystemAPI.GetSingletonRW<BioengineeringSimulationState>().ValueRW;
            if (sim.LastProcessedDay == day)
                return;
            sim.LastProcessedDay = day;

            var tick = SystemAPI.GetSingleton<SimulationRootState>().SimulationTick;
            var patients = SystemAPI.GetSingletonBuffer<BioPatientEntry>(ref state);
            var procedures = SystemAPI.GetSingletonBuffer<BioengineeringProcedureEntry>(ref state);

            var completedToday = 0u;
            var failedToday = 0u;
            var activeStimulants = 0u;

            for (var p = 0; p < patients.Length; p++)
            {
                var patient = patients[p];
                UpdateStimulantState(ref patient);
                ApplyWithdrawalPenalty(ref patient);
                if (patient.ActiveStimulantKind != StimulantKindId.None)
                    activeStimulants++;
                patients[p] = patient;
            }

            for (var i = 0; i < procedures.Length; i++)
            {
                var procedure = procedures[i];
                if (procedure.IsCompleted != 0)
                    continue;

                if (procedure.BaseDurationDays <= 0f)
                    procedure.BaseDurationDays = BioengineeringSimulationMath.GetProcedureDurationDays(procedure);
                if (procedure.RemainingDays <= 0f)
                    procedure.RemainingDays = procedure.BaseDurationDays;

                procedure.RemainingDays = math.max(0f, procedure.RemainingDays - 1f);
                if (procedure.RemainingDays > 0f)
                {
                    procedures[i] = procedure;
                    continue;
                }

                var patientIndex = FindPatientIndex(ref patients, procedure.PatientId);
                if (patientIndex < 0)
                {
                    procedure.IsCompleted = 1;
                    procedures[i] = procedure;
                    continue;
                }

                var patient = patients[patientIndex];
                var successChance = BioengineeringSimulationMath.ComputeSuccessChance(procedure.Type,
                    sim.MedicalStaffSkill01, sim.FacilityQuality01, patient.Health01);

                var random = Unity.Mathematics.Random.CreateFromIndex(
                    math.max(1u, day * 977u + procedure.ProcedureId * 131u + procedure.PatientId * 31u));
                var isSuccess = random.NextFloat() <= successChance;

                if (isSuccess)
                {
                    ApplyProcedureSuccess(ref patient, in procedure);
                    sim.ProceduresCompletedTotal++;
                    completedToday++;
                    TryEnqueueStoryEvent(ref state, tick, EventProcedureSuccess, new FixedString64Bytes("bio-success"));
                }
                else
                {
                    patient.Health01 = math.max(0.05f, patient.Health01 - 0.12f);
                    patient.Stamina01 = math.max(0.05f, patient.Stamina01 - 0.08f);
                    sim.ProcedureFailuresTotal++;
                    failedToday++;
                    TryEnqueueStoryEvent(ref state, tick, EventProcedureFailure, new FixedString64Bytes("bio-failure"));
                }

                patients[patientIndex] = patient;
                procedure.IsCompleted = 1;
                procedures[i] = procedure;
            }

            var dependencyCases = 0u;
            var healthSum = 0f;
            for (var p = 0; p < patients.Length; p++)
            {
                var patient = patients[p];
                if (patient.DependencyLevel != StimulantDependencyLevel.None)
                    dependencyCases++;
                healthSum += patient.Health01;
                patients[p] = patient;
            }

            var avgHealth = patients.Length == 0 ? 1f : healthSum / patients.Length;
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.BioengineeringProceduresCompletedTotal,
                sim.ProceduresCompletedTotal);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.BioengineeringProcedureFailuresTotal,
                sim.ProcedureFailuresTotal);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.BioengineeringDependencyCases,
                dependencyCases);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.BioengineeringAveragePatientHealth01,
                avgHealth);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.BioengineeringActiveStimulantCases,
                activeStimulants);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.BioengineeringCompletedToday,
                completedToday);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.BioengineeringFailedToday, failedToday);
        }

        private static void UpdateStimulantState(ref BioPatientEntry patient)
        {
            if (patient.ActiveStimulantKind != StimulantKindId.None)
            {
                patient.ActiveStimulantMinutesRemaining = math.max(0f, patient.ActiveStimulantMinutesRemaining - 24f * 60f);
                if (patient.ActiveStimulantMinutesRemaining <= 1e-3f)
                {
                    var effect = StimulantEffectTuning.Get(patient.ActiveStimulantKind);
                    if (effect.AftermathHpDelta < 0f)
                        patient.Health01 = math.max(0.05f, patient.Health01 + effect.AftermathHpDelta / 100f);
                    patient.Accuracy01 = math.clamp(patient.Accuracy01 * effect.AftermathAccuracyMultiplier, 0.05f, 2f);
                    patient.Mobility01 = math.clamp(patient.Mobility01 * effect.AftermathMoveSpeedMultiplier, 0.05f, 2f);
                    patient.ActiveStimulantKind = StimulantKindId.None;
                }
            }

            patient.DependencyLevel = BioengineeringSimulationMath.GetDependencyLevel(patient.StimulantUsesRecent);
            if (patient.InDetox != 0)
            {
                if (patient.WithdrawalDaysRemaining > 0)
                    patient.WithdrawalDaysRemaining--;
                if (patient.WithdrawalDaysRemaining == 0)
                {
                    patient.InDetox = 0;
                    if (patient.DependencyLevel > StimulantDependencyLevel.None)
                    {
                        patient.DependencyLevel =
                            (StimulantDependencyLevel)((byte)patient.DependencyLevel - 1);
                    }
                    if (patient.StimulantUsesRecent > 0)
                        patient.StimulantUsesRecent--;
                }
            }
            else if (patient.DependencyLevel >= StimulantDependencyLevel.Medium && patient.ActiveStimulantKind == StimulantKindId.None)
            {
                if (patient.WithdrawalDaysRemaining < 1)
                    patient.WithdrawalDaysRemaining = 1;
            }
        }

        private static void ApplyWithdrawalPenalty(ref BioPatientEntry patient)
        {
            if (patient.WithdrawalDaysRemaining == 0 || patient.DependencyLevel == StimulantDependencyLevel.None)
                return;

            var mult = BioengineeringSimulationMath.GetWithdrawalMultiplier(patient.DependencyLevel);
            patient.Strength01 = math.max(0.05f, patient.Strength01 * mult);
            patient.Stamina01 = math.max(0.05f, patient.Stamina01 * mult);
            patient.Mobility01 = math.max(0.05f, patient.Mobility01 * mult);
        }

        private static int FindPatientIndex(ref DynamicBuffer<BioPatientEntry> patients, uint patientId)
        {
            for (var i = 0; i < patients.Length; i++)
            {
                if (patients[i].PatientId == patientId)
                    return i;
            }

            return -1;
        }

        private static void ApplyProcedureSuccess(ref BioPatientEntry patient, in BioengineeringProcedureEntry procedure)
        {
            switch (procedure.Type)
            {
                case BioengineeringProcedureType.ProsthesisInstallation:
                    ApplyProsthesis(ref patient, procedure.ProsthesisKind);
                    break;
                case BioengineeringProcedureType.StimulantAdministration:
                    ApplyStimulant(ref patient, procedure.StimulantKind);
                    break;
                case BioengineeringProcedureType.GeneTherapy:
                    ApplyGeneTherapy(ref patient, procedure.GeneTherapyKind);
                    break;
                case BioengineeringProcedureType.Cloning:
                    ApplyCloning(ref patient, procedure.CloningKind);
                    break;
                case BioengineeringProcedureType.NeuroInterface:
                    ApplyNeuroInterface(ref patient, procedure.NeuroInterfaceKind);
                    break;
                case BioengineeringProcedureType.Detoxification:
                    patient.InDetox = 1;
                    patient.WithdrawalDaysRemaining = 10;
                    break;
            }
        }

        private static void ApplyProsthesis(ref BioPatientEntry patient, CyberneticProsthesisKindId prosthesisKind)
        {
            if (prosthesisKind is CyberneticProsthesisKindId.Hook
                or CyberneticProsthesisKindId.MechanicalArm
                or CyberneticProsthesisKindId.BionicArm
                or CyberneticProsthesisKindId.CombatProsthesis)
            {
                patient.ArmProsthesis = prosthesisKind;
                patient.Strength01 = math.clamp(patient.Strength01 + ProsthesisEfficiencyMath.GetArmEfficiency01(prosthesisKind) * 0.25f,
                    0.05f, 2f);
            }

            if (prosthesisKind is CyberneticProsthesisKindId.WoodenLeg
                or CyberneticProsthesisKindId.MechanicalLeg
                or CyberneticProsthesisKindId.BionicLeg)
            {
                patient.LegProsthesis = prosthesisKind;
                patient.Mobility01 = math.clamp(patient.Mobility01 * ProsthesisEfficiencyMath.GetLegSpeedMultiplier(prosthesisKind),
                    0.05f, 2f);
            }

            if (prosthesisKind == CyberneticProsthesisKindId.ArtificialHeart)
                patient.HasArtificialHeart = 1;
            else if (prosthesisKind == CyberneticProsthesisKindId.ArtificialLungs)
                patient.HasArtificialLungs = 1;
            else if (prosthesisKind == CyberneticProsthesisKindId.CyberEyes)
                patient.HasCyberEyes = 1;
        }

        private static void ApplyStimulant(ref BioPatientEntry patient, StimulantKindId stimulantKind)
        {
            if (stimulantKind == StimulantKindId.None)
                return;

            var effect = StimulantEffectTuning.Get(stimulantKind);
            patient.ActiveStimulantKind = stimulantKind;
            patient.ActiveStimulantMinutesRemaining = math.max(5f, effect.DurationGameMinutes * 60f);
            patient.Health01 = math.clamp(patient.Health01 + effect.HpBonus / 100f, 0.05f, 2f);
            patient.Mobility01 = math.clamp(patient.Mobility01 * effect.MoveSpeedMultiplier, 0.05f, 2f);
            patient.Strength01 = math.clamp(patient.Strength01 * effect.StrengthMultiplier, 0.05f, 2f);
            patient.Stamina01 = math.clamp(patient.Stamina01 * effect.StaminaMultiplier, 0.05f, 2f);
            if (patient.StimulantUsesRecent < byte.MaxValue)
                patient.StimulantUsesRecent++;

            var prevLevel = patient.DependencyLevel;
            patient.DependencyLevel = BioengineeringSimulationMath.GetDependencyLevel(patient.StimulantUsesRecent);
            if (patient.DependencyLevel > prevLevel)
            {
                if (patient.WithdrawalDaysRemaining < 1)
                    patient.WithdrawalDaysRemaining = 1;
            }
        }

        private static void ApplyGeneTherapy(ref BioPatientEntry patient, GeneTherapyApplicationKindId kind)
        {
            switch (kind)
            {
                case GeneTherapyApplicationKindId.HereditaryDiseaseTreatment:
                    patient.Health01 = math.clamp(patient.Health01 + 0.20f, 0.05f, 2f);
                    break;
                case GeneTherapyApplicationKindId.LifespanExtension:
                    patient.Health01 = math.clamp(patient.Health01 + 0.10f, 0.05f, 2f);
                    patient.Stamina01 = math.clamp(patient.Stamina01 + 0.15f, 0.05f, 2f);
                    break;
                case GeneTherapyApplicationKindId.StatImprovement:
                    patient.Strength01 = math.clamp(patient.Strength01 + 0.12f, 0.05f, 2f);
                    patient.Mobility01 = math.clamp(patient.Mobility01 + 0.12f, 0.05f, 2f);
                    break;
                case GeneTherapyApplicationKindId.OrganRegeneration:
                    patient.Health01 = math.clamp(patient.Health01 + 0.25f, 0.05f, 2f);
                    patient.HasArtificialHeart = 0;
                    patient.HasArtificialLungs = 0;
                    break;
            }
        }

        private static void ApplyCloning(ref BioPatientEntry patient, CloningProcedureKindId kind)
        {
            switch (kind)
            {
                case CloningProcedureKindId.TherapeuticOrganGrowth:
                    patient.Health01 = math.clamp(patient.Health01 + 0.20f, 0.05f, 2f);
                    break;
                case CloningProcedureKindId.RegenerativeLimbCloning:
                    patient.Mobility01 = math.clamp(patient.Mobility01 + 0.20f, 0.05f, 2f);
                    patient.Strength01 = math.clamp(patient.Strength01 + 0.10f, 0.05f, 2f);
                    break;
                case CloningProcedureKindId.ReproductiveAnimalCloning:
                    patient.Stamina01 = math.clamp(patient.Stamina01 + 0.05f, 0.05f, 2f);
                    break;
            }
        }

        private static void ApplyNeuroInterface(ref BioPatientEntry patient, NeuroInterfaceKindId kind)
        {
            patient.NeuroInterfaceKind = kind;
            var accBonus = kind switch
            {
                NeuroInterfaceKindId.NeuralinkBasic => 0.05f,
                NeuroInterfaceKindId.NeuralinkAdvanced => 0.10f,
                NeuroInterfaceKindId.CombatNeuralink => 0.20f,
                _ => 0f
            };
            var mobilityBonus = kind == NeuroInterfaceKindId.CombatNeuralink ? 0.12f : 0.04f;
            patient.Accuracy01 = math.clamp(patient.Accuracy01 + accBonus, 0.05f, 2f);
            patient.Mobility01 = math.clamp(patient.Mobility01 + mobilityBonus, 0.05f, 2f);
        }

        private static void TryEnqueueStoryEvent(ref SystemState state, uint tick, uint eventDefinitionId,
            in FixedString64Bytes label)
        {
            if (!SystemAPI.HasSingleton<StoryEventQueueSingleton>())
                return;

            var queue = SystemAPI.GetSingletonBuffer<GameEventQueueEntry>(ref state);
            queue.Add(new GameEventQueueEntry
            {
                Kind = StoryEventKind.Triggered,
                EventDefinitionId = eventDefinitionId,
                EnqueueSimulationTick = tick,
                DebugLabel = label
            });
        }
    }
}
