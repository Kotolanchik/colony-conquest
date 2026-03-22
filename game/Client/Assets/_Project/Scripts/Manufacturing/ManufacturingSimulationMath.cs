using ColonyConquest.Economy;
using ColonyConquest.Technology;
using Unity.Burst;
using Unity.Mathematics;

namespace ColonyConquest.Manufacturing
{
    /// <summary>Формулы и таблицы производственных заводов по spec/manufacturing_plants_spec.md.</summary>
    [BurstCompile]
    public static class ManufacturingSimulationMath
    {
        public const byte RetoolingDays = 7;

        public static void GetPolicyMultipliers(ManufacturingMobilizationPolicy policy, out float military, out float civilian)
        {
            switch (policy)
            {
                case ManufacturingMobilizationPolicy.PartialMobilization:
                    military = 1.5f;
                    civilian = 0.8f;
                    break;
                case ManufacturingMobilizationPolicy.TotalMobilization:
                    military = 2f;
                    civilian = 0.2f;
                    break;
                case ManufacturingMobilizationPolicy.ResourceSaving:
                    military = 0.5f;
                    civilian = 1.2f;
                    break;
                default:
                    military = 1f;
                    civilian = 1f;
                    break;
            }
        }

        public static float ComputeRetoolingPenalty01(byte switchDaysRemaining)
        {
            return switchDaysRemaining > 0 ? 0.5f : 1f;
        }

        public static float ComputePlantEfficiency(
            ushort assignedWorkers,
            ushort workerSlots,
            float automation01,
            float condition01,
            float workerQuality01,
            float energyRatio01)
        {
            var workerUtil = workerSlots <= 0 ? 0f : math.saturate(assignedWorkers / math.max(1f, workerSlots));
            var workerFactor = 0.35f + 0.65f * workerUtil;
            var qualityFactor = 0.6f + 0.5f * math.saturate(workerQuality01);
            var automationFactor = 0.75f + 0.5f * math.saturate(automation01);
            var conditionFactor = 0.4f + 0.6f * math.saturate(condition01);
            var energyFactor = 0.3f + 0.7f * math.saturate(energyRatio01);
            return workerFactor * qualityFactor * automationFactor * conditionFactor * energyFactor;
        }

        public static GameEpoch ToGameEpoch(TechEraId era)
        {
            return era switch
            {
                TechEraId.Era1_Foundation => GameEpoch.Epoch1_Foundation,
                TechEraId.Era2_Industrialization => GameEpoch.Epoch2_Industrialization,
                TechEraId.Era3_WorldWar1 => GameEpoch.Epoch3_WorldWar1,
                TechEraId.Era4_WorldWar2 => GameEpoch.Epoch4_WorldWar2,
                TechEraId.Era5_ModernFuture => GameEpoch.Epoch5_Modern,
                _ => GameEpoch.Epoch1_Foundation
            };
        }

