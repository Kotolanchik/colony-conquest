using ColonyConquest.Agriculture;
using ColonyConquest.Economy;
using Unity.Entities;
using Unity.Mathematics;

namespace ColonyConquest.Core
{
    /// <summary>Промышленная добыча §2.2: ед/час на склад (демо без привязки к месторождению).</summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(ColonyCalendarAdvanceSystem))]
    public partial struct IndustrialMiningProductionSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ResourceStockpileSingleton>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var dt = SystemAPI.Time.DeltaTime;
            var stockBuf = SystemAPI.GetSingletonBuffer<ResourceStockEntry>();

            foreach (var siteRw in SystemAPI.Query<RefRW<IndustrialMiningSiteRuntime>>().WithAll<IndustrialMiningSiteTag>())
            {
                ref var site = ref siteRw.ValueRW;
                if (site.WorkersAssigned == 0 || site.OutputResourceId == ResourceId.None)
                    continue;

                var perHour = IndustrialMiningFormulas.GetNominalOutputPerGameHour(site.Method);
                if (perHour <= 0f)
                    continue;

                var workerScale = math.saturate(site.WorkersAssigned / 5f);
                var perSec = perHour / 3600f * workerScale;
                site.OutputAccumulator += perSec * dt;
                while (site.OutputAccumulator >= 1f)
                {
                    ResourceStockpileOps.Add(ref stockBuf, site.OutputResourceId, 1f);
                    site.OutputAccumulator -= 1f;
                }
            }
        }
    }
}
