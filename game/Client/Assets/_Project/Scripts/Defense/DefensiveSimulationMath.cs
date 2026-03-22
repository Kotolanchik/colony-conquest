using ColonyConquest.Technology;
using Unity.Burst;
using Unity.Mathematics;

namespace ColonyConquest.Defense
{
    /// <summary>Формулы строительства и живучести укреплений по <c>spec/defensive_structures_spec.md</c>.</summary>
    [BurstCompile]
    public static class DefensiveSimulationMath
    {
        public static float GetUnderFireModifier(byte underFireIntensity)
        {
            return underFireIntensity switch
            {
                1 => 0.7f,
                2 => 0.4f,
                3 => 0.1f,
                _ => 1f
            };
        }

        public static float ComputeBuildProgressPerDay(
            float baseBuildHours,
            float engineerSkillLevel,
            ushort engineers,
            byte underFireIntensity)
        {
            var baseSpeed = 24f / math.max(0.25f, baseBuildHours);
            var skillMultiplier = 1f + math.max(0f, engineerSkillLevel) * 0.1f;
            var workerMultiplier = 1f + math.pow(math.max((float)engineers, 1f), 0.7f);
            var fireMultiplier = GetUnderFireModifier(underFireIntensity);
            return baseSpeed * skillMultiplier * workerMultiplier * fireMultiplier;
        }

        public static bool TryGetBaseBuildHours(DefensiveStructureKindId kind, out float hours)
        {
            switch (kind)
            {
                case DefensiveStructureKindId.Trenches:
                    hours = 0.25f;
                    return true;
                case DefensiveStructureKindId.SandbagWall:
                    hours = 0.50f;
                    return true;
                case DefensiveStructureKindId.BarbedWire:
                    hours = 0.25f;
                    return true;
                case DefensiveStructureKindId.AntiTankHedgehogs:
                    hours = 0.33f;
                    return true;
                case DefensiveStructureKindId.Minefield:
                    hours = 0.50f;
                    return true;
                case DefensiveStructureKindId.Pillbox:
                    hours = 24f;
                    return true;
                case DefensiveStructureKindId.Bunker:
                    hours = 24f * 5f;
                    return true;
                case DefensiveStructureKindId.ConcreteRedoubt:
                    hours = 24f * 2f;
                    return true;
                case DefensiveStructureKindId.FortifiedPosition:
                    hours = 24f * 3f;
                    return true;
                case DefensiveStructureKindId.EnergyShield:
                    hours = 24f;
                    return true;
                case DefensiveStructureKindId.ForceField:
                    hours = 24f * 2f;
                    return true;
                case DefensiveStructureKindId.AutomatedTurret:
                    hours = 12f;
                    return true;
                default:
                    hours = 0f;
                    return false;
            }
        }

        public static bool IsTechEraAllowed(DefensiveStructureKindId kind, TechEraId era)
        {
            if (!IsHighTech(kind))
                return true;
            return era >= TechEraId.Era5_ModernFuture;
        }

        public static bool IsHighTech(DefensiveStructureKindId kind)
        {
            return kind is DefensiveStructureKindId.EnergyShield
                or DefensiveStructureKindId.ForceField
                or DefensiveStructureKindId.AutomatedTurret;
        }

        public static float GetMaxHp(DefensiveStructureKindId kind)
        {
            return kind switch
            {
                DefensiveStructureKindId.Trenches => 400f,
                DefensiveStructureKindId.SandbagWall => 500f,
                DefensiveStructureKindId.BarbedWire => 250f,
                DefensiveStructureKindId.AntiTankHedgehogs => 900f,
                DefensiveStructureKindId.Minefield => 120f,
                DefensiveStructureKindId.Pillbox => 2000f,
                DefensiveStructureKindId.Bunker => 15000f,
                DefensiveStructureKindId.ConcreteRedoubt => 5000f,
                DefensiveStructureKindId.FortifiedPosition => 8000f,
                DefensiveStructureKindId.EnergyShield => 5000f,
                DefensiveStructureKindId.ForceField => 6000f,
                DefensiveStructureKindId.AutomatedTurret => 3500f,
                _ => 300f
            };
        }

        public static float GetDefenseBonusPercent(DefensiveStructureKindId kind)
        {
            return kind switch
            {
                DefensiveStructureKindId.Trenches => 50f,
                DefensiveStructureKindId.SandbagWall => 35f,
                DefensiveStructureKindId.BarbedWire => 10f,
                DefensiveStructureKindId.AntiTankHedgehogs => 20f,
                DefensiveStructureKindId.Minefield => 15f,
                DefensiveStructureKindId.Pillbox => 60f,
                DefensiveStructureKindId.Bunker => 75f,
                DefensiveStructureKindId.ConcreteRedoubt => 60f,
                DefensiveStructureKindId.FortifiedPosition => 70f,
                DefensiveStructureKindId.EnergyShield => 85f,
                DefensiveStructureKindId.ForceField => 75f,
                DefensiveStructureKindId.AutomatedTurret => 45f,
                _ => 0f
            };
        }

        public static float GetSlowEffectPercent(DefensiveStructureKindId kind)
        {
            return kind switch
            {
                DefensiveStructureKindId.BarbedWire => 70f,
                DefensiveStructureKindId.Minefield => 30f,
                DefensiveStructureKindId.AntiTankHedgehogs => 85f,
                DefensiveStructureKindId.ForceField => 50f,
                _ => 0f
            };
        }

        public static float GetContactDamage(DefensiveStructureKindId kind)
        {
            return kind switch
            {
                DefensiveStructureKindId.BarbedWire => 35f,
                DefensiveStructureKindId.Minefield => 500f,
                DefensiveStructureKindId.AutomatedTurret => 180f,
                _ => 0f
            };
        }

        public static float GetEnergyDemandKw(DefensiveStructureKindId kind)
        {
            return kind switch
            {
                DefensiveStructureKindId.EnergyShield => 50f,
                DefensiveStructureKindId.ForceField => 100f,
                DefensiveStructureKindId.AutomatedTurret => 50f,
                _ => 0f
            };
        }

        public static float ComputeDailyDamage(float incomingDamagePressure, float defenseBonusPercent, bool powered)
        {
            var mitigation = math.saturate(defenseBonusPercent / 100f);
            var baseDamage = math.max(0f, incomingDamagePressure) * (1f - mitigation);
            return powered ? baseDamage * 0.6f : baseDamage;
        }
    }
}
