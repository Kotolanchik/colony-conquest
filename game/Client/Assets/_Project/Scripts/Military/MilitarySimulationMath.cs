using Unity.Burst;
using Unity.Mathematics;

namespace ColonyConquest.Military
{
    /// <summary>Математика военной симуляции: командование, бой, подавление, усталость, ранения.</summary>
    [BurstCompile]
    public static class MilitarySimulationMath
    {
        public static void GetWeatherProfile(MilitaryWeatherType weather, out float visibility01, out float accuracyMult,
            out float movementMult, out float vehicleMult, out float communicationPenalty01, out float suppressionMult,
            out float severity01)
        {
            switch (weather)
            {
                case MilitaryWeatherType.Cloudy:
                    visibility01 = 0.90f;
                    accuracyMult = 1.00f;
                    movementMult = 1.00f;
                    vehicleMult = 1.00f;
                    communicationPenalty01 = 0.05f;
                    suppressionMult = 1.02f;
                    severity01 = 0.10f;
                    return;
                case MilitaryWeatherType.LightRain:
                    visibility01 = 0.80f;
                    accuracyMult = 0.95f;
                    movementMult = 0.90f;
                    vehicleMult = 0.90f;
                    communicationPenalty01 = 0.20f;
                    suppressionMult = 1.05f;
                    severity01 = 0.25f;
                    return;
                case MilitaryWeatherType.HeavyRain:
                    visibility01 = 0.60f;
                    accuracyMult = 0.80f;
                    movementMult = 0.70f;
                    vehicleMult = 0.70f;
                    communicationPenalty01 = 0.40f;
                    suppressionMult = 1.10f;
                    severity01 = 0.45f;
                    return;
                case MilitaryWeatherType.Thunderstorm:
                    visibility01 = 0.50f;
                    accuracyMult = 0.70f;
                    movementMult = 0.60f;
                    vehicleMult = 0.60f;
                    communicationPenalty01 = 0.55f;
                    suppressionMult = 1.15f;
                    severity01 = 0.60f;
                    return;
                case MilitaryWeatherType.Fog:
                    visibility01 = 0.30f;
                    accuracyMult = 0.60f;
                    movementMult = 0.80f;
                    vehicleMult = 0.90f;
                    communicationPenalty01 = 0.15f;
                    suppressionMult = 1.08f;
                    severity01 = 0.50f;
                    return;
                case MilitaryWeatherType.Snow:
                    visibility01 = 0.70f;
                    accuracyMult = 0.85f;
                    movementMult = 0.50f;
                    vehicleMult = 0.40f;
                    communicationPenalty01 = 0.20f;
                    suppressionMult = 1.07f;
                    severity01 = 0.35f;
                    return;
                case MilitaryWeatherType.Blizzard:
                    visibility01 = 0.20f;
                    accuracyMult = 0.50f;
                    movementMult = 0.30f;
                    vehicleMult = 0.20f;
                    communicationPenalty01 = 0.60f;
                    suppressionMult = 1.20f;
                    severity01 = 0.85f;
                    return;
                case MilitaryWeatherType.Sandstorm:
                    visibility01 = 0.15f;
                    accuracyMult = 0.40f;
                    movementMult = 0.40f;
                    vehicleMult = 0.50f;
                    communicationPenalty01 = 0.50f;
                    suppressionMult = 1.18f;
                    severity01 = 0.80f;
                    return;
                default:
                    visibility01 = 1f;
                    accuracyMult = 1f;
                    movementMult = 1f;
                    vehicleMult = 1f;
                    communicationPenalty01 = 0f;
                    suppressionMult = 1f;
                    severity01 = 0f;
                    return;
            }
        }

