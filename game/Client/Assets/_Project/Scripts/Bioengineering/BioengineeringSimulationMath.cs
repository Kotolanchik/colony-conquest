using Unity.Burst;
using Unity.Mathematics;

namespace ColonyConquest.Bioengineering
{
    /// <summary>Формулы вероятностей и длительностей процедур из <c>spec/bioengineering_spec.md</c>.</summary>
    [BurstCompile]
    public static class BioengineeringSimulationMath
    {
        public static float GetProcedureDurationDays(in BioengineeringProcedureEntry entry)
        {
            if (entry.BaseDurationDays > 0f)
                return entry.BaseDurationDays;

            return entry.Type switch
            {
                BioengineeringProcedureType.ProsthesisInstallation => 14f,
                BioengineeringProcedureType.StimulantAdministration => 1f,
                BioengineeringProcedureType.GeneTherapy => entry.GeneTherapyKind switch
                {
                    GeneTherapyApplicationKindId.HereditaryDiseaseTreatment => 30f,
                    GeneTherapyApplicationKindId.LifespanExtension => 60f,
                    GeneTherapyApplicationKindId.StatImprovement => 14f,
                    GeneTherapyApplicationKindId.OrganRegeneration => 45f,
                    _ => 30f
                },
                BioengineeringProcedureType.Cloning => entry.CloningKind switch
                {
                    CloningProcedureKindId.TherapeuticOrganGrowth => 30f,
                    CloningProcedureKindId.ReproductiveAnimalCloning => 60f,
                    CloningProcedureKindId.RegenerativeLimbCloning => 45f,
                    _ => 30f
                },
                BioengineeringProcedureType.NeuroInterface => entry.NeuroInterfaceKind switch
                {
                    NeuroInterfaceKindId.NeuralinkBasic => 7f,
                    NeuroInterfaceKindId.NeuralinkAdvanced => 14f,
                    NeuroInterfaceKindId.CombatNeuralink => 21f,
                    _ => 10f
                },
                BioengineeringProcedureType.Detoxification => 10f,
                _ => 14f
            };
        }

        public static float ComputeSuccessChance(
            BioengineeringProcedureType type,
            float staffSkill01,
            float facilityQuality01,
            float patientHealth01)
        {
            var baseChance = type switch
            {
                BioengineeringProcedureType.ProsthesisInstallation => 0.88f,
                BioengineeringProcedureType.StimulantAdministration => 0.98f,
                BioengineeringProcedureType.GeneTherapy => 0.73f,
                BioengineeringProcedureType.Cloning => 0.85f,
                BioengineeringProcedureType.NeuroInterface => 0.80f,
                BioengineeringProcedureType.Detoxification => 0.90f,
                _ => 0.75f
            };

            var skill = math.clamp(staffSkill01, 0f, 1f);
            var quality = math.clamp(facilityQuality01, 0f, 1f);
            var health = math.clamp(patientHealth01, 0f, 1f);
            var result = baseChance + skill * 0.15f + quality * 0.10f + (health - 0.5f) * 0.05f;
            return math.clamp(result, 0.05f, 0.99f);
        }

        public static StimulantDependencyLevel GetDependencyLevel(byte stimulantUsesRecent)
        {
            if (stimulantUsesRecent >= 8)
                return StimulantDependencyLevel.Heavy;
            if (stimulantUsesRecent >= 4)
                return StimulantDependencyLevel.Medium;
            if (stimulantUsesRecent >= 1)
                return StimulantDependencyLevel.Light;
            return StimulantDependencyLevel.None;
        }

        public static float GetWithdrawalMultiplier(StimulantDependencyLevel level)
        {
            return level switch
            {
                StimulantDependencyLevel.Light => 0.95f,
                StimulantDependencyLevel.Medium => 0.85f,
                StimulantDependencyLevel.Heavy => 0.70f,
                _ => 1f
            };
        }
    }
}
