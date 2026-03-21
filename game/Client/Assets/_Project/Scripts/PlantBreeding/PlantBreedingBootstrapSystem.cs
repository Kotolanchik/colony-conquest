using Unity.Collections;
using Unity.Entities;

namespace ColonyConquest.PlantBreeding
{
    /// <summary>Инициализация демо-лаборатории селекции, каталога сортов и стартовой заявки на скрещивание.</summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct PlantBreedingBootstrapSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            if (SystemAPI.HasSingleton<PlantBreedingLabSingleton>())
                return;

            var lab = state.EntityManager.CreateEntity();
            state.EntityManager.AddComponent<PlantBreedingLabSingleton>(lab);
            state.EntityManager.AddComponentData(lab, new PlantBreedingLabState
            {
                LastProcessedDay = uint.MaxValue,
                NextCultivarId = 3u,
                LabTier = 2,
                IsolationLevel01 = 0.60f,
                DemoPlantationAreaFactor = 1f
            });

            var cultivars = state.EntityManager.AddBuffer<PlantCultivarEntry>(lab);
            cultivars.Add(new PlantCultivarEntry
            {
                CultivarId = 1u,
                DebugName = new FixedString32Bytes("Wheat_Base"),
                Traits = new PlantGenomeTraits
                {
                    Yield = 100f,
                    GrowthSpeed = 100f,
                    DroughtResistance = 40f,
                    ColdResistance = 45f,
                    PestResistance = 55f,
                    NutritionalValue = 95f,
                    Taste = 90f
                },
                StabilityScore = 70f,
                MutationLoad = 0f,
                Generation = 1,
                IsGmo = 0,
                BioSafetyTier = 0,
                EditDepth = 0f
            });
            cultivars.Add(new PlantCultivarEntry
            {
                CultivarId = 2u,
                DebugName = new FixedString32Bytes("Rye_Base"),
                Traits = new PlantGenomeTraits
                {
                    Yield = 86f,
                    GrowthSpeed = 92f,
                    DroughtResistance = 55f,
                    ColdResistance = 62f,
                    PestResistance = 58f,
                    NutritionalValue = 90f,
                    Taste = 84f
                },
                StabilityScore = 72f,
                MutationLoad = 0f,
                Generation = 1,
                IsGmo = 0,
                BioSafetyTier = 0,
                EditDepth = 0f
            });

            var workOrders = state.EntityManager.AddBuffer<PlantBreedingWorkOrderEntry>(lab);
            workOrders.Add(new PlantBreedingWorkOrderEntry
            {
                ParentAId = 1u,
                ParentBId = 2u,
                ParentWeightA = 1.2f,
                ParentWeightB = 0.8f,
                RemainingDays = 3,
                IsGmo = 0,
                BioSafetyTier = 0,
                EditDepth = 0f
            });
        }
    }
}
