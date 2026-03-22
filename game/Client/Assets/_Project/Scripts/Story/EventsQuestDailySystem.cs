using ColonyConquest.Analytics;
using ColonyConquest.Core;
using ColonyConquest.Diplomacy;
using ColonyConquest.Ecology;
using ColonyConquest.Economy;
using ColonyConquest.Military;
using ColonyConquest.Politics;
using ColonyConquest.Settlers;
using ColonyConquest.Simulation;
using ColonyConquest.Technology;
using ColonyConquest.WorldMap;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace ColonyConquest.Story
{
    /// <summary>
    /// Полный суточный цикл событий/квестов: AI Director выбор событий, применение эффектов,
    /// персональные истории и прогресс квестов.
    /// </summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(ColonyCalendarAdvanceSystem))]
    [UpdateAfter(typeof(AiDirectorPolicyUpdateSystem))]
    [UpdateAfter(typeof(EconomySimulationDailySystem))]
    [UpdateAfter(typeof(SettlerSimulationDailySystem))]
    [UpdateAfter(typeof(MilitarySimulationDailySystem))]
    [UpdateAfter(typeof(DiplomacyDailySystem))]
    [UpdateAfter(typeof(PoliticalDailySystem))]
    [UpdateAfter(typeof(WorldMapDailySimulationSystem))]
    [UpdateAfter(typeof(EcologySimulationDailySystem))]
    [UpdateBefore(typeof(StoryEventPipelineSystem))]
    public partial struct EventsQuestDailySystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameCalendarState>();
            state.RequireForUpdate<SimulationRootState>();
            state.RequireForUpdate<AiDirectorDimensionsState>();
            state.RequireForUpdate<AiDirectorPolicyState>();
            state.RequireForUpdate<StorySimulationSingleton>();
            state.RequireForUpdate<StorySimulationState>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var day = SystemAPI.GetSingleton<GameCalendarState>().DayIndex;
            var tick = SystemAPI.GetSingleton<SimulationRootState>().SimulationTick;
            ref var sim = ref SystemAPI.GetSingletonRW<StorySimulationState>().ValueRW;
            if (sim.LastProcessedDay == day)
                return;
            sim.LastProcessedDay = day;

            var dims = SystemAPI.GetSingleton<AiDirectorDimensionsState>();
            var policy = SystemAPI.GetSingleton<AiDirectorPolicyState>().ActivePolicy;
            var era = SystemAPI.HasSingleton<ColonyTechProgressState>()
                ? SystemAPI.GetSingleton<ColonyTechProgressState>().CurrentEra
                : TechEraId.Era1_Foundation;

            var defs = SystemAPI.GetSingletonBuffer<StoryEventDefinitionEntry>(ref state);
            var cooldowns = SystemAPI.GetSingletonBuffer<StoryEventCooldownEntry>(ref state);
            var activeEvents = SystemAPI.GetSingletonBuffer<StoryActiveEventEntry>(ref state);
            var history = SystemAPI.GetSingletonBuffer<StoryEventHistoryEntry>(ref state);
            var quests = SystemAPI.GetSingletonBuffer<QuestRecordEntry>(ref state);
            var arcs = SystemAPI.GetSingletonBuffer<PersonalStoryArcEntry>(ref state);
            var hasStock = SystemAPI.HasSingleton<ResourceStockpileSingleton>();
            var stock = hasStock ? SystemAPI.GetSingletonBuffer<ResourceStockEntry>(ref state) : default;
            var directorTriggeredToday = 0u;

            UpdateCooldowns(ref cooldowns);

            var climateRisk01 = SystemAPI.HasSingleton<EcologySimulationState>()
                ? math.saturate(SystemAPI.GetSingleton<EcologySimulationState>().ExtremeWeatherRisk01)
                : 0f;
            var militaryPressure01 = SystemAPI.HasSingleton<MilitarySimulationState>()
                ? math.saturate(SystemAPI.GetSingleton<MilitarySimulationState>().AverageSuppression01)
                : 0f;

            ProcessActiveEvents(ref state, day, ref sim, ref activeEvents, ref history, climateRisk01, militaryPressure01);
            var importedTriggeredToday = ImportTriggeredQueue(ref state, day, tick, ref sim, ref quests, ref history, ref activeEvents, ref cooldowns, hasStock,
                ref stock);

            if (day >= sim.NextDirectorEventDay)
            {
                var selected = SelectEventDefinition(ref defs, ref cooldowns, dims, policy, era, day);
                if (selected.EventDefinitionId != 0)
                {
                    sim.LastRuntimeEventId++;
                    var severity01 = EventsQuestSimulationMath.ComputeSeverity01(selected.SeverityBase01, dims, climateRisk01,
                        militaryPressure01);
                    var active = new StoryActiveEventEntry
                    {
                        RuntimeEventId = sim.LastRuntimeEventId,
                        EventDefinitionId = selected.EventDefinitionId,
                        Kind = selected.Kind,
                        Category = selected.Category,
                        StartedDay = day,
                        DaysRemaining = selected.DurationDays > 1 ? selected.DurationDays : (short)1,
                        Severity01 = severity01,
                        SourcePolicy = (byte)policy,
                        DebugName = selected.DebugName
                    };
                    activeEvents.Add(active);
                    history.Add(new StoryEventHistoryEntry
                    {
                        RuntimeEventId = sim.LastRuntimeEventId,
                        EventDefinitionId = selected.EventDefinitionId,
                        Kind = selected.Kind,
                        Category = selected.Category,
                        DayIndex = day,
                        Severity01 = severity01,
                        Outcome = 0
                    });
                    AddOrRefreshCooldown(ref cooldowns, selected.EventDefinitionId, selected.CooldownDays);
                    ApplyEventImmediateEffects(ref state, selected.Category, severity01, hasStock, ref stock);
                    sim.DirectorEventsTriggeredTotal++;
                    directorTriggeredToday++;
                    if (selected.IsPersonal != 0)
                        sim.PersonalEventsTotal++;
                    if (selected.IsGlobal != 0)
                        sim.GlobalEventsTotal++;

                    var rng = Random.CreateFromIndex(day * 2654435761u + selected.EventDefinitionId);
                    if (selected.CanStartQuest != 0 && rng.NextFloat() < 0.75f)
                        CreateQuestFromEvent(ref state, day, ref sim, ref quests, selected, severity01, rng.NextFloat());
                }

                var cadence = math.lerp(4f, 1f, math.saturate(dims.Tension0to100 / 100f));
                sim.NextDirectorEventDay = day + (uint)math.max(1f, math.round(cadence));
            }

            UpdateQuests(ref state, day, ref sim, ref quests, hasStock, ref stock);
            UpdatePersonalStories(ref state, day, ref sim, ref arcs);

            sim.ActiveEventsCount = (uint)activeEvents.Length;
            sim.QuestsActive = CountQuestsByStatus(ref quests, QuestStatus.Active);
            sim.PersonalArcsActive = (uint)arcs.Length;
            sim.AverageQuestProgress01 = ComputeAverageQuestProgress(ref quests);
            sim.StoryTension01 = math.saturate(dims.Tension0to100 / 100f);

            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.EventsDirectorPolicy, (byte)policy);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.EventsTriggeredToday,
                directorTriggeredToday + importedTriggeredToday);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.EventsActiveCount, sim.ActiveEventsCount);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.EventsPersonalTotal, sim.PersonalEventsTotal);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.EventsGlobalTotal, sim.GlobalEventsTotal);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.QuestsActiveCount, sim.QuestsActive);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.QuestsCompletedTotal, sim.QuestsCompletedTotal);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.QuestsFailedTotal, sim.QuestsFailedTotal);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.QuestsProceduralGeneratedTotal,
                sim.QuestsProceduralGeneratedTotal);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.StoryArcActiveCount, sim.PersonalArcsActive);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.StoryArcBeatsTotal, sim.StoryArcBeatsTotal);
            AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.StoryTension01, sim.StoryTension01);
        }

        private static void UpdateCooldowns(ref DynamicBuffer<StoryEventCooldownEntry> cooldowns)
        {
            for (var i = cooldowns.Length - 1; i >= 0; i--)
            {
                var c = cooldowns[i];
                c.DaysRemaining--;
                if (c.DaysRemaining <= 0)
                {
                    cooldowns.RemoveAt(i);
                    continue;
                }
                cooldowns[i] = c;
            }
        }

        private static void ProcessActiveEvents(ref SystemState state, uint day, ref StorySimulationState sim,
            ref DynamicBuffer<StoryActiveEventEntry> activeEvents, ref DynamicBuffer<StoryEventHistoryEntry> history,
            float climateRisk01, float militaryPressure01)
        {
            for (var i = activeEvents.Length - 1; i >= 0; i--)
            {
                var active = activeEvents[i];
                ApplyEventOngoingEffects(ref state, active.Category, active.Severity01, climateRisk01, militaryPressure01);
                active.DaysRemaining--;
                if (active.DaysRemaining <= 0)
                {
                    history.Add(new StoryEventHistoryEntry
                    {
                        RuntimeEventId = active.RuntimeEventId,
                        EventDefinitionId = active.EventDefinitionId,
                        Kind = active.Kind,
                        Category = active.Category,
                        DayIndex = day,
                        Severity01 = active.Severity01,
                        Outcome = 1
                    });
                    activeEvents.RemoveAt(i);
                    continue;
                }
                activeEvents[i] = active;
            }

            sim.ActiveEventsCount = (uint)activeEvents.Length;
        }

        private static uint ImportTriggeredQueue(ref SystemState state, uint day, ulong tick, ref StorySimulationState sim,
            ref DynamicBuffer<QuestRecordEntry> quests, ref DynamicBuffer<StoryEventHistoryEntry> history,
            ref DynamicBuffer<StoryActiveEventEntry> activeEvents, ref DynamicBuffer<StoryEventCooldownEntry> cooldowns, bool hasStock,
            ref DynamicBuffer<ResourceStockEntry> stock)
        {
            if (!SystemAPI.HasSingleton<StoryEventQueueSingleton>())
                return 0u;

            var queue = SystemAPI.GetSingletonBuffer<GameEventQueueEntry>(ref state);
            if (queue.Length == 0)
                return 0u;

            var importedToday = 0u;
            var rng = Random.CreateFromIndex((uint)tick + 0xA11CEu);
            for (var i = 0; i < queue.Length; i++)
            {
                var entry = queue[i];
                var category = CategorizeQueueEntry(in entry);
                var severity01 = math.saturate(0.35f + (entry.Kind == StoryEventKind.Global ? 0.35f : 0f) + rng.NextFloat(0f, 0.2f));

                sim.LastRuntimeEventId++;
                history.Add(new StoryEventHistoryEntry
                {
                    RuntimeEventId = sim.LastRuntimeEventId,
                    EventDefinitionId = entry.EventDefinitionId,
                    Kind = entry.Kind,
                    Category = category,
                    DayIndex = day,
                    Severity01 = severity01,
                    Outcome = 0
                });
                AddOrRefreshCooldown(ref cooldowns, entry.EventDefinitionId, 2);

                activeEvents.Add(new StoryActiveEventEntry
                {
                    RuntimeEventId = sim.LastRuntimeEventId,
                    EventDefinitionId = entry.EventDefinitionId,
                    Kind = entry.Kind,
                    Category = category,
                    StartedDay = day,
                    DaysRemaining = 1,
                    Severity01 = severity01,
                    SourcePolicy = 0,
                    DebugName = entry.DebugLabel
                });

                AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement,
                    AnalyticsMetricIds.IntegrationStoryEventBase + (uint)entry.Kind, entry.EventDefinitionId);

                if (entry.Kind == StoryEventKind.Personal)
                    sim.PersonalEventsTotal++;
                if (entry.Kind == StoryEventKind.Global)
                    sim.GlobalEventsTotal++;

                if (rng.NextFloat() < 0.4f)
                {
                    var syntheticDef = new StoryEventDefinitionEntry
                    {
                        EventDefinitionId = entry.EventDefinitionId,
                        Kind = entry.Kind,
                        Category = category,
                        SeverityBase01 = severity01,
                        CanStartQuest = 1,
                        IsPersonal = entry.Kind == StoryEventKind.Personal ? (byte)1 : (byte)0,
                        IsGlobal = entry.Kind == StoryEventKind.Global ? (byte)1 : (byte)0,
                        DebugName = entry.DebugLabel
                    };
                    CreateQuestFromEvent(ref state, day, ref sim, ref quests, syntheticDef, severity01, rng.NextFloat());
                }

                ApplyEventImmediateEffects(ref state, category, severity01, hasStock, ref stock);
                importedToday++;
            }

            queue.Clear();
            sim.TriggeredEventsImportedTotal += importedToday;
            return importedToday;
        }

        private static StoryEventDefinitionEntry SelectEventDefinition(ref DynamicBuffer<StoryEventDefinitionEntry> defs,
            ref DynamicBuffer<StoryEventCooldownEntry> cooldowns, in AiDirectorDimensionsState dims, AiDirectorPolicyKind policy,
            TechEraId era, uint day)
        {
            var totalWeight = 0f;
            for (var i = 0; i < defs.Length; i++)
            {
                var def = defs[i];
                if (!EventsQuestSimulationMath.IsEligible(in def, in dims, era))
                    continue;
                if (IsCooldownActive(ref cooldowns, def.EventDefinitionId))
                    continue;
                totalWeight += EventsQuestSimulationMath.ComputeEventWeight(in def, in dims, policy);
            }

            if (totalWeight <= 1e-4f)
                return default;

            var rng = Random.CreateFromIndex(day * 1103515245u + (uint)policy * 12345u + 0xBADC0DEu);
            var target = rng.NextFloat(0f, totalWeight);
            var cumulative = 0f;
            for (var i = 0; i < defs.Length; i++)
            {
                var def = defs[i];
                if (!EventsQuestSimulationMath.IsEligible(in def, in dims, era))
                    continue;
                if (IsCooldownActive(ref cooldowns, def.EventDefinitionId))
                    continue;
                cumulative += EventsQuestSimulationMath.ComputeEventWeight(in def, in dims, policy);
                if (target <= cumulative)
                    return def;
            }

            return default;
        }

        private static bool IsCooldownActive(ref DynamicBuffer<StoryEventCooldownEntry> cooldowns, uint eventDefinitionId)
        {
            for (var i = 0; i < cooldowns.Length; i++)
            {
                if (cooldowns[i].EventDefinitionId == eventDefinitionId && cooldowns[i].DaysRemaining > 0)
                    return true;
            }

            return false;
        }

        private static void AddOrRefreshCooldown(ref DynamicBuffer<StoryEventCooldownEntry> cooldowns, uint eventDefinitionId,
            short days)
        {
            var targetDays = days > 1 ? days : (short)1;
            for (var i = 0; i < cooldowns.Length; i++)
            {
                if (cooldowns[i].EventDefinitionId != eventDefinitionId)
                    continue;
                var entry = cooldowns[i];
                entry.DaysRemaining = entry.DaysRemaining > targetDays ? entry.DaysRemaining : targetDays;
                cooldowns[i] = entry;
                return;
            }

            cooldowns.Add(new StoryEventCooldownEntry
            {
                EventDefinitionId = eventDefinitionId,
                DaysRemaining = targetDays
            });
        }

        private static StoryEventCategory CategorizeQueueEntry(in GameEventQueueEntry entry)
        {
            if (entry.Kind == StoryEventKind.Global)
                return StoryEventCategory.Global;
            if (entry.Kind == StoryEventKind.Personal)
                return StoryEventCategory.Social;

            var label = entry.DebugLabel.ToString();
            if (label.Contains("mil") || label.Contains("defense") || label.Contains("raid") || label.Contains("siege"))
                return StoryEventCategory.Military;
            if (label.Contains("eco") || label.Contains("pollution") || label.Contains("climate"))
                return StoryEventCategory.NaturalDisaster;
            if (label.Contains("economy") || label.Contains("manufacturing") || label.Contains("trade"))
                return StoryEventCategory.Economic;
            if (label.Contains("tech") || label.Contains("research"))
                return StoryEventCategory.Technology;
            if (label.Contains("crime") || label.Contains("festival") || label.Contains("settler") || label.Contains("bio"))
                return StoryEventCategory.Social;
            if (label.Contains("world") || label.Contains("special-site"))
                return StoryEventCategory.Global;
            return StoryEventCategory.Social;
        }

        private static void ApplyEventImmediateEffects(ref SystemState state, StoryEventCategory category, float severity01, bool hasStock,
            ref DynamicBuffer<ResourceStockEntry> stock)
        {
            switch (category)
            {
                case StoryEventCategory.NaturalDisaster:
                    if (SystemAPI.HasSingleton<ColonyEcologyIndicatorsState>())
                    {
                        ref var eco = ref SystemAPI.GetSingletonRW<ColonyEcologyIndicatorsState>().ValueRW;
                        eco.AirQuality01 = math.saturate(eco.AirQuality01 - severity01 * 0.05f);
                        eco.WaterQuality01 = math.saturate(eco.WaterQuality01 - severity01 * 0.04f);
                        eco.SoilFertilityIndicator01 = math.saturate(eco.SoilFertilityIndicator01 - severity01 * 0.03f);
                    }
                    if (hasStock)
                    {
                        ResourceStockpileOps.TryConsume(ref stock, ResourceId.CropWheat, severity01 * 25f);
                        ResourceStockpileOps.TryConsume(ref stock, ResourceId.LivestockMeat, severity01 * 10f);
                    }
                    break;
                case StoryEventCategory.Military:
                    if (SystemAPI.HasSingleton<MilitarySimulationState>())
                    {
                        ref var m = ref SystemAPI.GetSingletonRW<MilitarySimulationState>().ValueRW;
                        m.CasualtiesFriendlyWounded += (uint)math.round(severity01 * 12f);
                        m.CasualtiesEnemyKilled += (uint)math.round(severity01 * 8f);
                        m.AverageMorale01 = math.saturate(m.AverageMorale01 - severity01 * 0.04f);
                    }
                    if (hasStock)
                        ResourceStockpileOps.TryConsume(ref stock, ResourceId.Gunpowder, severity01 * 12f);
                    break;
                case StoryEventCategory.Social:
                    if (SystemAPI.HasSingleton<SettlerSimulationState>())
                    {
                        ref var s = ref SystemAPI.GetSingletonRW<SettlerSimulationState>().ValueRW;
                        s.AverageMood = math.clamp(s.AverageMood + (0.5f - severity01) * 15f, -100f, 100f);
                        s.AverageStress = math.clamp(s.AverageStress + severity01 * 8f - 2f, 0f, 100f);
                    }
                    break;
                case StoryEventCategory.Economic:
                    if (SystemAPI.HasSingleton<EconomySimulationState>())
                    {
                        ref var eco = ref SystemAPI.GetSingletonRW<EconomySimulationState>().ValueRW;
                        eco.InflationPercent = math.clamp(eco.InflationPercent + (severity01 - 0.35f) * 2.5f, 0f, 60f);
                        eco.TradeBalance -= severity01 * 80f;
                    }
                    break;
                case StoryEventCategory.Technology:
                    if (SystemAPI.HasSingleton<ColonyTechProgressState>())
                    {
                        ref var tech = ref SystemAPI.GetSingletonRW<ColonyTechProgressState>().ValueRW;
                        tech.ResearchPointsPerDay = math.max(1f, tech.ResearchPointsPerDay * (1f + (0.25f - severity01) * 0.2f));
                        tech.ResearchPointsAccumulated = math.max(0f, tech.ResearchPointsAccumulated - severity01 * 8f);
                    }
                    break;
                case StoryEventCategory.Global:
                    ApplyGlobalShock(ref state, severity01, hasStock, ref stock);
                    break;
            }
        }

        private static void ApplyEventOngoingEffects(ref SystemState state, StoryEventCategory category, float severity01,
            float climateRisk01, float militaryPressure01)
        {
            switch (category)
            {
                case StoryEventCategory.NaturalDisaster:
                    if (SystemAPI.HasSingleton<ColonyPollutionSummaryState>())
                    {
                        ref var pollution = ref SystemAPI.GetSingletonRW<ColonyPollutionSummaryState>().ValueRW;
                        pollution.CombinedPollutionPercent0to100 = math.clamp(
                            pollution.CombinedPollutionPercent0to100 + severity01 * (2f + climateRisk01 * 2f), 0f, 100f);
                    }
                    break;
                case StoryEventCategory.Military:
                    if (SystemAPI.HasSingleton<MilitarySimulationState>())
                    {
                        ref var m = ref SystemAPI.GetSingletonRW<MilitarySimulationState>().ValueRW;
                        m.AverageSuppression01 = math.saturate(m.AverageSuppression01 + severity01 * 0.03f + militaryPressure01 * 0.02f);
                        m.CombatReadiness01 = math.saturate(m.CombatReadiness01 - severity01 * 0.02f);
                    }
                    break;
                case StoryEventCategory.Social:
                    if (SystemAPI.HasSingleton<SettlerSimulationState>())
                    {
                        ref var s = ref SystemAPI.GetSingletonRW<SettlerSimulationState>().ValueRW;
                        s.ColonyMorale01 = math.saturate(s.ColonyMorale01 + (0.5f - severity01) * 0.02f);
                    }
                    break;
                case StoryEventCategory.Economic:
                    if (SystemAPI.HasSingleton<EconomySimulationState>())
                    {
                        ref var e = ref SystemAPI.GetSingletonRW<EconomySimulationState>().ValueRW;
                        e.Unemployment01 = math.saturate(e.Unemployment01 + severity01 * 0.01f);
                    }
                    break;
            }
        }

        private static void ApplyGlobalShock(ref SystemState state, float severity01, bool hasStock,
            ref DynamicBuffer<ResourceStockEntry> stock)
        {
            if (SystemAPI.HasSingleton<SettlerSimulationState>())
            {
                ref var s = ref SystemAPI.GetSingletonRW<SettlerSimulationState>().ValueRW;
                s.AverageMood = math.clamp(s.AverageMood - severity01 * 18f, -100f, 100f);
                s.ColonyMorale01 = math.saturate(s.ColonyMorale01 - severity01 * 0.08f);
                s.AverageStress = math.clamp(s.AverageStress + severity01 * 10f, 0f, 100f);
            }

            if (SystemAPI.HasSingleton<MilitarySimulationState>())
            {
                ref var m = ref SystemAPI.GetSingletonRW<MilitarySimulationState>().ValueRW;
                m.AverageMorale01 = math.saturate(m.AverageMorale01 - severity01 * 0.08f);
                m.CasualtiesFriendlyMia += (uint)math.round(severity01 * 4f);
            }

            if (SystemAPI.HasSingleton<EconomySimulationState>())
            {
                ref var e = ref SystemAPI.GetSingletonRW<EconomySimulationState>().ValueRW;
                e.InflationPercent = math.clamp(e.InflationPercent + severity01 * 3f, 0f, 70f);
                e.TradeBalance -= severity01 * 200f;
            }

            if (hasStock)
            {
                ResourceStockpileOps.TryConsume(ref stock, ResourceId.CropWheat, severity01 * 40f);
                ResourceStockpileOps.TryConsume(ref stock, ResourceId.PetroleumProducts, severity01 * 30f);
                ResourceStockpileOps.TryConsume(ref stock, ResourceId.Gunpowder, severity01 * 20f);
            }
        }

        private static void CreateQuestFromEvent(ref SystemState state, uint day, ref StorySimulationState sim,
            ref DynamicBuffer<QuestRecordEntry> quests, in StoryEventDefinitionEntry def, float severity01, float random01)
        {
            sim.LastQuestId++;
            var template = EventsQuestSimulationMath.PickQuestTemplate(def.Category, random01);
            var difficulty = EventsQuestSimulationMath.ComputeQuestDifficulty(severity01, random01);
            var duration = EventsQuestSimulationMath.ComputeQuestDurationDays(template, difficulty);
            var economyScale01 = 0.5f;
            if (SystemAPI.HasSingleton<AnalyticsLocalSnapshot>())
                economyScale01 = math.saturate(SystemAPI.GetSingleton<AnalyticsLocalSnapshot>().Economy.GdpPerCapita / 2000f);
            var reward = EventsQuestSimulationMath.ComputeQuestReward(severity01, difficulty, economyScale01);
            var rewardResource = PickRewardResource(def.Category);
            var kind = ResolveQuestKind(def.Kind, def.IsPersonal != 0, def.IsGlobal != 0);

            var linkedSettler = 0u;
            if (def.IsPersonal != 0 && SystemAPI.HasSingleton<SettlerSimulationState>())
            {
                var settlers = SystemAPI.GetSingleton<SettlerSimulationState>();
                if (settlers.LastSettlerId > 0)
                    linkedSettler = math.max(1u, settlers.LastSettlerId - (uint)math.floor(random01 * 6f));
            }

            quests.Add(new QuestRecordEntry
            {
                QuestId = sim.LastQuestId,
                Kind = kind,
                Template = template,
                Status = QuestStatus.Active,
                Difficulty = difficulty,
                Stage = 0,
                Progress01 = 0f,
                LinkedEventDefinitionId = def.EventDefinitionId,
                LinkedSettlerId = linkedSettler,
                StartDay = day,
                ExpireDay = day + duration,
                RewardResource = rewardResource,
                RewardAmount = reward,
                Title = BuildQuestTitle(def.DebugName, template)
            });
            sim.QuestsProceduralGeneratedTotal++;
        }

        private static QuestKind ResolveQuestKind(StoryEventKind eventKind, bool isPersonal, bool isGlobal)
        {
            if (isGlobal)
                return QuestKind.Global;
            if (isPersonal || eventKind == StoryEventKind.Personal)
                return QuestKind.Personal;
            if (eventKind == StoryEventKind.Historical)
                return QuestKind.Main;
            if (eventKind == StoryEventKind.Triggered)
                return QuestKind.Faction;
            return QuestKind.Procedural;
        }

        private static ResourceId PickRewardResource(StoryEventCategory category)
        {
            return category switch
            {
                StoryEventCategory.Military => ResourceId.Gunpowder,
                StoryEventCategory.NaturalDisaster => ResourceId.CropWheat,
                StoryEventCategory.Economic => ResourceId.GoldIngot,
                StoryEventCategory.Technology => ResourceId.ChemicalReagents,
                StoryEventCategory.Global => ResourceId.CompositeMaterials,
                _ => ResourceId.Cloth
            };
        }

        private static FixedString64Bytes BuildQuestTitle(in FixedString64Bytes eventName, QuestTemplateId template)
        {
            var prefix = template switch
            {
                QuestTemplateId.Delivery => "delivery",
                QuestTemplateId.Escort => "escort",
                QuestTemplateId.Eliminate => "eliminate",
                QuestTemplateId.Find => "find",
                QuestTemplateId.Defend => "defend",
                _ => "investigate"
            };
            return new FixedString64Bytes(prefix + "-" + eventName.ToString());
        }

        private static void UpdateQuests(ref SystemState state, uint day, ref StorySimulationState sim,
            ref DynamicBuffer<QuestRecordEntry> quests, bool hasStock, ref DynamicBuffer<ResourceStockEntry> stock)
        {
            var security01 = math.saturate(SystemAPI.GetSingleton<AiDirectorDimensionsState>().Security0to100 / 100f);
            var stability01 = math.saturate(SystemAPI.GetSingleton<AiDirectorDimensionsState>().Stability0to100 / 100f);
            var supplyAdequacy01 = SystemAPI.HasSingleton<MilitarySimulationState>()
                ? math.saturate(SystemAPI.GetSingleton<MilitarySimulationState>().SupplyAdequacy01)
                : 0.8f;
            var militaryReadiness01 = SystemAPI.HasSingleton<MilitarySimulationState>()
                ? math.saturate(SystemAPI.GetSingleton<MilitarySimulationState>().CombatReadiness01)
                : 0.7f;

            var rng = Random.CreateFromIndex(day * 2246822519u + 0x51D7u);
            for (var i = 0; i < quests.Length; i++)
            {
                var q = quests[i];
                if (q.Status != QuestStatus.Active)
                    continue;

                if (day > q.ExpireDay)
                {
                    q.Status = QuestStatus.Failed;
                    sim.QuestsFailedTotal++;
                    quests[i] = q;
                    continue;
                }

                var delta = EventsQuestSimulationMath.ComputeQuestProgressDelta01(q.Template, security01, stability01,
                    supplyAdequacy01, militaryReadiness01, rng.NextFloat());
                q.Progress01 = math.saturate(q.Progress01 + delta);
                if (q.Progress01 >= 0.999f)
                {
                    q.Progress01 = 1f;
                    q.Status = QuestStatus.Completed;
                    sim.QuestsCompletedTotal++;
                    if (hasStock)
                        ResourceStockpileOps.Add(ref stock, q.RewardResource, q.RewardAmount);
                    if (SystemAPI.HasSingleton<SettlerSimulationState>())
                    {
                        ref var settlers = ref SystemAPI.GetSingletonRW<SettlerSimulationState>().ValueRW;
                        settlers.ColonyMorale01 = math.saturate(settlers.ColonyMorale01 + 0.02f + q.Difficulty * 0.01f);
                    }
                }
                else
                {
                    var stage = (byte)math.min(4f, math.floor(q.Progress01 * 4f));
                    q.Stage = stage;
                }

                quests[i] = q;
            }
        }

        private static void UpdatePersonalStories(ref SystemState state, uint day, ref StorySimulationState sim,
            ref DynamicBuffer<PersonalStoryArcEntry> arcs)
        {
            if (!SystemAPI.HasSingleton<SettlerSimulationState>())
                return;

            var settlers = SystemAPI.GetSingleton<SettlerSimulationState>();
            if (settlers.PopulationAlive == 0 || settlers.LastSettlerId == 0)
                return;

            var rng = Random.CreateFromIndex(day * 3266489917u + 0x91E10DA5u);
            var arcCap = 24;
            if (arcs.Length < arcCap && rng.NextFloat() < 0.45f)
            {
                var settlerId = math.max(1u, settlers.LastSettlerId - (uint)math.floor(rng.NextFloat() * 16f));
                if (!HasArc(ref arcs, settlerId))
                {
                    var archetype = EventsQuestSimulationMath.ResolveArchetype(
                        settlers.ColonyMorale01,
                        math.saturate(settlers.AverageStress / 100f),
                        settlers.AverageHealth01,
                        rng.NextFloat());
                    arcs.Add(new PersonalStoryArcEntry
                    {
                        SettlerId = settlerId,
                        Archetype = archetype,
                        BeatsCompleted = 0,
                        LastBeatDay = day,
                        Impact01 = 0.2f + rng.NextFloat() * 0.4f,
                        Nickname = new FixedString64Bytes("arc-" + settlerId)
                    });
                }
            }

            var beatsToday = 0u;
            var battleFactor = SystemAPI.HasSingleton<MilitarySimulationState>()
                ? math.saturate(SystemAPI.GetSingleton<MilitarySimulationState>().AverageSuppression01)
                : 0f;
            for (var i = 0; i < arcs.Length; i++)
            {
                var arc = arcs[i];
                var chance = 0.10f + battleFactor * 0.35f + arc.Impact01 * 0.15f;
                if (rng.NextFloat() < chance)
                {
                    arc.BeatsCompleted++;
                    arc.LastBeatDay = day;
                    arc.Impact01 = math.saturate(arc.Impact01 + 0.05f);
                    beatsToday++;
                }

                if (day - arc.LastBeatDay > 40u && arc.Impact01 < 0.35f)
                {
                    arcs.RemoveAt(i);
                    i--;
                    continue;
                }

                arcs[i] = arc;
            }

            sim.StoryArcBeatsTotal += beatsToday;
        }

        private static bool HasArc(ref DynamicBuffer<PersonalStoryArcEntry> arcs, uint settlerId)
        {
            for (var i = 0; i < arcs.Length; i++)
            {
                if (arcs[i].SettlerId == settlerId)
                    return true;
            }
            return false;
        }

        private static uint CountQuestsByStatus(ref DynamicBuffer<QuestRecordEntry> quests, QuestStatus status)
        {
            uint count = 0;
            for (var i = 0; i < quests.Length; i++)
            {
                if (quests[i].Status == status)
                    count++;
            }
            return count;
        }

        private static float ComputeAverageQuestProgress(ref DynamicBuffer<QuestRecordEntry> quests)
        {
            var sum = 0f;
            var count = 0u;
            for (var i = 0; i < quests.Length; i++)
            {
                if (quests[i].Status != QuestStatus.Active)
                    continue;
                sum += quests[i].Progress01;
                count++;
            }
            return count == 0u ? 0f : sum / count;
        }
    }
}
