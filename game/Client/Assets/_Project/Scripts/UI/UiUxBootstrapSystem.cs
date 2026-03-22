using ColonyConquest.Core;
using Unity.Entities;

namespace ColonyConquest.UI
{
    /// <summary>Создаёт singleton и каталоги runtime UI/UX.</summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(GameBootstrapSystem))]
    public partial struct UiUxBootstrapSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<SimulationRootState>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var em = state.EntityManager;
            using var query = em.CreateEntityQuery(ComponentType.ReadOnly<UiUxSimulationSingleton>());
            if (query.CalculateEntityCount() != 0)
                return;

            var entity = em.CreateEntity();
            em.AddComponent<UiUxSimulationSingleton>(entity);
            em.AddComponent(entity, new UiUxSimulationState
            {
                LastProcessedDay = uint.MaxValue,
                LastNotificationId = 0,
                NotificationsTotal = 0,
                CameraLevel = UiCameraLevel.Tactical,
                TimeSpeed = UiTimeSpeedLevel.Normal,
                IsPaused = 0,
                BuildModeActive = 0,
                AutoPauseOnCritical = 1,
                ColorBlindMode = UiColorBlindMode.Off,
                ContrastMode = UiContrastMode.Normal,
                FontScalePercent = 100,
                AnimationsEnabled = 1,
                SoundSignalsEnabled = 1,
                LastFoodBand = UiResourceBand.Green,
                LastEnergyBand = UiResourceBand.Green,
                LastSupplyBand = UiResourceBand.Green
            });

            var notifications = em.AddBuffer<UiNotificationEntry>(entity);
            notifications.Clear();

            var hotkeys = em.AddBuffer<UiHotkeyBindingEntry>(entity);
            SeedHotkeys(ref hotkeys);

            var panels = em.AddBuffer<UiPanelStateEntry>(entity);
            SeedPanels(ref panels);

            var resources = em.AddBuffer<UiResourceIndicatorEntry>(entity);
            resources.Clear();
        }

        static void SeedHotkeys(ref DynamicBuffer<UiHotkeyBindingEntry> hotkeys)
        {
            AddHotkey(ref hotkeys, UiHotkeyGroup.General, "Pause", "Space");
            AddHotkey(ref hotkeys, UiHotkeyGroup.General, "Cancel/Menu", "Esc");
            AddHotkey(ref hotkeys, UiHotkeyGroup.General, "Cycle Camera", "Tab");
            AddHotkey(ref hotkeys, UiHotkeyGroup.Camera, "Camera Micro", "F1");
            AddHotkey(ref hotkeys, UiHotkeyGroup.Camera, "Camera Tactical", "F2");
            AddHotkey(ref hotkeys, UiHotkeyGroup.Camera, "Camera Operational", "F3");
            AddHotkey(ref hotkeys, UiHotkeyGroup.Camera, "Camera Strategic", "F4");
            AddHotkey(ref hotkeys, UiHotkeyGroup.Selection, "Select All", "Ctrl+A");
            AddHotkey(ref hotkeys, UiHotkeyGroup.Selection, "Add Selection", "Shift+LMB");
            AddHotkey(ref hotkeys, UiHotkeyGroup.Camera, "Move Camera", "WASD");
            AddHotkey(ref hotkeys, UiHotkeyGroup.Camera, "Zoom", "MouseWheel");
            AddHotkey(ref hotkeys, UiHotkeyGroup.Camera, "Rotate Camera", "Q/E");
            AddHotkey(ref hotkeys, UiHotkeyGroup.Camera, "Center Base", "Home");
            AddHotkey(ref hotkeys, UiHotkeyGroup.Construction, "Toggle Build Mode", "B");
            AddHotkey(ref hotkeys, UiHotkeyGroup.Construction, "Rotate Building", "R");
            AddHotkey(ref hotkeys, UiHotkeyGroup.Military, "Attack", "A");
            AddHotkey(ref hotkeys, UiHotkeyGroup.Military, "Move", "M");
            AddHotkey(ref hotkeys, UiHotkeyGroup.Military, "Hold Position", "H");
            AddHotkey(ref hotkeys, UiHotkeyGroup.Time, "Speed Normal", "~");
            AddHotkey(ref hotkeys, UiHotkeyGroup.Time, "Speed x2", "1");
            AddHotkey(ref hotkeys, UiHotkeyGroup.Time, "Speed x3", "2");
            AddHotkey(ref hotkeys, UiHotkeyGroup.Time, "Speed x5", "3");
        }

        static void SeedPanels(ref DynamicBuffer<UiPanelStateEntry> panels)
        {
            AddPanel(ref panels, UiPanelKind.ResourceTopBar, UiCameraLevel.Micro, UiCameraLevel.Strategic);
            AddPanel(ref panels, UiPanelKind.TimeWeatherTopBar, UiCameraLevel.Micro, UiCameraLevel.Strategic);
            AddPanel(ref panels, UiPanelKind.Minimap, UiCameraLevel.Micro, UiCameraLevel.Operational);
            AddPanel(ref panels, UiPanelKind.UnitPanel, UiCameraLevel.Micro, UiCameraLevel.Tactical);
            AddPanel(ref panels, UiPanelKind.BuildingPanel, UiCameraLevel.Tactical, UiCameraLevel.Operational);
            AddPanel(ref panels, UiPanelKind.BottomActionBar, UiCameraLevel.Micro, UiCameraLevel.Operational);
            AddPanel(ref panels, UiPanelKind.NotificationsPanel, UiCameraLevel.Micro, UiCameraLevel.Strategic);
            AddPanel(ref panels, UiPanelKind.OperationsPanel, UiCameraLevel.Operational, UiCameraLevel.Strategic);
            AddPanel(ref panels, UiPanelKind.StrategyPanel, UiCameraLevel.Strategic, UiCameraLevel.Strategic);
        }

        static void AddHotkey(ref DynamicBuffer<UiHotkeyBindingEntry> hotkeys, UiHotkeyGroup group, FixedString32Bytes action,
            FixedString16Bytes key)
        {
            hotkeys.Add(new UiHotkeyBindingEntry
            {
                Group = group,
                Enabled = 1,
                ActionName = action,
                KeyChord = key
            });
        }

        static void AddPanel(ref DynamicBuffer<UiPanelStateEntry> panels, UiPanelKind panel, UiCameraLevel min, UiCameraLevel max)
        {
            panels.Add(new UiPanelStateEntry
            {
                Panel = panel,
                MinCamera = min,
                MaxCamera = max,
                IsVisible = 1
            });
        }
    }
}
