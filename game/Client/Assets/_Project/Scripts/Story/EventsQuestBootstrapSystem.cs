using Unity.Collections;
using Unity.Entities;

namespace ColonyConquest.Story
{
    /// <summary>Инициализирует full-runtime событий/квестов: каталог событий, буферы истории/квестов/арк.</summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct EventsQuestBootstrapSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            if (SystemAPI.HasSingleton<StorySimulationSingleton>())
                return;

            var em = state.EntityManager;
            var e = em.CreateEntity();
            em.AddComponent<StorySimulationSingleton>(e);
            em.AddComponent(e, new StorySimulationState
            {
                LastProcessedDay = uint.MaxValue,
                LastRuntimeEventId = 0,
                LastQuestId = 0,
                NextDirectorEventDay = 1,
                DirectorEventsTriggeredTotal = 0,
                TriggeredEventsImportedTotal = 0,
                ActiveEventsCount = 0,
                PersonalEventsTotal = 0,
                GlobalEventsTotal = 0,
                QuestsActive = 0,
                QuestsCompletedTotal = 0,
                QuestsFailedTotal = 0,
                QuestsProceduralGeneratedTotal = 0,
                PersonalArcsActive = 0,
                StoryArcBeatsTotal = 0,
                AverageQuestProgress01 = 0f,
                StoryTension01 = 0.5f
            });

            var defs = em.AddBuffer<StoryEventDefinitionEntry>(e);
            var cooldowns = em.AddBuffer<StoryEventCooldownEntry>(e);
            em.AddBuffer<StoryActiveEventEntry>(e);
            em.AddBuffer<StoryEventHistoryEntry>(e);
            em.AddBuffer<QuestRecordEntry>(e);
            em.AddBuffer<PersonalStoryArcEntry>(e);

            AddDefinition(ref defs, 0xA101, StoryEventKind.Cyclic, StoryEventCategory.NaturalDisaster, 1, 5, 1.2f,
                0f, 0f, 0f, 0f, 0f, 100f, 3, 2, 0.40f, true, false, false, "season-flood");
            AddDefinition(ref defs, 0xA102, StoryEventKind.Cyclic, StoryEventCategory.NaturalDisaster, 1, 5, 1.1f,
                0f, 0f, 0f, 0f, 0f, 100f, 4, 8, 0.45f, true, false, false, "season-drought");
            AddDefinition(ref defs, 0xA103, StoryEventKind.Random, StoryEventCategory.NaturalDisaster, 1, 5, 0.8f,
                0f, 0f, 0f, 0f, 20f, 100f, 6, 1, 0.55f, true, false, false, "earthquake");
            AddDefinition(ref defs, 0xA104, StoryEventKind.Historical, StoryEventCategory.NaturalDisaster, 4, 5, 0.7f,
                0f, 0f, 0f, 40f, 25f, 100f, 8, 2, 0.60f, true, false, false, "magnetic-storm");

            AddDefinition(ref defs, 0xA201, StoryEventKind.Triggered, StoryEventCategory.Military, 1, 5, 1.5f,
                20f, 0f, 0f, 0f, 20f, 100f, 3, 1, 0.50f, true, false, false, "raid");
            AddDefinition(ref defs, 0xA202, StoryEventKind.Triggered, StoryEventCategory.Military, 2, 5, 1.2f,
                35f, 0f, 0f, 0f, 30f, 100f, 5, 2, 0.60f, true, false, false, "siege");
            AddDefinition(ref defs, 0xA203, StoryEventKind.Personal, StoryEventCategory.Military, 3, 5, 0.7f,
                10f, 0f, 0f, 0f, 35f, 100f, 6, 1, 0.45f, true, true, false, "betrayal");
            AddDefinition(ref defs, 0xA204, StoryEventKind.Triggered, StoryEventCategory.Military, 3, 5, 0.8f,
                25f, 0f, 0f, 10f, 40f, 100f, 5, 1, 0.50f, true, false, false, "sabotage");

            AddDefinition(ref defs, 0xA301, StoryEventKind.Personal, StoryEventCategory.Social, 1, 5, 1.4f,
                0f, 0f, 25f, 0f, 0f, 90f, 4, 1, 0.35f, false, true, false, "wedding");
            AddDefinition(ref defs, 0xA302, StoryEventKind.Personal, StoryEventCategory.Social, 1, 5, 1.3f,
                0f, 0f, 20f, 0f, 0f, 85f, 2, 1, 0.30f, false, true, false, "birth");
            AddDefinition(ref defs, 0xA303, StoryEventKind.Personal, StoryEventCategory.Social, 1, 5, 0.9f,
                0f, 0f, 0f, 0f, 35f, 100f, 2, 1, 0.55f, false, true, false, "death");
            AddDefinition(ref defs, 0xA304, StoryEventKind.Triggered, StoryEventCategory.Social, 1, 5, 1.0f,
                0f, 0f, 0f, 0f, 35f, 100f, 3, 1, 0.45f, false, false, false, "conflict");