        public static float ComputeOrderDelayMinutes(float distanceKm, float reactionTimeSeconds, bool hasRadio,
            float radioInterference01, float weatherCommunicationPenalty01, float nightPenalty01,
            float commanderLossPenaltyMinutes)
        {
            var distanceDelay = math.max(0f, distanceKm) / 5f;
            var reactionDelay = math.max(0f, reactionTimeSeconds) / 60f;
            var radioFactor = hasRadio ? 1f + math.saturate(radioInterference01) : 1.6f;
            var weatherFactor = 1f + math.saturate(weatherCommunicationPenalty01);
            var nightFactor = 1f + math.saturate(nightPenalty01);
            var delay = (distanceDelay + reactionDelay + 0.5f) * radioFactor * weatherFactor * nightFactor;
            return delay + math.max(0f, commanderLossPenaltyMinutes);
        }

        public static float ComputeMorale(float commanderBonus, float veteranBonus, float supplyBonus, float casualtyPenalty,
            float isolationPenalty, float suppressionPenalty, float coverBonus, float nightPenalty)
        {
            var morale = 100f +
                         math.clamp(commanderBonus, 0f, 30f) +
                         math.clamp(veteranBonus, 0f, 20f) +
                         math.clamp(supplyBonus, 0f, 15f) -
                         math.clamp(casualtyPenalty, 0f, 50f) -
                         math.clamp(isolationPenalty, 0f, 25f) -
                         math.clamp(suppressionPenalty, 0f, 40f) +
                         math.clamp(coverBonus, 0f, 10f) -
                         math.clamp(nightPenalty, 0f, 10f);
            return math.clamp(morale, 0f, 100f);
        }

        public static float GetMoraleAccuracyMultiplier(float morale)
        {
            if (morale >= 90f)
                return 1.15f;
            if (morale >= 70f)
                return 1.05f;
            if (morale >= 50f)
                return 1f;
            if (morale >= 30f)
                return 0.85f;
            if (morale >= 10f)
                return 0.70f;
            return 0.50f;
        }

        public static float GetMoraleSpeedMultiplier(float morale)
        {
            if (morale >= 90f)
                return 1.10f;
            if (morale >= 70f)
                return 1f;
            if (morale >= 50f)
                return 1f;
            if (morale >= 30f)
                return 0.90f;
            if (morale >= 10f)
                return 0.75f;
            return 0.50f;
        }

        public static float GetRetreatChance01(float morale)
        {
            if (morale >= 50f)
                return 0f;
            if (morale >= 30f)
                return 0.05f;
            if (morale >= 10f)
                return 0.25f;
            return 0.60f;
        }

        public static float ComputeHitChance(float baseAccuracy01, float distanceMeters, float effectiveRangeMeters,
            float maxRangeMeters, float movementModifier, float targetSizeModifier, float coverModifier,
            float visibilityModifier, float weatherModifier, float fatigueModifier, float moraleModifier)
        {
            var distanceFactor = 1f - math.saturate(math.max(0f, distanceMeters - effectiveRangeMeters) /
                                                    math.max(1f, maxRangeMeters - effectiveRangeMeters));
            var hit = math.saturate(baseAccuracy01) *
                      math.saturate(distanceFactor) *
                      math.saturate(movementModifier) *
                      math.saturate(targetSizeModifier) *
                      math.saturate(coverModifier) *
                      math.saturate(visibilityModifier) *
                      math.saturate(weatherModifier) *
                      math.saturate(fatigueModifier) *
                      math.saturate(moraleModifier);
            return math.saturate(hit);
        }

        public static float ResolveArmorDamage(float baseDamage, float penetrationValue, float armorValue, float random01)
        {
            var penChance = armorValue <= 1e-3f ? 1f : penetrationValue / armorValue;
            if (penChance >= 1f)
                return baseDamage;
            if (penChance >= 0.5f)
            {
                var hit = random01 < penChance;
                return hit ? baseDamage * 0.8f : baseDamage * 0.1f;
            }

            var reducedChance = math.saturate(penChance * 0.5f);
            return random01 < reducedChance ? baseDamage * 0.5f : 0f;
        }

        public static float ComputeSuppressionGain(float baseSuppression, float suppressionTypeMultiplier,
            float proximityMultiplier, float coverReduction, float moraleResistance)
        {
            return math.max(0f, baseSuppression) *
                   math.max(0f, suppressionTypeMultiplier) *
                   math.max(0f, proximityMultiplier) *
                   math.saturate(1f - coverReduction) *
                   math.saturate(1f - moraleResistance);
        }

