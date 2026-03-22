using Unity.Collections;
using Unity.Entities;

namespace ColonyConquest.Defense
{
    /// <summary>Создаёт singleton и демо-данные для оборонительной симуляции.</summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct DefensiveBootstrapSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            if (SystemAPI.HasSingleton<DefensiveSimulationSingleton>())
                return;

            var em = state.EntityManager;
            var entity = em.CreateEntity();
            em.AddComponent<DefensiveSimulationSingleton>(entity);
            em.AddComponent(entity, new DefensiveSimulationState
            {
                LastProcessedDay = uint.MaxValue,
                LastStructureId = 1,
                StructuresBuiltTotal = 1,
                StructuresDestroyedTotal = 0,
                EngineersAssigned = 6,
                EngineerSkillLevel = 2f,
                UnderFireIntensity = 1,
                IncomingDamagePressure = 120f,
                PowerReserveKw = 200f
            });

            var orders = em.AddBuffer<DefensiveConstructionOrderEntry>(entity);
            orders.Add(BuildOrder(1, DefensiveStructureKindId.Trenches, 4, 0f, new FixedString64Bytes("front-trench")));
            orders.Add(BuildOrder(2, DefensiveStructureKindId.AutomatedTurret, 2, 0.2f,
                new FixedString64Bytes("autoturret-mk1")));

            var runtime = em.AddBuffer<DefensiveStructureRuntimeEntry>(entity);
            runtime.Add(BuildRuntime(1, DefensiveStructureKindId.SandbagWall));
        }

        private static DefensiveConstructionOrderEntry BuildOrder(
            uint orderId,
            DefensiveStructureKindId kind,
            ushort engineers,
            float progress01,
            in FixedString64Bytes debugName)
        {
            DefensiveSimulationMath.TryGetBaseBuildHours(kind, out var hours);
            return new DefensiveConstructionOrderEntry
            {
                OrderId = orderId,
                Kind = kind,
                BaseBuildHours = hours,
                EngineersAssigned = engineers,
                UnderFireIntensity = 0,
                Progress01 = progress01,
                IsCompleted = 0,
                DebugName = debugName
            };
        }

        private static DefensiveStructureRuntimeEntry BuildRuntime(uint id, DefensiveStructureKindId kind)
        {
            var hp = DefensiveSimulationMath.GetMaxHp(kind);
            return new DefensiveStructureRuntimeEntry
            {
                StructureId = id,
                Kind = kind,
                CurrentHp = hp,
                MaxHp = hp,
                DefenseBonusPercent = DefensiveSimulationMath.GetDefenseBonusPercent(kind),
                SlowEffectPercent = DefensiveSimulationMath.GetSlowEffectPercent(kind),
                ContactDamage = DefensiveSimulationMath.GetContactDamage(kind),
                EnergyDemandKw = DefensiveSimulationMath.GetEnergyDemandKw(kind),
                IsOperational = 1,
                IsHighTech = (byte)(DefensiveSimulationMath.IsHighTech(kind) ? 1 : 0)
            };
        }
    }
}
