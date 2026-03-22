using ColonyConquest.Analytics;
using ColonyConquest.Core;
using ColonyConquest.Simulation;
using ColonyConquest.Story;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace ColonyConquest.Diplomacy
{
    /// <summary>Суточный апдейт дипломатии: отношения, торговля, союзы и решение ИИ о войне.</summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(ColonyCalendarAdvanceSystem))]
    public partial struct DiplomacyDailySystem : ISystem
    {
        private const uint EventTradeCompleted = 0xC101;
        private const uint EventWarDeclared = 0xC102;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameCalendarState>();
            state.RequireForUpdate<DiplomacySimulationSingleton>();
            state.RequireForUpdate<DiplomacySimulationState>();
            state.RequireForUpdate<SimulationRootState>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var day = SystemAPI.GetSingleton<GameCalendarState>().DayIndex;
            ref var sim = ref SystemAPI.GetSingletonRW<DiplomacySimulationState>().ValueRW;
            if (sim.LastProcessedDay == day)
                return;
            sim.LastProcessedDay = day;

            var tick = SystemAPI.GetSingleton<SimulationRootState>().SimulationTick;
            var rng = Random.CreateFromIndex(math.hash(new uint2(day, tick)));

            var relations = SystemAPI.GetSingletonBuffer<DiplomaticRelationEntry>();
            var factions = SystemAPI.GetSingletonBuffer<FactionProfileEntry>();
            var deals = SystemAPI.GetSingletonBuffer<TradeDealEntry>();
            var alliances = SystemAPI.GetSingletonBuffer<DiplomaticAllianceEntry>();
            var wars = SystemAPI.GetSingletonBuffer<ActiveWarEntry>();

            UpdateRelations(ref relations, ref factions);
            var tradeProfit = ProcessTradeDeals(ref deals, ref relations, ref state, tick);
            UpdateAlliances(ref alliances, ref relations);
            TryDeclareWar(ref factions, ref relations, ref wars, ref state, tick, day, ref rng, ref sim);
            UpdateWars(ref wars);

            sim.DailyTradeProfit = tradeProfit;
            sim.ActiveDeals = (ushort)deals.Length;
            sim.ActiveAlliances = (ushort)alliances.Length;
            sim.AverageRelations = ComputeAverageRelations(ref relations);

            AnalyticsHooks.Record(AnalyticsDomain.GlobalWorld, AnalyticsMetricIds.DiplomacyAverageRelations,
                sim.AverageRelations);
            AnalyticsHooks.Record(AnalyticsDomain.GlobalWorld, AnalyticsMetricIds.DiplomacyTradeProfitDaily, sim.DailyTradeProfit);
            AnalyticsHooks.Record(AnalyticsDomain.GlobalWorld, AnalyticsMetricIds.DiplomacyActiveAlliances, sim.ActiveAlliances);
            AnalyticsHooks.Record(AnalyticsDomain.GlobalWorld, AnalyticsMetricIds.DiplomacyWarsDeclaredTotal,
                sim.WarsDeclaredTotal);
        }

        private static void UpdateRelations(ref DynamicBuffer<DiplomaticRelationEntry> relations,
            ref DynamicBuffer<FactionProfileEntry> factions)
        {
            for (var i = 0; i < relations.Length; i++)
            {
                var entry = relations[i];
                if (!TryGetFaction(ref factions, entry.FactionA, out var fa) ||
                    !TryGetFaction(ref factions, entry.FactionB, out var fb))
                    continue;

                var ideologyAffinity = DiplomacyMath.AreIdeologiesCompatible(fa.Ideology, fb.Ideology) ? 1f : -1f;
                var delta = DiplomacyMath.ComputeRelationsDelta(
                    tradeDealsCount: entry.RelationScore >= 25f ? 1f : 0f,
                    commonEnemyFactor: entry.CommonEnemyBonus,
                    ideologyAffinity: ideologyAffinity,
                    borderTension: entry.BorderTension);

                entry.RelationScore = math.clamp(entry.RelationScore + delta, -100f, 100f);
                relations[i] = entry;
            }
        }

        private static float ProcessTradeDeals(ref DynamicBuffer<TradeDealEntry> deals,
            ref DynamicBuffer<DiplomaticRelationEntry> relations, ref SystemState state, uint tick)
        {
            var totalProfit = 0f;
            for (var i = deals.Length - 1; i >= 0; i--)
            {
                var deal = deals[i];
                var relation = GetRelationScore(ref relations, deal.SellerFaction, deal.BuyerFaction);
                var unitPrice = DiplomacyMath.ComputeTradePrice(
                    deal.BasePrice,
                    deal.DemandSupplyFactor,
                    deal.DistanceKm,
                    relation,
                    deal.TraderSkillLevel);
                var commission = deal.DealKind switch
                {
                    TradeDealKindId.Instant => 0.10f,
                    TradeDealKindId.Contract => 0.05f,
                    TradeDealKindId.LongTerm => 0.02f,
                    TradeDealKindId.Barter => 0f,
                    _ => 0.10f
                };
                var gross = unitPrice * math.max(0f, deal.VolumePerDay);
                totalProfit += gross * (1f - commission);

                deal.RemainingDays--;
                if (deal.RemainingDays <= 0)
                {
                    deals.RemoveAt(i);
                    TryEnqueueStoryEvent(ref state, tick, EventTradeCompleted, new FixedString64Bytes("trade-complete"));
                    continue;
                }

                deals[i] = deal;
            }

            return totalProfit;
        }

        private static void UpdateAlliances(ref DynamicBuffer<DiplomaticAllianceEntry> alliances,
            ref DynamicBuffer<DiplomaticRelationEntry> relations)
        {
            for (var i = alliances.Length - 1; i >= 0; i--)
            {
                var alliance = alliances[i];
                var relation = GetRelationScore(ref relations, alliance.FactionA, alliance.FactionB);
                if (relation < alliance.RequiredRelation - 20f)
                    alliances.RemoveAt(i);
            }
        }

        private static void TryDeclareWar(ref DynamicBuffer<FactionProfileEntry> factions,
            ref DynamicBuffer<DiplomaticRelationEntry> relations, ref DynamicBuffer<ActiveWarEntry> wars, ref SystemState state,
            uint tick, uint day, ref Random rng, ref DiplomacySimulationState sim)
        {
            if (day % 30u != 0)
                return;

            for (var i = 0; i < relations.Length; i++)
            {
                var relation = relations[i];
                if (!TryGetFaction(ref factions, relation.FactionA, out var a) ||
                    !TryGetFaction(ref factions, relation.FactionB, out var b))
                    continue;

                var powerRatio = a.MilitaryPower / math.max(1f, b.MilitaryPower);
                var warChance = DiplomacyMath.ComputeAiWarChancePerYear(0.05f, powerRatio, relation.RelationScore,
                    a.Personality);
                var monthlyChance = math.saturate(warChance / 12f);
                if (rng.NextFloat() > monthlyChance || relation.RelationScore > -50f)
                    continue;
                if (HasActiveWar(ref wars, a.FactionId, b.FactionId))
                    continue;

                wars.Add(new ActiveWarEntry
                {
                    AttackerFaction = a.FactionId,
                    DefenderFaction = b.FactionId,
                    WarGoal = 0,
                    RemainingDays = 120
                });
                sim.WarsDeclaredTotal++;
                relation.RelationScore = math.clamp(relation.RelationScore - 20f, -100f, 100f);
                relations[i] = relation;
                TryEnqueueStoryEvent(ref state, tick, EventWarDeclared, new FixedString64Bytes("war-declared"));
            }
        }

        private static void UpdateWars(ref DynamicBuffer<ActiveWarEntry> wars)
        {
            for (var i = wars.Length - 1; i >= 0; i--)
            {
                var war = wars[i];
                if (war.RemainingDays > 0)
                    war.RemainingDays--;
                if (war.RemainingDays == 0)
                {
                    wars.RemoveAt(i);
                    continue;
                }

                wars[i] = war;
            }
        }

        private static bool TryGetFaction(ref DynamicBuffer<FactionProfileEntry> factions, uint factionId,
            out FactionProfileEntry profile)
        {
            for (var i = 0; i < factions.Length; i++)
            {
                if (factions[i].FactionId != factionId)
                    continue;
                profile = factions[i];
                return true;
            }

            profile = default;
            return false;
        }

        private static float GetRelationScore(ref DynamicBuffer<DiplomaticRelationEntry> relations, uint a, uint b)
        {
            for (var i = 0; i < relations.Length; i++)
            {
                var r = relations[i];
                if ((r.FactionA == a && r.FactionB == b) || (r.FactionA == b && r.FactionB == a))
                    return r.RelationScore;
            }

            return 0f;
        }

        private static bool HasActiveWar(ref DynamicBuffer<ActiveWarEntry> wars, uint a, uint b)
        {
            for (var i = 0; i < wars.Length; i++)
            {
                var w = wars[i];
                if ((w.AttackerFaction == a && w.DefenderFaction == b) || (w.AttackerFaction == b && w.DefenderFaction == a))
                    return true;
            }

            return false;
        }

        private static float ComputeAverageRelations(ref DynamicBuffer<DiplomaticRelationEntry> relations)
        {
            if (relations.Length == 0)
                return 0f;
            var sum = 0f;
            for (var i = 0; i < relations.Length; i++)
                sum += relations[i].RelationScore;
            return sum / relations.Length;
        }

        private static void TryEnqueueStoryEvent(ref SystemState state, uint tick, uint eventDefinitionId,
            in FixedString64Bytes label)
        {
            if (!SystemAPI.HasSingleton<StoryEventQueueSingleton>())
                return;
            var queue = SystemAPI.GetSingletonBuffer<GameEventQueueEntry>(ref state);
            queue.Add(new GameEventQueueEntry
            {
                Kind = StoryEventKind.Triggered,
                EventDefinitionId = eventDefinitionId,
                EnqueueSimulationTick = tick,
                DebugLabel = label
            });
        }
    }
}