        public static float ComputeSuppressionDecayPerSecond(bool inCombat, bool underFire)
        {
            if (underFire)
                return 0f;
            return inCombat ? 2f : 5f;
        }

        public static float ComputeFatigueDelta(float activityRatePerMinute, float loadMultiplier, float terrainMultiplier,
            float weatherMultiplier, bool resting, float restQuality, float supplyModifier, float medicalModifier)
        {
            if (resting)
            {
                var recovery = 5f * math.max(0.25f, restQuality) * math.max(0.25f, supplyModifier) * math.max(0.25f, medicalModifier);
                return -recovery;
            }

            return math.max(0f, activityRatePerMinute) *
                   math.max(0.1f, loadMultiplier) *
                   math.max(0.1f, terrainMultiplier) *
                   math.max(0.1f, weatherMultiplier);
        }

        public static float ComputeEffectiveCoverProtection(float baseProtection01, float qualityMultiplier,
            float occupantState01, float directionFactor01)
        {
            return math.saturate(math.saturate(baseProtection01) *
                                 math.clamp(qualityMultiplier, 0.5f, 1.5f) *
                                 math.saturate(occupantState01) *
                                 math.saturate(directionFactor01));
        }

        public static float ComputeStructureDamage(float baseDamage, float damageTypeMultiplier, float armorPenetration01,
            float structureResistance)
        {
            var incoming = math.max(0f, baseDamage) * math.max(0f, damageTypeMultiplier) * math.max(0f, armorPenetration01);
            return math.max(0f, incoming - math.max(0f, structureResistance));
        }

        public static MilitaryWoundType ResolveWoundType(float damageToMaxHpRatio01)
        {
            if (damageToMaxHpRatio01 <= 0f)
                return MilitaryWoundType.None;
            if (damageToMaxHpRatio01 < 0.3f)
                return MilitaryWoundType.Light;
            if (damageToMaxHpRatio01 < 0.6f)
                return MilitaryWoundType.Medium;
            if (damageToMaxHpRatio01 < 0.9f)
                return MilitaryWoundType.Heavy;
            if (damageToMaxHpRatio01 <= 1f)
                return MilitaryWoundType.Critical;
            return MilitaryWoundType.Fatal;
        }

        public static float GetBleedingRate(MilitaryWoundType wound)
        {
            return wound switch
            {
                MilitaryWoundType.Light => 0.03f,
                MilitaryWoundType.Medium => 0.09f,
                MilitaryWoundType.Heavy => 0.20f,
                MilitaryWoundType.Critical => 0.40f,
                _ => 0f
            };
        }

        public static float GetTimeToDeathSeconds(MilitaryWoundType wound)
        {
            return wound switch
            {
                MilitaryWoundType.Light => 600f,
                MilitaryWoundType.Medium => 600f,
                MilitaryWoundType.Heavy => 300f,
                MilitaryWoundType.Critical => 120f,
                _ => 0f
            };
        }