        public static bool TryGetProductDefinition(ManufacturingProductKind kind, out ManufacturingProductDefinition def)
        {
            switch (kind)
            {
                case ManufacturingProductKind.Musket:
                    def = Build(ManufacturingPlantCategory.Military, GameEpoch.Epoch1_Foundation,
                        ResourceId.SteelBasic, 5f, ResourceId.Wood, 2f, ResourceId.None, 0f,
                        ResourceId.MusketFirearm, 1f, 1f, true);
                    return true;
                case ManufacturingProductKind.Pike:
                    def = Build(ManufacturingPlantCategory.Military, GameEpoch.Epoch1_Foundation,
                        ResourceId.SteelBasic, 2f, ResourceId.Wood, 2f, ResourceId.None, 0f,
                        ResourceId.PikeWeapon, 1f, 1f, true);
                    return true;
                case ManufacturingProductKind.GunpowderBatch:
                    def = Build(ManufacturingPlantCategory.Military, GameEpoch.Epoch1_Foundation,
                        ResourceId.Saltpeter, 1f, ResourceId.Sulfur, 1f, ResourceId.Coal, 1f,
                        ResourceId.Gunpowder, 10f, 1f, true);
                    return true;
                case ManufacturingProductKind.BronzeCannon:
                    def = Build(ManufacturingPlantCategory.Military, GameEpoch.Epoch1_Foundation,
                        ResourceId.Bronze, 50f, ResourceId.None, 0f, ResourceId.None, 0f,
                        ResourceId.BronzeCannonEpoch1, 1f, 4f, true);
                    return true;
                case ManufacturingProductKind.BoltActionRifle:
                    def = Build(ManufacturingPlantCategory.Military, GameEpoch.Epoch2_Industrialization,
                        ResourceId.SteelIndustrial, 5f, ResourceId.Wood, 2f, ResourceId.None, 0f,
                        ResourceId.MilitaryRifleEpoch2, 1f, 0.1f, true);
                    return true;
                case ManufacturingProductKind.MachineGun:
                    def = Build(ManufacturingPlantCategory.Military, GameEpoch.Epoch2_Industrialization,
                        ResourceId.SteelIndustrial, 20f, ResourceId.Wood, 5f, ResourceId.None, 0f,
                        ResourceId.None, 1f, 0.5f, true);
                    return true;
                case ManufacturingProductKind.Howitzer:
                    def = Build(ManufacturingPlantCategory.Military, GameEpoch.Epoch2_Industrialization,
                        ResourceId.SteelIndustrial, 100f, ResourceId.None, 0f, ResourceId.None, 0f,
                        ResourceId.ArtilleryShellExplosiveEpoch2, 4f, 1f, true);
                    return true;
                case ManufacturingProductKind.CartridgeBatch:
                    def = Build(ManufacturingPlantCategory.Military, GameEpoch.Epoch2_Industrialization,
                        ResourceId.Brass, 1f, ResourceId.Gunpowder, 1f, ResourceId.LeadOre, 1f,
                        ResourceId.ArtilleryShellExplosiveEpoch2, 20f, 0.05f, true);
                    return true;
                case ManufacturingProductKind.AssaultRifle:
                    def = Build(ManufacturingPlantCategory.Military, GameEpoch.Epoch4_WorldWar2,
                        ResourceId.SteelIndustrial, 3f, ResourceId.PlasticBakelite, 1f, ResourceId.None, 0f,
                        ResourceId.MilitaryRifleMagazineWW1, 1f, 0.02f, true);
                    return true;
                case ManufacturingProductKind.MediumTank:
                    def = Build(ManufacturingPlantCategory.Military, GameEpoch.Epoch4_WorldWar2,
                        ResourceId.SteelIndustrial, 200f, ResourceId.SteelArmorPiercing, 50f, ResourceId.PetroleumProducts, 50f,
                        ResourceId.None, 1f, 72f, true);
                    return true;
                case ManufacturingProductKind.CombatDrone:
                    def = Build(ManufacturingPlantCategory.Military, GameEpoch.Epoch5_Modern,
                        ResourceId.CompositeMaterials, 100f, ResourceId.ChemicalReagents, 50f, ResourceId.PureSilicon, 20f,
                        ResourceId.None, 1f, 48f, true);
                    return true;
                case ManufacturingProductKind.BallisticMissile:
                    def = Build(ManufacturingPlantCategory.Military, GameEpoch.Epoch5_Modern,
                        ResourceId.CompositeMaterials, 1000f, ResourceId.ChemicalReagents, 500f, ResourceId.Superalloys, 120f,
                        ResourceId.None, 1f, 336f, true);
                    return true;

                case ManufacturingProductKind.BreadBatch:
                    def = Build(ManufacturingPlantCategory.Civilian, GameEpoch.Epoch1_Foundation,
                        ResourceId.CropWheat, 100f, ResourceId.None, 0f, ResourceId.None, 0f,
                        ResourceId.CropWheat, 70f, 1f, false);
                    return true;
                case ManufacturingProductKind.CannedFoodBatch:
                    def = Build(ManufacturingPlantCategory.Civilian, GameEpoch.Epoch2_Industrialization,
                        ResourceId.CropVegetables, 25f, ResourceId.LivestockMeat, 25f, ResourceId.SteelRolledPlate, 10f,
                        ResourceId.None, 40f, 1f, false);
                    return true;
                case ManufacturingProductKind.ClothBatch:
                    def = Build(ManufacturingPlantCategory.Civilian, GameEpoch.Epoch1_Foundation,
                        ResourceId.CropWheat, 50f, ResourceId.LivestockWool, 20f, ResourceId.None, 0f,
                        ResourceId.Cloth, 50f, 1f, false);
                    return true;
                case ManufacturingProductKind.ClothingBatch:
                    def = Build(ManufacturingPlantCategory.Civilian, GameEpoch.Epoch1_Foundation,
                        ResourceId.Cloth, 20f, ResourceId.None, 0f, ResourceId.None, 0f,
                        ResourceId.MilitaryUniformEpoch1, 20f, 1f, false);
                    return true;
                case ManufacturingProductKind.FurnitureSet:
                    def = Build(ManufacturingPlantCategory.Civilian, GameEpoch.Epoch1_Foundation,
                        ResourceId.Wood, 20f, ResourceId.Cloth, 5f, ResourceId.None, 0f,
                        ResourceId.None, 5f, 1f, false);
                    return true;
                case ManufacturingProductKind.ConsumerElectronics:
                    def = Build(ManufacturingPlantCategory.Civilian, GameEpoch.Epoch4_WorldWar2,
                        ResourceId.PlasticBakelite, 20f, ResourceId.CopperIngot, 10f, ResourceId.None, 0f,
                        ResourceId.None, 2f, 0.5f, false);
                    return true;
                case ManufacturingProductKind.Computer:
                    def = Build(ManufacturingPlantCategory.Civilian, GameEpoch.Epoch4_WorldWar2,
                        ResourceId.PlasticBakelite, 50f, ResourceId.ChemicalReagents, 50f, ResourceId.PureSilicon, 10f,
                        ResourceId.None, 1f, 24f, false);
                    return true;
                case ManufacturingProductKind.MicrochipBatch:
                    def = Build(ManufacturingPlantCategory.Civilian, GameEpoch.Epoch5_Modern,
                        ResourceId.PureSilicon, 10f, ResourceId.RareEarthMetals, 5f, ResourceId.None, 0f,
                        ResourceId.None, 100f, 1f, false);
                    return true;

                case ManufacturingProductKind.CastIronBatch:
                    def = Build(ManufacturingPlantCategory.HeavyIndustry, GameEpoch.Epoch2_Industrialization,
                        ResourceId.IronOre, 10f, ResourceId.CoalCoke, 5f, ResourceId.None, 0f,
                        ResourceId.CastIron, 10f, 1f, false);
                    return true;
                case ManufacturingProductKind.SteelIndustrialBatch:
                    def = Build(ManufacturingPlantCategory.HeavyIndustry, GameEpoch.Epoch2_Industrialization,
                        ResourceId.CastIron, 10f, ResourceId.Quicklime, 2f, ResourceId.None, 0f,
                        ResourceId.SteelIndustrial, 8f, 1f, false);
                    return true;
                case ManufacturingProductKind.AlloySteelBatch:
                    def = Build(ManufacturingPlantCategory.HeavyIndustry, GameEpoch.Epoch3_WorldWar1,
                        ResourceId.SteelIndustrial, 8f, ResourceId.ChromiteOre, 1f, ResourceId.NickelOre, 1f,
                        ResourceId.SteelAlloyed, 5f, 1f, false);
                    return true;
                case ManufacturingProductKind.SteelRolledBatch:
                    def = Build(ManufacturingPlantCategory.HeavyIndustry, GameEpoch.Epoch2_Industrialization,
                        ResourceId.SteelIndustrial, 10f, ResourceId.None, 0f, ResourceId.None, 0f,
                        ResourceId.SteelRolledPlate, 20f, 1f, false);
                    return true;
                case ManufacturingProductKind.PetroleumProductsBatch:
                    def = Build(ManufacturingPlantCategory.HeavyIndustry, GameEpoch.Epoch3_WorldWar1,
                        ResourceId.Oil, 100f, ResourceId.None, 0f, ResourceId.None, 0f,
                        ResourceId.PetroleumProducts, 100f, 1f, false);
                    return true;
                case ManufacturingProductKind.PlasticBatch:
                    def = Build(ManufacturingPlantCategory.HeavyIndustry, GameEpoch.Epoch4_WorldWar2,
                        ResourceId.PetroleumProducts, 20f, ResourceId.ChemicalReagents, 2f, ResourceId.None, 0f,
                        ResourceId.PlasticBakelite, 30f, 1f, false);
                    return true;
                case ManufacturingProductKind.MedicineBatch:
                    def = Build(ManufacturingPlantCategory.HeavyIndustry, GameEpoch.Epoch4_WorldWar2,
                        ResourceId.ChemicalReagents, 5f, ResourceId.Cloth, 1f, ResourceId.None, 0f,
                        ResourceId.ChemicalReagents, 10f, 1f, false);
                    return true;
                case ManufacturingProductKind.ExplosiveBatch:
                    def = Build(ManufacturingPlantCategory.HeavyIndustry, GameEpoch.Epoch3_WorldWar1,
                        ResourceId.ChemicalReagents, 10f, ResourceId.Saltpeter, 6f, ResourceId.Sulfur, 6f,
                        ResourceId.Dynamite, 20f, 1f, false);
                    return true;
            }

            def = default;
            return false;
        }

        private static ManufacturingProductDefinition Build(
            ManufacturingPlantCategory category,
            GameEpoch minEpoch,
            ResourceId in0,
            float amount0,
            ResourceId in1,
            float amount1,
            ResourceId in2,
            float amount2,
            ResourceId output,
            float outputAmount,
            float hours,
            bool isMilitary)
        {
            return new ManufacturingProductDefinition
            {
                Category = category,
                MinEpoch = minEpoch,
                In0 = in0,
                Amount0 = amount0,
                In1 = in1,
                Amount1 = amount1,
                In2 = in2,
                Amount2 = amount2,
                OutputResource = output,
                OutputAmount = outputAmount,
                BaseHoursPerUnit = hours,
                IsMilitary = isMilitary ? (byte)1 : (byte)0
            };
        }
    }
}
