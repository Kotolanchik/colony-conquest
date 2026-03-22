using ColonyConquest.Simulation;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace ColonyConquest.Housing
{
    /// <summary>Раз в игровой день распределяет очередь домохозяйств по лучшим доступным жилым блокам.</summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(ColonyCalendarAdvanceSystem))]
    public partial struct HousingAssignmentSystem : ISystem
    {
        private EntityQuery _housingQuery;

        public void OnCreate(ref SystemState state)
        {
            _housingQuery = state.GetEntityQuery(ComponentType.ReadWrite<HousingUnitRuntime>(),
                ComponentType.ReadOnly<HousingComfortSnapshot>());
            state.RequireForUpdate<GameCalendarState>();
            state.RequireForUpdate<HousingAssignmentQueueSingleton>();
            state.RequireForUpdate<HousingAssignmentProcessState>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var day = SystemAPI.GetSingleton<GameCalendarState>().DayIndex;
            ref var proc = ref SystemAPI.GetSingletonRW<HousingAssignmentProcessState>().ValueRW;
            if (proc.LastProcessedDay == day)
                return;
            proc.LastProcessedDay = day;

            var queue = SystemAPI.GetSingletonBuffer<HousingAssignmentRequestEntry>(ref state);
            if (queue.Length <= 0)
            {
                if (SystemAPI.HasSingleton<HousingColonyState>())
                {
                    ref var colony = ref SystemAPI.GetSingletonRW<HousingColonyState>().ValueRW;
                    colony.AssignmentBacklog = 0;
                }

                return;
            }

            var pending = new NativeList<HousingAssignmentRequestEntry>(Allocator.Temp);
            var entities = _housingQuery.ToEntityArray(Allocator.Temp);
            var em = state.EntityManager;

            for (var i = 0; i < queue.Length; i++)
            {
                var request = queue[i];
                var bestEntity = Entity.Null;
                var bestScore = float.MinValue;

                for (var j = 0; j < entities.Length; j++)
                {
                    var e = entities[j];
                    var unit = em.GetComponentData<HousingUnitRuntime>(e);
                    var snapshot = em.GetComponentData<HousingComfortSnapshot>(e);
                    var score = HousingMath.ComputeHousingScore(unit, snapshot, request);
                    if (score <= bestScore)
                        continue;
                    bestScore = score;
                    bestEntity = e;
                }

                if (bestEntity == Entity.Null)
                {
                    pending.Add(request);
                    continue;
                }

                var selected = em.GetComponentData<HousingUnitRuntime>(bestEntity);
                var hardCap = HousingMath.GetHardCapacity(selected);
                var free = hardCap - selected.Residents;
                if (free <= 0)
                {
                    pending.Add(request);
                    continue;
                }

                var assign = math.min(free, request.HouseholdSize);
                selected.Residents += (short)assign;
                em.SetComponentData(bestEntity, selected);

                if (assign < request.HouseholdSize)
                {
                    request.HouseholdSize -= (short)assign;
                    pending.Add(request);
                }
            }

            queue.Clear();
            for (var i = 0; i < pending.Length; i++)
                queue.Add(pending[i]);

            pending.Dispose();
            entities.Dispose();

            if (SystemAPI.HasSingleton<HousingColonyState>())
            {
                ref var colony = ref SystemAPI.GetSingletonRW<HousingColonyState>().ValueRW;
                colony.AssignmentBacklog = queue.Length;
            }
        }
    }
}
