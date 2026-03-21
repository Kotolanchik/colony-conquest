using ColonyConquest.Agriculture;
using ColonyConquest.Simulation;
using Unity.Entities;
using Unity.Mathematics;

namespace ColonyConquest.Core
{
    /// <summary>
    /// Восстановление леса: +1% к запасу в год до исходного максимума — <c>spec/agriculture_mining_spec.md</c> §2.3.
    /// </summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(ColonyCalendarAdvanceSystem))]
    public partial struct MiningForestRegenerationSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<MiningWorldRegenerationState>();
            state.RequireForUpdate<GameCalendarState>();
            state.RequireForUpdate<WildRenewableStockState>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var day = SystemAPI.GetSingleton<GameCalendarState>().DayIndex;
            var gameYear = day / MiningRegenerationTuning.DaysPerGameYear;
            ref var reg = ref SystemAPI.GetSingletonRW<MiningWorldRegenerationState>().ValueRW;
            if (gameYear == 0)
                return;
            if (gameYear <= reg.LastProcessedGameYear)
                return;
            reg.LastProcessedGameYear = gameYear;

            foreach (var depositRw in SystemAPI.Query<RefRW<MiningDepositRuntime>>())
            {
                ref var d = ref depositRw.ValueRW;
                if (d.Kind != MiningDepositKindId.Forest)
                    continue;
                var cap = d.InitialAmount > 0f ? d.InitialAmount : d.AmountRemaining;
                if (cap <= 0f)
                    continue;
                var add = cap * MiningRegenerationTuning.ForestRegrowthFractionOfCapPerYear;
                d.AmountRemaining = math.min(cap, d.AmountRemaining + add);
            }

            ref var wild = ref SystemAPI.GetSingletonRW<WildRenewableStockState>().ValueRW;
            if (wild.FishBiomassCap > 0f)
                wild.FishBiomass = math.min(
                    wild.FishBiomassCap,
                    wild.FishBiomass + wild.FishBiomassCap * MiningRegenerationTuning.FishRegrowthFractionOfCapPerYear);
            if (wild.WildGameCap > 0f)
                wild.WildGameBiomass = math.min(
                    wild.WildGameCap,
                    wild.WildGameBiomass + wild.WildGameCap * MiningRegenerationTuning.WildGameRegrowthFractionOfCapPerYear);
        }
    }
}
