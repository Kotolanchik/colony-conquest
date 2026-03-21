using ColonyConquest.Analytics;
using ColonyConquest.Core;
using ColonyConquest.Simulation;
using ColonyConquest.Story;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace ColonyConquest.PlantBreeding
{
    /// <summary>Суточный цикл селекции: исполнение заявок, регистрация новых сортов, риск-ивенты ГМО и метрики.</summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(ColonyCalendarAdvanceSystem))]
    public partial struct PlantBreedingDailySimulationSystem : ISystem
    {
        private const uint GmoRiskEventDefinitionId = 0xB501;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameCalendarState>();
            state.RequireForUpdate<PlantBreedingLabSingleton>();
            state.RequireForUpdate<PlantBreedingLabState>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var day = SystemAPI.GetSingleton<GameCalendarState>().DayIndex;
            ref var lab = ref SystemAPI.GetSingletonRW<PlantBreedingLabState>().ValueRW;
            if (lab.LastProcessedDay == day)
                return;
            lab.LastProcessedDay = day;

            var cultivars = SystemAPI.GetSingletonBuffer<PlantCultivarEntry>();
            var orders = SystemAPI.GetSingletonBuffer<PlantBreedingWorkOrderEntry>();
            if (orders.Length <= 0)
            {
                EnqueueAutoWorkOrder(ref cultivars, ref orders);
                return;
            }

            var tick = SystemAPI.GetSingleton<SimulationRootState>().SimulationTick;
            var rng = Random.CreateFromIndex(math.hash(new uint3(day, tick, lab.NextCultivarId)));
            var completedOrders = 0;
            var successes = 0;
            var gmoIncidents = 0;
            var positiveMutations = 0;
            var totalMutations = 0;

            for (var i = orders.Length - 1; i >= 0; i--)
            {
                var order = orders[i];
                order.RemainingDays--;
                if (order.RemainingDays > 0)
                {
                    orders[i] = order;
                    continue;
                }
                completedOrders++;

                if (!TryFindCultivar(ref cultivars, order.ParentAId, out var parentA) ||
                    !TryFindCultivar(ref cultivars, order.ParentBId, out var parentB))
                {
                    orders.RemoveAt(i);
                    continue;
                }

                var mutationKind = PlantBreedingMath.RollMutationKind(lab.LabTier, ref rng);
                if (mutationKind == PlantBreedingMath.MutationKind.Lethal)
                {
                    orders.RemoveAt(i);
                    if (order.IsGmo != 0)
                        gmoIncidents++;
                    continue;
                }

                totalMutations++;
                if (mutationKind == PlantBreedingMath.MutationKind.Positive ||
                    mutationKind == PlantBreedingMath.MutationKind.Major)
                {
                    positiveMutations++;
                }

                var childTraits = PlantBreedingMath.BuildOffspring(
                    parentA.Traits,
                    parentB.Traits,
                    math.max(0.1f, order.ParentWeightA),
                    math.max(0.1f, order.ParentWeightB),
                    hasHeterosisBonus: math.abs((int)parentA.Generation - (int)parentB.Generation) >= 3,
                    mutationKind,
                    ref rng,
                    out var mutationLoad);

                var generation = (byte)math.min(255, math.max(parentA.Generation, parentB.Generation) + 1);
                var stability = PlantBreedingMath.ComputeStabilityScore(parentA.StabilityScore, parentB.StabilityScore,
                    mutationLoad, generation, order.IsGmo != 0, order.EditDepth);

                var childId = lab.NextCultivarId++;
                cultivars.Add(new PlantCultivarEntry
                {
                    CultivarId = childId,
                    DebugName = BuildCultivarName(),
                    Traits = childTraits,
                    StabilityScore = stability,
                    MutationLoad = mutationLoad,
                    Generation = generation,
                    IsGmo = order.IsGmo,
                    BioSafetyTier = order.BioSafetyTier,
                    EditDepth = order.EditDepth
                });
                successes++;

                if (order.IsGmo != 0)
                {
                    var risk = PlantBreedingMath.ComputeEcologyRisk(order.EditDepth, lab.DemoPlantationAreaFactor,
                        order.BioSafetyTier, lab.IsolationLevel01);
                    if (risk >= 65f)
                    {
                        gmoIncidents++;
                        TryEnqueueStoryEvent(ref state, tick, GmoRiskEventDefinitionId,
                            new FixedString64Bytes("gmo-risk"));
                    }
                }

                orders.RemoveAt(i);
            }

            AnalyticsHooks.RecordCounter(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.PlantBreedingCyclesTotal,
                completedOrders);
            if (successes > 0)
            {
                AnalyticsHooks.RecordCounter(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.PlantBreedingSuccessTotal,
                    successes);
            }
            if (gmoIncidents > 0)
            {
                AnalyticsHooks.RecordCounter(AnalyticsDomain.LocalSettlement,
                    AnalyticsMetricIds.PlantBreedingGmoIncidentTotal, gmoIncidents);
            }

            if (cultivars.Length > 0)
            {
                var stabilityAvg = 0f;
                for (var i = 0; i < cultivars.Length; i++)
                    stabilityAvg += cultivars[i].StabilityScore;
                stabilityAvg /= cultivars.Length;
                AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement, AnalyticsMetricIds.PlantBreedingLineStabilityAvg,
                    stabilityAvg);
            }

            if (totalMutations > 0)
            {
                var positiveShare = positiveMutations / (float)totalMutations;
                AnalyticsHooks.Record(AnalyticsDomain.LocalSettlement,
                    AnalyticsMetricIds.PlantBreedingMutationPositiveShare, positiveShare);
            }

            if (orders.Length == 0)
                EnqueueAutoWorkOrder(ref cultivars, ref orders);
        }

        private static void EnqueueAutoWorkOrder(ref DynamicBuffer<PlantCultivarEntry> cultivars,
            ref DynamicBuffer<PlantBreedingWorkOrderEntry> orders)
        {
            if (cultivars.Length < 2 || orders.Length > 0)
                return;

            var bestA = cultivars[0];
            var bestB = cultivars[1];
            if (bestB.StabilityScore > bestA.StabilityScore)
            {
                var t = bestA;
                bestA = bestB;
                bestB = t;
            }

            for (var i = 2; i < cultivars.Length; i++)
            {
                var c = cultivars[i];
                if (c.StabilityScore > bestA.StabilityScore)
                {
                    bestB = bestA;
                    bestA = c;
                }
                else if (c.StabilityScore > bestB.StabilityScore)
                {
                    bestB = c;
                }
            }

            orders.Add(new PlantBreedingWorkOrderEntry
            {
                ParentAId = bestA.CultivarId,
                ParentBId = bestB.CultivarId,
                ParentWeightA = 1.2f,
                ParentWeightB = 0.8f,
                RemainingDays = 3,
                IsGmo = 0,
                BioSafetyTier = 0,
                EditDepth = 0f
            });
        }

        private static FixedString32Bytes BuildCultivarName()
        {
            return new FixedString32Bytes("Cultivar");
        }

        private static bool TryFindCultivar(ref DynamicBuffer<PlantCultivarEntry> cultivars, uint id,
            out PlantCultivarEntry cultivar)
        {
            for (var i = 0; i < cultivars.Length; i++)
            {
                if (cultivars[i].CultivarId != id)
                    continue;
                cultivar = cultivars[i];
                return true;
            }

            cultivar = default;
            return false;
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
