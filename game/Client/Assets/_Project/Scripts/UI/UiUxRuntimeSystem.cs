using ColonyConquest.Analytics;
using ColonyConquest.Audio;
using ColonyConquest.Core;
using ColonyConquest.Economy;
using ColonyConquest.Military;
using ColonyConquest.Simulation;
using ColonyConquest.Story;
using ColonyConquest.Technology;
using ColonyConquest.WorldMap;
using Unity.Collections;
using Unity.Entities;
using UnityEngine.InputSystem;

namespace ColonyConquest.UI
{
    /// <summary>Полный runtime UI/UX: hotkeys, adaptive panels, HUD indicators, notifications, accessibility.</summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(InputGatherSystem))]
    public partial struct UiUxRuntimeSystem : ISystem
    {
        const uint UiSfxCritical = 401u;
        const uint UiSfxImportant = 402u;
        const uint UiSfxInfo = 403u;
        const uint UiSfxAchievement = 404u;
        const int MaxNotifications = 24;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<UiUxSimulationSingleton>();
            state.RequireForUpdate<UiUxSimulationState>();
            state.RequireForUpdate<GameCalendarState>();
            state.RequireForUpdate<WorldMapFocusState>();
        }

        public void OnUpdate(ref SystemState state)
        {
            ref var sim = ref SystemAPI.GetSingletonRW<UiUxSimulationState>().ValueRW;
            var calendar = SystemAPI.GetSingleton<GameCalendarState>();
            var focus = SystemAPI.GetSingleton<WorldMapFocusState>();

            var notifications = SystemAPI.GetSingletonBuffer<UiNotificationEntry>(ref state);
            var panels = SystemAPI.GetSingletonBuffer<UiPanelStateEntry>(ref state);
            var resources = SystemAPI.GetSingletonBuffer<UiResourceIndicatorEntry>(ref state);

            var isNewDay = sim.LastProcessedDay != calendar.DayIndex;
            if (isNewDay)
            {
                sim.LastProcessedDay = calendar.DayIndex;
                sim.HotkeyActivationsToday = 0;
                sim.NotificationsCriticalToday = 0;
                sim.NotificationsImportantToday = 0;
                sim.NotificationsInfoToday = 0;
                sim.NotificationsAchievementToday = 0;
            }

            PruneNotifications(calendar.DayIndex, ref notifications);
            sim.NotificationsActive = (uint)notifications.Length;
            sim.FontScalePercent = UiUxSimulationMath.ClampFontScalePercent(sim.FontScalePercent);

            var cameraLevel = UiUxSimulationMath.MapScaleToCameraLevel(focus.ActiveScale);
            ProcessHotkeys(ref sim, ref notifications, ref cameraLevel);
            sim.CameraLevel = cameraLevel;

            if (SystemAPI.HasSingleton<ConstructionGhostState>())
                sim.BuildModeActive = SystemAPI.GetSingleton<ConstructionGhostState>().Active;
            else
                sim.BuildModeActive = 0;

            var foodAmount = 0f;
            if (SystemAPI.HasSingleton<ResourceStockpileSingleton>())
            {
                var stock = SystemAPI.GetSingletonBuffer<ResourceStockEntry>(ref state);
                foodAmount = ResourceStockpileOps.GetAmount(ref stock, ResourceId.CropWheat)
                    + ResourceStockpileOps.GetAmount(ref stock, ResourceId.LivestockMeat)
                    + ResourceStockpileOps.GetAmount(ref stock, ResourceId.FishCatch);
                var woodAmount = ResourceStockpileOps.GetAmount(ref stock, ResourceId.Wood);
                var stoneAmount = ResourceStockpileOps.GetAmount(ref stock, ResourceId.Stone);
                var steelAmount = ResourceStockpileOps.GetAmount(ref stock, ResourceId.SteelIndustrial)
                    + ResourceStockpileOps.GetAmount(ref stock, ResourceId.SteelBasic)
                    + ResourceStockpileOps.GetAmount(ref stock, ResourceId.SteelRolledPlate);

                UpsertResource(ref resources, ResourceId.Wood, woodAmount, 0f, UiUxSimulationMath.ComputeResourceBand(woodAmount, 60f));
                UpsertResource(ref resources, ResourceId.Stone, stoneAmount, 0f,
                    UiUxSimulationMath.ComputeResourceBand(stoneAmount, 40f));
                UpsertResource(ref resources, ResourceId.SteelIndustrial, steelAmount, 0f,
                    UiUxSimulationMath.ComputeResourceBand(steelAmount, 35f));
                UpsertResource(ref resources, ResourceId.CropWheat, foodAmount, 0f,
                    UiUxSimulationMath.ComputeResourceBand(foodAmount, 90f));
            }

            var energySatisfied01 = 1f;
            if (SystemAPI.HasSingleton<EconomyEnergyState>())
            {
                var energy = SystemAPI.GetSingleton<EconomyEnergyState>();
                energySatisfied01 = energy.DemandKw > 1f ? energy.DeliveredKw / energy.DemandKw : 1f;
                UpsertResource(ref resources, ResourceId.None, energy.DeliveredKw, energy.DemandKw, UiUxSimulationMath.ComputeResourceBand(
                    energySatisfied01, 1f));
            }

            var supply01 = 1f;
            if (SystemAPI.HasSingleton<EconomyArmySupplyState>())
                supply01 = SystemAPI.GetSingleton<EconomyArmySupplyState>().ArmySupplyAdequacy01;

            var foodBand = UiUxSimulationMath.ComputeResourceBand(foodAmount, 90f);
            var energyBand = UiUxSimulationMath.ComputeResourceBand(energySatisfied01, 1f);
            var supplyBand = UiUxSimulationMath.ComputeResourceBand(supply01, 1f);

            RaiseBandTransitionNotifications(ref sim, ref notifications, calendar.DayIndex, foodBand, energyBand, supplyBand);
            sim.LastFoodBand = foodBand;
            sim.LastEnergyBand = energyBand;
            sim.LastSupplyBand = supplyBand;

            if (SystemAPI.HasSingleton<ColonyTechProgressState>())
            {
                var tech = SystemAPI.GetSingleton<ColonyTechProgressState>();
                if (tech.TechnologiesUnlocked > sim.LastSeenTechnologiesUnlocked)
                {
                    PushNotification(ref sim, ref notifications, calendar.DayIndex, UiNotificationType.Information,
                        "Research milestone completed", "Open Tech");
                }

                sim.LastSeenTechnologiesUnlocked = tech.TechnologiesUnlocked;
            }

            if (SystemAPI.HasSingleton<StorySimulationState>())
            {
                var story = SystemAPI.GetSingleton<StorySimulationState>();
                if (story.QuestsCompletedTotal > sim.LastSeenQuestsCompleted)
                {
                    PushNotification(ref sim, ref notifications, calendar.DayIndex, UiNotificationType.Achievement,
                        "Quest completed", "Open Journal");
                }

                sim.LastSeenQuestsCompleted = story.QuestsCompletedTotal;
                sim.HudLoad01 = UiUxSimulationMath.ComputeHudLoad01(story.StoryTension01, (uint)notifications.Length, sim.CameraLevel);
            }
            else
            {
                sim.HudLoad01 = UiUxSimulationMath.ComputeHudLoad01(0f, (uint)notifications.Length, sim.CameraLevel);
            }

            if (SystemAPI.HasSingleton<MilitarySimulationState>())
            {
                var military = SystemAPI.GetSingleton<MilitarySimulationState>();
                if (military.BattlesTotal > sim.LastSeenBattlesTotal)
                {
                    PushNotification(ref sim, ref notifications, calendar.DayIndex, UiNotificationType.Important,
                        "Combat report updated", "Open Military");
                }

                sim.LastSeenBattlesTotal = military.BattlesTotal;
            }

            if (SystemAPI.HasSingleton<MilitaryEnvironmentState>())
                sim.LastWeatherSeverity01 = SystemAPI.GetSingleton<MilitaryEnvironmentState>().WeatherSeverity01;
            else
                sim.LastWeatherSeverity01 = 0f;

            sim.ResourceStress01 = UiUxSimulationMath.ComputeResourceStress01(foodBand, energyBand, supplyBand);
            UpdatePanelVisibility(sim.CameraLevel, sim.BuildModeActive != 0, ref panels);
            sim.NotificationsActive = (uint)notifications.Length;

            if (isNewDay)
            {
                AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.UiCameraLevel, (byte)sim.CameraLevel);
                AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.UiNotificationsActive,
                    sim.NotificationsActive);
                AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.UiCriticalNotificationsToday,
                    sim.NotificationsCriticalToday);
                AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.UiResourceStress01, sim.ResourceStress01);
                AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.UiHudLoad01, sim.HudLoad01);
                AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.UiTimeSpeed, (byte)sim.TimeSpeed);
                AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.UiHotkeyActivationsToday,
                    sim.HotkeyActivationsToday);
            }
        }

        static void ProcessHotkeys(ref UiUxSimulationState sim, ref DynamicBuffer<UiNotificationEntry> notifications,
            ref UiCameraLevel cameraLevel)
        {
            var keyboard = Keyboard.current;
            if (keyboard == null)
                return;

            if (keyboard.tabKey.wasPressedThisFrame)
            {
                cameraLevel = UiUxSimulationMath.NextCameraLevel(sim.CameraLevel);
                RegisterHotkey(ref sim);
            }

            if (keyboard.f1Key.wasPressedThisFrame)
            {
                cameraLevel = UiCameraLevel.Micro;
                RegisterHotkey(ref sim);
            }

            if (keyboard.f2Key.wasPressedThisFrame)
            {
                cameraLevel = UiCameraLevel.Tactical;
                RegisterHotkey(ref sim);
            }

            if (keyboard.f3Key.wasPressedThisFrame)
            {
                cameraLevel = UiCameraLevel.Operational;
                RegisterHotkey(ref sim);
            }

            if (keyboard.f4Key.wasPressedThisFrame)
            {
                cameraLevel = UiCameraLevel.Strategic;
                RegisterHotkey(ref sim);
            }

            if (keyboard.spaceKey.wasPressedThisFrame)
            {
                sim.IsPaused = (byte)(sim.IsPaused == 0 ? 1 : 0);
                RegisterHotkey(ref sim);
            }

            if (keyboard.backquoteKey.wasPressedThisFrame)
            {
                sim.TimeSpeed = UiTimeSpeedLevel.Normal;
                RegisterHotkey(ref sim);
            }

            if (keyboard.digit1Key.wasPressedThisFrame)
            {
                sim.TimeSpeed = UiTimeSpeedLevel.X2;
                RegisterHotkey(ref sim);
            }

            if (keyboard.digit2Key.wasPressedThisFrame)
            {
                sim.TimeSpeed = UiTimeSpeedLevel.X3;
                RegisterHotkey(ref sim);
            }

            if (keyboard.digit3Key.wasPressedThisFrame)
            {
                sim.TimeSpeed = UiTimeSpeedLevel.X5;
                RegisterHotkey(ref sim);
            }

            if (keyboard.escapeKey.wasPressedThisFrame && notifications.Length > 0)
            {
                notifications.RemoveAt(notifications.Length - 1);
                RegisterHotkey(ref sim);
            }
        }

        static void RegisterHotkey(ref UiUxSimulationState sim)
        {
            sim.HotkeyActivationsToday++;
            sim.HotkeyActivationsTotal++;
        }

        static void RaiseBandTransitionNotifications(ref UiUxSimulationState sim, ref DynamicBuffer<UiNotificationEntry> notifications,
            uint day, UiResourceBand foodBand, UiResourceBand energyBand, UiResourceBand supplyBand)
        {
            if (sim.LastFoodBand != foodBand)
            {
                if (foodBand == UiResourceBand.Red)
                    PushNotification(ref sim, ref notifications, day, UiNotificationType.Critical, "Food shortage critical",
                        "Open Economy");
                else if (foodBand == UiResourceBand.Yellow)
                    PushNotification(ref sim, ref notifications, day, UiNotificationType.Important, "Food reserve is low",
                        "Open Economy");
            }

            if (sim.LastEnergyBand != energyBand)
            {
                if (energyBand == UiResourceBand.Red)
                    PushNotification(ref sim, ref notifications, day, UiNotificationType.Critical, "Energy grid overload",
                        "Open Power");
                else if (energyBand == UiResourceBand.Yellow)
                    PushNotification(ref sim, ref notifications, day, UiNotificationType.Important, "Energy reserve warning",
                        "Open Power");
            }

            if (sim.LastSupplyBand != supplyBand)
            {
                if (supplyBand == UiResourceBand.Red)
                    PushNotification(ref sim, ref notifications, day, UiNotificationType.Critical, "Army supply collapsing",
                        "Open Logistics");
                else if (supplyBand == UiResourceBand.Yellow)
                    PushNotification(ref sim, ref notifications, day, UiNotificationType.Important, "Army supply strained",
                        "Open Logistics");
            }
        }

        static void PushNotification(ref UiUxSimulationState sim, ref DynamicBuffer<UiNotificationEntry> notifications, uint day,
            UiNotificationType type, FixedString128Bytes message, FixedString64Bytes action)
        {
            sim.LastNotificationId++;
            sim.NotificationsTotal++;

            var ttl = UiUxSimulationMath.GetNotificationLifetimeDays(type);
            notifications.Add(new UiNotificationEntry
            {
                NotificationId = sim.LastNotificationId,
                Type = type,
                CreatedDay = day,
                ExpiresDay = day + ttl,
                HasAction = action.Length > 0 ? (byte)1 : (byte)0,
                PrimaryAction = action,
                Message = message
            });

            while (notifications.Length > MaxNotifications)
                notifications.RemoveAt(0);

            switch (type)
            {
                case UiNotificationType.Critical:
                    sim.NotificationsCriticalToday++;
                    if (sim.AutoPauseOnCritical != 0)
                        sim.IsPaused = 1;
                    if (sim.SoundSignalsEnabled != 0)
                        AudioBusStub.Post(UiSfxCritical, AudioSfxCategory.Interface);
                    break;
                case UiNotificationType.Important:
                    sim.NotificationsImportantToday++;
                    if (sim.SoundSignalsEnabled != 0)
                        AudioBusStub.Post(UiSfxImportant, AudioSfxCategory.Interface);
                    break;
                case UiNotificationType.Information:
                    sim.NotificationsInfoToday++;
                    if (sim.SoundSignalsEnabled != 0)
                        AudioBusStub.Post(UiSfxInfo, AudioSfxCategory.Interface);
                    break;
                default:
                    sim.NotificationsAchievementToday++;
                    if (sim.SoundSignalsEnabled != 0)
                        AudioBusStub.Post(UiSfxAchievement, AudioSfxCategory.Interface);
                    break;
            }
        }

        static void PruneNotifications(uint day, ref DynamicBuffer<UiNotificationEntry> notifications)
        {
            for (var i = notifications.Length - 1; i >= 0; i--)
            {
                if (day > notifications[i].ExpiresDay)
                    notifications.RemoveAt(i);
            }
        }

        static void UpdatePanelVisibility(UiCameraLevel level, bool buildModeActive, ref DynamicBuffer<UiPanelStateEntry> panels)
        {
            for (var i = 0; i < panels.Length; i++)
            {
                var panel = panels[i];
                var inRange = level >= panel.MinCamera && level <= panel.MaxCamera;

                if (panel.Panel == UiPanelKind.BuildingPanel && buildModeActive)
                    inRange = true;

                panel.IsVisible = (byte)(inRange ? 1 : 0);
                panels[i] = panel;
            }
        }

        static void UpsertResource(ref DynamicBuffer<UiResourceIndicatorEntry> resources, ResourceId id, float amount, float deltaPerDay,
            UiResourceBand band)
        {
            for (var i = 0; i < resources.Length; i++)
            {
                if (resources[i].Resource != id)
                    continue;
                var entry = resources[i];
                entry.Amount = amount;
                entry.DeltaPerDay = deltaPerDay;
                entry.Band = band;
                resources[i] = entry;
                return;
            }

            resources.Add(new UiResourceIndicatorEntry
            {
                Resource = id,
                Amount = amount,
                DeltaPerDay = deltaPerDay,
                Band = band
            });
        }
    }
}
