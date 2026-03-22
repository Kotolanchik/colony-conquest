using ColonyConquest.Economy;
using Unity.Collections;
using Unity.Entities;

namespace ColonyConquest.UI
{
    public enum UiCameraLevel : byte
    {
        Micro = 1,
        Tactical = 2,
        Operational = 3,
        Strategic = 4
    }

    public enum UiNotificationType : byte
    {
        Critical = 0,
        Important = 1,
        Information = 2,
        Achievement = 3
    }

    public enum UiResourceBand : byte
    {
        Green = 0,
        Yellow = 1,
        Red = 2
    }

    public enum UiHotkeyGroup : byte
    {
        General = 0,
        Selection = 1,
        Camera = 2,
        Construction = 3,
        Military = 4,
        Time = 5
    }

    public enum UiColorBlindMode : byte
    {
        Off = 0,
        Protanopia = 1,
        Deuteranopia = 2,
        Tritanopia = 3
    }

    public enum UiContrastMode : byte
    {
        Normal = 0,
        High = 1
    }

    public enum UiPanelKind : byte
    {
        ResourceTopBar = 0,
        TimeWeatherTopBar = 1,
        Minimap = 2,
        UnitPanel = 3,
        BuildingPanel = 4,
        BottomActionBar = 5,
        NotificationsPanel = 6,
        OperationsPanel = 7,
        StrategyPanel = 8
    }

    public enum UiTimeSpeedLevel : byte
    {
        Normal = 0,
        X2 = 1,
        X3 = 2,
        X5 = 3
    }

    /// <summary>Маркер singleton полной UI/UX runtime-симуляции.</summary>
    public struct UiUxSimulationSingleton : IComponentData
    {
    }

    /// <summary>Сводное состояние интерфейса, доступности и уведомлений.</summary>
    public struct UiUxSimulationState : IComponentData
    {
        public uint LastProcessedDay;
        public uint LastNotificationId;
        public uint NotificationsTotal;

        public UiCameraLevel CameraLevel;
        public UiTimeSpeedLevel TimeSpeed;
        public byte IsPaused;
        public byte BuildModeActive;
        public byte AutoPauseOnCritical;

        public UiColorBlindMode ColorBlindMode;
        public UiContrastMode ContrastMode;
        public byte FontScalePercent;
        public byte AnimationsEnabled;
        public byte SoundSignalsEnabled;

        public UiResourceBand LastFoodBand;
        public UiResourceBand LastEnergyBand;
        public UiResourceBand LastSupplyBand;

        public uint HotkeyActivationsToday;
        public uint HotkeyActivationsTotal;
        public uint NotificationsCriticalToday;
        public uint NotificationsImportantToday;
        public uint NotificationsInfoToday;
        public uint NotificationsAchievementToday;
        public uint NotificationsActive;

        public uint LastSeenTechnologiesUnlocked;
        public uint LastSeenQuestsCompleted;
        public uint LastSeenBattlesTotal;

        public float ResourceStress01;
        public float HudLoad01;
        public float LastWeatherSeverity01;
    }

    /// <summary>Уведомление в HUD-ленте.</summary>
    public struct UiNotificationEntry : IBufferElementData
    {
        public uint NotificationId;
        public UiNotificationType Type;
        public uint CreatedDay;
        public uint ExpiresDay;
        public byte HasAction;
        public FixedString64Bytes PrimaryAction;
        public FixedString128Bytes Message;
    }

    /// <summary>Горячая клавиша в справке и для телеметрии/доступности.</summary>
    public struct UiHotkeyBindingEntry : IBufferElementData
    {
        public UiHotkeyGroup Group;
        public byte Enabled;
        public FixedString32Bytes ActionName;
        public FixedString16Bytes KeyChord;
    }

    /// <summary>Видимость панелей по уровням камеры.</summary>
    public struct UiPanelStateEntry : IBufferElementData
    {
        public UiPanelKind Panel;
        public UiCameraLevel MinCamera;
        public UiCameraLevel MaxCamera;
        public byte IsVisible;
    }

    /// <summary>Индикатор ресурса верхней панели.</summary>
    public struct UiResourceIndicatorEntry : IBufferElementData
    {
        public ResourceId Resource;
        public float Amount;
        public float DeltaPerDay;
        public UiResourceBand Band;
    }
}
