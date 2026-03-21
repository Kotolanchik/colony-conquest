using Unity.Burst;
using Unity.Mathematics;
using ColonyConquest.Core;

namespace ColonyConquest.Construction
{
    /// <summary>Формулы строительства по <c>spec/construction_system_spec.md</c> §5.</summary>
    [BurstCompile]
    public static class ConstructionRuntimeMath
    {
        private const float DayMinutes = 24f * 60f;

        public static float ComputeAdjustedBuildMinutes(
            float baseWorkMinutes,
            ushort workers,
            float averageBuilderSkill,
            float toolQuality,
            float weatherModifier,
            bool hasLighting)
        {
            var workersFactor = math.pow(math.max((float)workers, 1f), 0.7f);
            var skill = math.max(0.5f, averageBuilderSkill);
            var tools = math.clamp(toolQuality, 0.5f, 1.5f);
            var weather = math.max(1f, weatherModifier);
            var nightPenalty = hasLighting ? 0f : 0.5f;
            return baseWorkMinutes * (1f / workersFactor) * (1f / skill) * (1f / tools) * weather * (1f + nightPenalty);
        }

        public static float GetDailyProgressDelta(
            float baseWorkMinutes,
            ushort workers,
            float averageBuilderSkill,
            float toolQuality,
            float weatherModifier,
            bool hasLighting,
            ConstructionPriority priority)
        {
            var adjusted = ComputeAdjustedBuildMinutes(baseWorkMinutes, workers, averageBuilderSkill, toolQuality, weatherModifier,
                hasLighting);
            if (adjusted <= 1e-3f)
                return 1f;

            var priorityMul = GetPrioritySpeedMultiplier(priority);
            return math.saturate((DayMinutes / adjusted) * priorityMul);
        }

        public static float GetPrioritySpeedMultiplier(ConstructionPriority priority)
        {
            return priority switch
            {
                ConstructionPriority.Critical => 1.8f,
                ConstructionPriority.High => 1.5f,
                ConstructionPriority.Low => 0.7f,
                _ => 1f
            };
        }

        public static ConstructionStage ResolveStage(float progress01)
        {
            if (progress01 >= 0.999f)
                return ConstructionStage.Completed;
            if (progress01 < 0.001f)
                return ConstructionStage.Planning;
            if (progress01 < 0.10f)
                return ConstructionStage.Preparation;
            if (progress01 < 0.20f)
                return ConstructionStage.Foundation;
            if (progress01 < 0.50f)
                return ConstructionStage.Framing;
            if (progress01 < 0.90f)
                return ConstructionStage.Finishing;
            return ConstructionStage.Equipment;
        }

        public static bool TryGetBlueprintDefaults(
            ConstructionBlueprintId blueprint,
            out float wood,
            out float stone,
            out float steel,
            out float baseWorkMinutes,
            out ConstructionZoneKindId zoneKind)
        {
            switch (blueprint)
            {
                case ConstructionBlueprintId.EarthHut:
                    wood = 10f;
                    stone = 5f;
                    steel = 0f;
                    baseWorkMinutes = 30f;
                    zoneKind = ConstructionZoneKindId.Residential;
                    return true;
                case ConstructionBlueprintId.Cabin:
                    wood = 30f;
                    stone = 10f;
                    steel = 0f;
                    baseWorkMinutes = 120f;
                    zoneKind = ConstructionZoneKindId.Residential;
                    return true;
                case ConstructionBlueprintId.House:
                    wood = 50f;
                    stone = 20f;
                    steel = 5f;
                    baseWorkMinutes = 240f;
                    zoneKind = ConstructionZoneKindId.Residential;
                    return true;
                case ConstructionBlueprintId.Manor:
                    wood = 100f;
                    stone = 50f;
                    steel = 20f;
                    baseWorkMinutes = 480f;
                    zoneKind = ConstructionZoneKindId.Residential;
                    return true;
                case ConstructionBlueprintId.WorkerTenement:
                    wood = 80f;
                    stone = 40f;
                    steel = 10f;
                    baseWorkMinutes = 360f;
                    zoneKind = ConstructionZoneKindId.Residential;
                    return true;
                case ConstructionBlueprintId.TownhouseEpoch2:
                    wood = 20f;
                    stone = 60f;
                    steel = 20f;
                    baseWorkMinutes = 480f;
                    zoneKind = ConstructionZoneKindId.Residential;
                    return true;
                default:
                    wood = 0f;
                    stone = 0f;
                    steel = 0f;
                    baseWorkMinutes = 0f;
                    zoneKind = ConstructionZoneKindId.None;
                    return false;
            }
        }
    }
}
