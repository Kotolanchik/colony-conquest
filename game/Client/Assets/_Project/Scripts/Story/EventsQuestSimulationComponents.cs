using ColonyConquest.Economy;
using Unity.Collections;
using Unity.Entities;

namespace ColonyConquest.Story
{
    public enum StoryEventCategory : byte
    {
        NaturalDisaster = 1,
        Military = 2,
        Social = 3,
        Economic = 4,
        Technology = 5,
        Global = 6
    }

    public enum QuestTemplateId : byte
    {
        Delivery = 1,
        Escort = 2,
        Eliminate = 3,
        Find = 4,
        Defend = 5,
        Investigate = 6
    }

    public enum QuestStatus : byte
    {
        Active = 0,
        Completed = 1,
        Failed = 2
    }

    public enum PersonalStoryArchetype : byte
    {
        Hero = 1,
        Villain = 2,
        Victim = 3,
        Sage = 4,
        Lovers = 5,
        Rebirth = 6
    }

    /// <summary>Маркер singleton полной событийно-квестовой системы.</summary>
    public struct StorySimulationSingleton : IComponentData
    {
    }

    /// <summary>Сводное состояние AI Director и контура событий/квестов.</summary>
    public struct StorySimulationState : IComponentData
    {
        public uint LastProcessedDay;
        public uint LastRuntimeEventId;
        public uint LastQuestId;

        public uint NextDirectorEventDay;
        public uint DirectorEventsTriggeredTotal;
        public uint TriggeredEventsImportedTotal;
        public uint ActiveEventsCount;
        public uint PersonalEventsTotal;
        public uint GlobalEventsTotal;

        public uint QuestsActive;
        public uint QuestsCompletedTotal;
        public uint QuestsFailedTotal;
        public uint QuestsProceduralGeneratedTotal;

        public uint PersonalArcsActive;
        public uint StoryArcBeatsTotal;

        public float AverageQuestProgress01;
        public float StoryTension01;
    }

    /// <summary>Каталожная дефиниция события для AI Director weighted selection.</summary>
    public struct StoryEventDefinitionEntry : IBufferElementData
    {
        public uint EventDefinitionId;
        public StoryEventKind Kind;
        public StoryEventCategory Category;

        public byte MinEra;
        public byte MaxEra;
        public float BaseWeight;

        public float MinWealth;
        public float MinSecurity;
        public float MinStability;
        public float MinProgress;
        public float MinTension;
        public float MaxTension;

        public short CooldownDays;
        public short DurationDays;
        public float SeverityBase01;
        public byte CanStartQuest;
        public byte IsPersonal;
        public byte IsGlobal;
        public FixedString64Bytes DebugName;
    }

    /// <summary>Runtime-cooldown события после срабатывания.</summary>
    public struct StoryEventCooldownEntry : IBufferElementData
    {
        public uint EventDefinitionId;
        public short DaysRemaining;
    }

    /// <summary>Активное событие с длительностью и severity.</summary>
    public struct StoryActiveEventEntry : IBufferElementData
    {
        public uint RuntimeEventId;
        public uint EventDefinitionId;
        public StoryEventKind Kind;
        public StoryEventCategory Category;
        public uint StartedDay;
        public short DaysRemaining;
        public float Severity01;
        public byte SourcePolicy;
        public FixedString64Bytes DebugName;
    }

    /// <summary>История обработанных событий (timeline).</summary>
    public struct StoryEventHistoryEntry : IBufferElementData
    {
        public uint RuntimeEventId;
        public uint EventDefinitionId;
        public StoryEventKind Kind;
        public StoryEventCategory Category;
        public uint DayIndex;
        public float Severity01;
        public byte Outcome; // 0 started, 1 completed, 2 failed
    }

    /// <summary>Запись активного/завершённого квеста.</summary>
    public struct QuestRecordEntry : IBufferElementData
    {
        public uint QuestId;
        public QuestKind Kind;
        public QuestTemplateId Template;
        public QuestStatus Status;
        public byte Difficulty;
        public byte Stage;
        public float Progress01;
        public uint LinkedEventDefinitionId;
        public uint LinkedSettlerId;
        public uint StartDay;
        public uint ExpireDay;
        public ResourceId RewardResource;
        public float RewardAmount;
        public FixedString64Bytes Title;
    }

    /// <summary>Персональные арки поселенцев по архетипам §4.2.</summary>
    public struct PersonalStoryArcEntry : IBufferElementData
    {
        public uint SettlerId;
        public PersonalStoryArchetype Archetype;
        public uint BeatsCompleted;
        public uint LastBeatDay;
        public float Impact01;
        public FixedString64Bytes Nickname;
    }
}
