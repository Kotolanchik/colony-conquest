using ColonyConquest.Analytics;
using ColonyConquest.Ecology;
using ColonyConquest.Simulation;
using ColonyConquest.Story;
using Unity.Entities;
using Unity.Mathematics;

namespace ColonyConquest.Core
{
    /// <summary>
    /// Заполняет <see cref="AiDirectorDimensionsState"/> по <c>spec/events_quests_spec.md</c> §2.2 из аналитики и экологии.
    /// </summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(AnalyticsSnapshotUpdateSystem))]
    public partial struct AiDirectorDimensionsUpdateSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<AiDirectorDimensionsState>();
            state.RequireForUpdate<AnalyticsLocalSnapshot>();
            state.RequireForUpdate<ColonyDemographyState>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var snap = SystemAPI.GetSingleton<AnalyticsLocalSnapshot>();
            var pop = math.max(1f, SystemAPI.GetSingleton<ColonyDemographyState>().Population);
            ref var ai = ref SystemAPI.GetSingletonRW<AiDirectorDimensionsState>().ValueRW;

            float wealth = math.saturate(snap.Economy.GdpPerCapita / 2000f) * 45f +
                           math.saturate(math.log10(snap.Economy.Gdp + 10f) / 6f) * 55f;
            ai.Wealth0to100 = math.clamp(wealth, 0f, 100f);

            float army = snap.Military.ActiveArmy;
            float security01 = math.saturate(army / math.max(1f, pop * 0.02f));
            ai.Security0to100 = security01 * 100f;

            float stability = (snap.Social.Happiness01 + snap.Social.Health01 + snap.Social.Ecology01) / 3f * 100f;
            ai.Stability0to100 = math.clamp(stability, 0f, 100f);

            float progress = snap.Technology.CurrentEraProgress01 * 70f + math.min(30f, snap.Technology.TechnologiesUnlocked * 2f);
            ai.Progress0to100 = math.clamp(progress, 0f, 100f);

            float pol = 0f;
            if (SystemAPI.HasSingleton<ColonyPollutionSummaryState>())
                pol = SystemAPI.GetSingleton<ColonyPollutionSummaryState>().CombinedPollutionPercent0to100;

            float drama = math.abs(ai.Stability0to100 - 50f);
            float tension = math.clamp(
                0.35f * drama + 0.45f * pol + 0.2f * math.max(0f, 70f - ai.Wealth0to100),
                0f,
                100f);
            ai.Tension0to100 = tension;
        }
    }
}
