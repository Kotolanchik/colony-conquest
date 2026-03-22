using ColonyConquest.Economy;
using Unity.Entities;
using Unity.Mathematics;

namespace ColonyConquest.Core
{
    /// <summary>
    /// Демо производства по §2.1–2.3 <c>spec/economic_system_specification.md</c>: один виртуальный цех,
    /// очередь рецептов эпохи 1, списание/зачисление на <see cref="ResourceStockpileSingleton"/>.
    /// </summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(GameBootstrapSystem))]
    public partial struct EconomyWorkshopProductionSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EconomyWorkshopRuntime>();
            state.RequireForUpdate<ResourceStockpileSingleton>();
        }

        public void OnUpdate(ref SystemState state)
        {
            // При наличии полного экономического контура демо-цех отключается, чтобы не дублировать производство.
            if (SystemAPI.HasSingleton<EconomySimulationSingleton>())
                return;

            var dt = SystemAPI.Time.DeltaTime;
            if (dt <= 0f)
                return;

            ref var ws = ref SystemAPI.GetSingletonRW<EconomyWorkshopRuntime>().ValueRW;
            var stockBuf = SystemAPI.GetSingletonBuffer<ResourceStockEntry>();

            var pick = SelectAffordableRecipeByPriority(ref stockBuf);
            if (pick == ProductionRecipeId.None)
            {
                ws.ActiveRecipe = ProductionRecipeId.None;
                ws.Progress01 = 0f;
                return;
            }

            if (ws.ActiveRecipe != pick)
            {
                ws.ActiveRecipe = pick;
                ws.Progress01 = 0f;
            }

            var def = ProductionRecipeCatalog.Get(ws.ActiveRecipe);
            if (def.Id == ProductionRecipeId.None)
                return;

            if (!CanRun(ref stockBuf, in def))
            {
                ws.Progress01 = 0f;
                return;
            }

            var workerCount = math.max((float)ws.AssignedWorkers, 1f);
            var eff = ProductionEfficiencyMath.ComputeSpeedMultiplier(
                workerCount,
                def.OptimalWorkers,
                ws.EnergyRatio01,
                ws.ToolCondition01,
                ws.AverageSkill0To100,
                ws.BuildingWear01);

            ws.Progress01 += eff * (dt / def.DurationSeconds);
            if (ws.Progress01 < 1f)
                return;

            ws.Progress01 = 0f;
            if (!ResourceStockpileOps.TryConsumeRecipe(ref stockBuf, in def))
                return;

            ResourceStockpileOps.Add(ref stockBuf, def.Output, def.OutputAmount);
        }

        private static ProductionRecipeId SelectAffordableRecipeByPriority(ref DynamicBuffer<ResourceStockEntry> buffer)
        {
            var order = ProductionRecipeCatalog.SelectionPriorityOrder;
            for (var i = 0; i < order.Length; i++)
            {
                var id = order[i];
                var def = ProductionRecipeCatalog.Get(id);
                if (CanRun(ref buffer, in def))
                    return id;
            }

            return ProductionRecipeId.None;
        }

        private static bool CanRun(ref DynamicBuffer<ResourceStockEntry> buffer, in ProductionRecipeDefinition def)
        {
            if (def.Id == ProductionRecipeId.None)
                return false;
            if (!ResourceStockpileOps.HasAtLeast(ref buffer, def.In0, def.Amount0))
                return false;
            if (def.In1 != ResourceId.None && def.Amount1 > 0f && !ResourceStockpileOps.HasAtLeast(ref buffer, def.In1, def.Amount1))
                return false;
            if (def.In2 != ResourceId.None && def.Amount2 > 0f && !ResourceStockpileOps.HasAtLeast(ref buffer, def.In2, def.Amount2))
                return false;
            return true;
        }
    }
}
