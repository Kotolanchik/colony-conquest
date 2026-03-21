namespace ColonyConquest.Story
{
    /// <summary>Типы событий; <c>spec/events_quests_spec.md</c> §1.2.</summary>
    public enum StoryEventKind : byte
    {
        Random = 0,
        Triggered = 1,
        Cyclic = 2,
        Historical = 3,
        Personal = 4,
        Global = 5
    }

    /// <summary>Типы квестов; <c>spec/events_quests_spec.md</c> §5.1.</summary>
    public enum QuestKind : byte
    {
        Main = 0,
        Side = 1,
        Personal = 2,
        Faction = 3,
        Global = 4,
        Procedural = 5
    }
}
