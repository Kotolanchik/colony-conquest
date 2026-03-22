using Unity.Mathematics;

namespace ColonyConquest.Justice
{
    /// <summary>Формулы преступности, эффективности полиции и рецидива.</summary>
    public static class CrimeJusticeMath
    {
        public static float ComputeCrimeLevelPercent(
            float povertyPercent,
            float unemploymentPercent,
            float inequalityGini,
            float illiteracyPercent,
            float policeCoveragePercent,
            float corruptionLevel,
            float overcrowdingThousands,
            float religiousPopulationPercent,
            float entertainmentAccess01,
            float penaltySeverity)
        {
            var level = 20f;
            level += povertyPercent * 0.10f;
            level += unemploymentPercent * 0.08f;
            level += inequalityGini * 18f;
            level += illiteracyPercent * 0.04f;
            level += math.max(0f, 100f - policeCoveragePercent) * 0.12f;
            level += corruptionLevel * 15f;
            level += overcrowdingThousands * 0.5f;

            level -= religiousPopulationPercent * 0.03f;
            level -= entertainmentAccess01 * 12f;
            level -= penaltySeverity * 8f;

            return math.clamp(level, 0f, 100f);
        }

        public static float ComputePoliceEfficiency(float baseEfficiency, float skillLevel, float equipmentLevel,
            float officersPerPopulation)
        {
            var result = baseEfficiency *
                         (1f + skillLevel * 0.1f) *
                         (1f + equipmentLevel * 0.2f) *
                         (1f + officersPerPopulation * 10f);
            return math.clamp(result, 0f, 5f);
        }

        public static float ComputeSolveChance(CrimeOffenseKindId offense, float policeEfficiency)
        {
            var baseChance = offense switch
            {
                CrimeOffenseKindId.PettyTheft => 0.35f,
                CrimeOffenseKindId.FoodTheft => 0.40f,
                CrimeOffenseKindId.PropertyDamage => 0.30f,
                CrimeOffenseKindId.Drunkenness => 0.60f,
                CrimeOffenseKindId.Robbery => 0.55f,
                CrimeOffenseKindId.AssaultGrievous => 0.60f,
                CrimeOffenseKindId.Arson => 0.50f,
                CrimeOffenseKindId.Murder => 0.75f,
                CrimeOffenseKindId.Treason => 0.80f,
                CrimeOffenseKindId.Espionage => 0.70f,
                CrimeOffenseKindId.Sabotage => 0.72f,
                _ => 0.4f
            };
            return math.clamp(baseChance * policeEfficiency, 0.01f, 0.98f);
        }

        public static float ComputeRecidivismChance(float penaltySeverity, float prisonConditions01, float rehabPrograms01,
            float organizedCrimePressure01)
        {
            var chance = 0.40f;
            chance -= penaltySeverity * 0.10f;
            chance -= prisonConditions01 * 0.10f;
            chance -= rehabPrograms01 * 0.20f;
            chance += organizedCrimePressure01 * 0.20f;
            return math.clamp(chance, 0.05f, 0.70f);
        }
    }
}