            AddDefinition(ref defs, 0xA401, StoryEventKind.Triggered, StoryEventCategory.Economic, 2, 5, 1.0f,
                70f, 20f, 30f, 10f, 0f, 80f, 6, 3, 0.40f, true, false, false, "economic-boom");
            AddDefinition(ref defs, 0xA402, StoryEventKind.Triggered, StoryEventCategory.Economic, 1, 5, 1.3f,
                0f, 0f, 0f, 0f, 35f, 100f, 4, 3, 0.60f, true, false, false, "economic-crisis");
            AddDefinition(ref defs, 0xA403, StoryEventKind.Cyclic, StoryEventCategory.Economic, 2, 5, 0.9f,
                50f, 0f, 0f, 0f, 0f, 100f, 5, 2, 0.45f, true, false, false, "inflation");
            AddDefinition(ref defs, 0xA404, StoryEventKind.Cyclic, StoryEventCategory.Economic, 2, 5, 0.8f,
                0f, 0f, 0f, 0f, 0f, 100f, 5, 2, 0.35f, true, false, false, "deflation");

            AddDefinition(ref defs, 0xA501, StoryEventKind.Triggered, StoryEventCategory.Technology, 3, 5, 1.1f,
                30f, 20f, 25f, 20f, 10f, 100f, 5, 1, 0.45f, true, false, false, "research-breakthrough");
            AddDefinition(ref defs, 0xA502, StoryEventKind.Random, StoryEventCategory.Technology, 3, 5, 0.8f,
                25f, 15f, 20f, 30f, 10f, 100f, 7, 1, 0.50f, true, false, false, "lab-accident");
            AddDefinition(ref defs, 0xA503, StoryEventKind.Triggered, StoryEventCategory.Technology, 4, 5, 0.7f,
                40f, 20f, 20f, 35f, 15f, 100f, 6, 1, 0.40f, true, false, false, "tech-leak");

            AddDefinition(ref defs, 0xA601, StoryEventKind.Global, StoryEventCategory.Global, 1, 5, 0.25f,
                0f, 0f, 0f, 0f, 40f, 100f, 12, 5, 0.70f, true, false, true, "plague");
            AddDefinition(ref defs, 0xA602, StoryEventKind.Global, StoryEventCategory.Global, 3, 5, 0.20f,
                10f, 0f, 0f, 25f, 50f, 100f, 14, 6, 0.75f, true, false, true, "world-war");
            AddDefinition(ref defs, 0xA603, StoryEventKind.Global, StoryEventCategory.Global, 5, 5, 0.12f,
                20f, 10f, 10f, 60f, 60f, 100f, 20, 7, 0.85f, true, false, true, "alien-incursion");

            cooldowns.Clear();
        }

        private static void AddDefinition(ref DynamicBuffer<StoryEventDefinitionEntry> defs, uint id, StoryEventKind kind,
            StoryEventCategory category, byte minEra, byte maxEra, float baseWeight, float minWealth, float minSecurity,
            float minStability, float minProgress, float minTension, float maxTension, short cooldownDays,
            short durationDays, float severity01, bool canStartQuest, bool isPersonal, bool isGlobal,
            in FixedString64Bytes name)
        {
            defs.Add(new StoryEventDefinitionEntry
            {
                EventDefinitionId = id,
                Kind = kind,
                Category = category,
                MinEra = minEra,
                MaxEra = maxEra,
                BaseWeight = baseWeight,
                MinWealth = minWealth,
                MinSecurity = minSecurity,
                MinStability = minStability,
                MinProgress = minProgress,
                MinTension = minTension,
                MaxTension = maxTension,
                CooldownDays = cooldownDays,
                DurationDays = durationDays,
                SeverityBase01 = severity01,
                CanStartQuest = canStartQuest ? (byte)1 : (byte)0,
                IsPersonal = isPersonal ? (byte)1 : (byte)0,
                IsGlobal = isGlobal ? (byte)1 : (byte)0,
                DebugName = name
            });
        }
    }
}
