using ColonyConquest.WorldMap;
using Unity.Burst;
using Unity.Mathematics;

namespace ColonyConquest.UI
{
    /// <summary>Математика и правила отображения UI/UX runtime.</summary>
    [BurstCompile]
    public static class UiUxSimulationMath
    {
        [BurstCompile]
        public static UiCameraLevel MapScaleToCameraLevel(WorldMapScaleLevel scale)
        {
            return scale switch
            {
                WorldMapScaleLevel.Tactical => UiCameraLevel.Micro,
                WorldMapScaleLevel.Local => UiCameraLevel.Tactical,
                WorldMapScaleLevel.Regional => UiCameraLevel.Operational,
                _ => UiCameraLevel.Strategic
            };
        }

        [BurstCompile]
        public static UiResourceBand ComputeResourceBand(float amount, float safeAmount)
        {
            var safe = math.max(1f, safeAmount);
            var ratio = math.saturate(amount / safe);
            if (ratio < 0.25f)
                return UiResourceBand.Red;
            if (ratio < 0.5f)
                return UiResourceBand.Yellow;
            return UiResourceBand.Green;
        }

        [BurstCompile]
        public static float ComputeResourceStress01(UiResourceBand food, UiResourceBand energy, UiResourceBand supply)
        {
            var score = 0f;
            score += food == UiResourceBand.Red ? 0.45f : food == UiResourceBand.Yellow ? 0.2f : 0f;
            score += energy == UiResourceBand.Red ? 0.35f : energy == UiResourceBand.Yellow ? 0.15f : 0f;
            score += supply == UiResourceBand.Red ? 0.2f : supply == UiResourceBand.Yellow ? 0.1f : 0f;
            return math.saturate(score);
        }

        [BurstCompile]
        public static float ComputeHudLoad01(float tension01, uint activeNotifications, UiCameraLevel level)
        {
            var cameraWeight = level switch
            {
                UiCameraLevel.Micro => 0.2f,
                UiCameraLevel.Tactical => 0.35f,
                UiCameraLevel.Operational => 0.55f,
                _ => 0.7f
            };
            var notificationLoad = math.saturate(activeNotifications / 10f) * 0.5f;
            return math.saturate(cameraWeight + notificationLoad + tension01 * 0.35f);
        }

        [BurstCompile]
        public static byte GetNotificationLifetimeDays(UiNotificationType type)
        {
            return type switch
            {
                UiNotificationType.Critical => (byte)1,
                UiNotificationType.Important => (byte)2,
                UiNotificationType.Information => (byte)3,
                _ => (byte)4
            };
        }

        [BurstCompile]
        public static byte ClampFontScalePercent(byte value)
        {
            if (value <= 87)
                return 75;
            if (value <= 112)
                return 100;
            if (value <= 137)
                return 125;
            return 150;
        }

        [BurstCompile]
        public static UiCameraLevel NextCameraLevel(UiCameraLevel level)
        {
            return level switch
            {
                UiCameraLevel.Micro => UiCameraLevel.Tactical,
                UiCameraLevel.Tactical => UiCameraLevel.Operational,
                UiCameraLevel.Operational => UiCameraLevel.Strategic,
                _ => UiCameraLevel.Micro
            };
        }
    }
}