        public static bool TryGetUnitTemplate(MilitaryUnitType type, out MilitaryUnitTemplate template)
        {
            switch (type)
            {
                case MilitaryUnitType.Rifleman:
                    template = Build(100f, 5f, 25f, 0.75f, 600f, 500f, 180f, 0f, 8f, false, false);
                    return true;
                case MilitaryUnitType.Assault:
                    template = Build(120f, 10f, 20f, 0.60f, 900f, 120f, 240f, 0f, 10f, false, false);
                    return true;
                case MilitaryUnitType.MachineGunner:
                    template = Build(100f, 5f, 30f, 0.60f, 800f, 600f, 600f, 0f, 15f, false, false);
                    return true;
                case MilitaryUnitType.Sniper:
                    template = Build(80f, 3f, 150f, 0.95f, 40f, 1500f, 30f, 0f, 20f, false, true);
                    return true;
                case MilitaryUnitType.Engineer:
                    template = Build(100f, 8f, 22f, 0.65f, 500f, 250f, 150f, 0f, 12f, false, false);
                    return true;
                case MilitaryUnitType.Medic:
                    template = Build(90f, 4f, 12f, 0.55f, 250f, 120f, 45f, 0f, 6f, false, false);
                    return true;
                case MilitaryUnitType.Grenadier:
                    template = Build(100f, 6f, 80f, 0.60f, 12f, 400f, 12f, 0f, 120f, false, false);
                    return true;
                case MilitaryUnitType.AntiTankRifleman:
                    template = Build(100f, 6f, 400f, 0.70f, 6f, 300f, 4f, 0f, 300f, false, false);
                    return true;
                case MilitaryUnitType.ManpadsOperator:
                    template = Build(90f, 5f, 380f, 0.75f, 4f, 4000f, 2f, 0f, 250f, false, false);
                    return true;
                case MilitaryUnitType.LightTank:
                    template = Build(800f, 350f, 180f, 0.85f, 10f, 1500f, 45f, 300f, 120f, true, true);
                    return true;
                case MilitaryUnitType.MediumTank:
                    template = Build(1500f, 800f, 280f, 0.90f, 8f, 2000f, 50f, 500f, 220f, true, true);
                    return true;
                case MilitaryUnitType.HeavyTank:
                    template = Build(2500f, 1500f, 450f, 0.85f, 5f, 2500f, 40f, 800f, 350f, true, true);
                    return true;
                case MilitaryUnitType.InfantryFightingVehicle:
                    template = Build(600f, 160f, 45f, 0.80f, 300f, 1500f, 500f, 400f, 120f, true, true);
                    return true;
                case MilitaryUnitType.ArmoredPersonnelCarrier:
                    template = Build(400f, 120f, 35f, 0.75f, 300f, 1200f, 500f, 300f, 70f, true, false);
                    return true;
                case MilitaryUnitType.Mortar82:
                    template = Build(50f, 0f, 150f, 0.65f, 15f, 4000f, 60f, 0f, 40f, false, false);
                    return true;
                case MilitaryUnitType.Mortar120:
                    template = Build(80f, 0f, 280f, 0.60f, 8f, 7000f, 36f, 0f, 60f, false, false);
                    return true;
                case MilitaryUnitType.Howitzer122:
                    template = Build(100f, 5f, 450f, 0.55f, 4f, 15000f, 24f, 0f, 80f, false, false);
                    return true;
                case MilitaryUnitType.Howitzer152:
                    template = Build(150f, 8f, 800f, 0.50f, 2f, 20000f, 16f, 0f, 100f, false, false);
                    return true;
                case MilitaryUnitType.Mlrs:
                    template = Build(200f, 10f, 240f, 0.45f, 36f, 30000f, 36f, 250f, 90f, true, false);
                    return true;
                case MilitaryUnitType.ReconDrone:
                    template = Build(50f, 3f, 0f, 0.90f, 0f, 5000f, 0f, 50f, 0f, true, true);
                    return true;
                case MilitaryUnitType.AttackDrone:
                    template = Build(80f, 6f, 180f, 0.80f, 8f, 4500f, 8f, 80f, 120f, true, true);
                    return true;
                case MilitaryUnitType.AttackHelicopter:
                    template = Build(500f, 50f, 300f, 0.82f, 12f, 5000f, 16f, 1000f, 250f, true, true);
                    return true;
                default:
                    template = default;
                    return false;
            }
        }

        private static MilitaryUnitTemplate Build(float hp, float armor, float damage, float accuracy01, float fireRatePerMinute,
            float rangeMeters, float ammo, float fuel, float penetration, bool isVehicle, bool hasNvg)
        {
            return new MilitaryUnitTemplate
            {
                Health = hp,
                Armor = armor,
                Damage = damage,
                Accuracy01 = accuracy01,
                FireRatePerMinute = fireRatePerMinute,
                RangeMeters = rangeMeters,
                Ammo = ammo,
                Fuel = fuel,
                ArmorPenetration = penetration,
                IsVehicle = isVehicle ? (byte)1 : (byte)0,
                HasNightVision = hasNvg ? (byte)1 : (byte)0
            };
        }
    }
}
