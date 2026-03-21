namespace ColonyConquest.Economy
{
    /// <summary>Статические рецепты по §2.2–2.4 <c>spec/economic_system_specification.md</c>.</summary>
    public static class ProductionRecipeCatalog
    {
        /// <summary>
        /// Порядок выбора: сначала готовая продукция и «верх» цепочек (в т.ч. эпоха 2), затем полуфабрикаты, затем плавки.
        /// </summary>
        public static readonly ProductionRecipeId[] SelectionPriorityOrder =
        {
            ProductionRecipeId.CalibrateSteelPlate,
            ProductionRecipeId.ElectricFurnaceArmorPiercing,
            ProductionRecipeId.ElectricFurnaceAlloyedSteel,
            ProductionRecipeId.RollingMillSteelPlate,
            ProductionRecipeId.CraftExplosiveShellEpoch2,
            ProductionRecipeId.CraftHandGrenadeWW1,
            ProductionRecipeId.CraftLandMineWW1,
            ProductionRecipeId.CraftChemicalArtilleryShellWW1,
            ProductionRecipeId.CraftSyntheticRubberEpoch2,
            ProductionRecipeId.CraftPlasticBakeliteEpoch4,
            ProductionRecipeId.CraftMagazineRifleWW1,
            ProductionRecipeId.CraftBoltActionRifleEpoch2,
            ProductionRecipeId.CraftRevolverEpoch2,
            ProductionRecipeId.CraftDynamiteIndustrial,
            ProductionRecipeId.CraftBronzeCannon,
            ProductionRecipeId.CraftMusket,
            ProductionRecipeId.CraftPike,
            ProductionRecipeId.CraftStoneRoundShot,
            ProductionRecipeId.CraftGunpowder,
            ProductionRecipeId.ForgeEpoch1Tools,
            ProductionRecipeId.SewUniformEpoch1,
            ProductionRecipeId.TanLeather,
            ProductionRecipeId.WeaveClothFromWheat,
            ProductionRecipeId.CraftPlywoodEpoch1,
            ProductionRecipeId.CalcinateQuicklime,
            ProductionRecipeId.BessemerSteelIndustrial,
            ProductionRecipeId.BlastFurnaceCastIron,
            ProductionRecipeId.RefineryCrudeToPetroleum,
            ProductionRecipeId.ProduceSteelBasic,
            ProductionRecipeId.BakeBrick,
            ProductionRecipeId.CokeCoal,
            ProductionRecipeId.SawPlanks,
            ProductionRecipeId.SmeltBronzeAlloy,
            ProductionRecipeId.SmeltIronOre,
            ProductionRecipeId.SmeltCopperOre,
            ProductionRecipeId.SmeltGoldOre,
            ProductionRecipeId.SmeltSilverOre,
            ProductionRecipeId.SmeltUraniumToNuclearMaterial,
        };

        public static ProductionRecipeDefinition Get(ProductionRecipeId id)
        {
            switch (id)
            {
                case ProductionRecipeId.SmeltIronOre:
                    return new ProductionRecipeDefinition(
                        id,
                        ResourceId.IronOre, 2f,
                        ResourceId.Coal, 1f,
                        ResourceId.None, 0f,
                        ResourceId.IronIngot, 1f,
                        30f,
                        1);

                case ProductionRecipeId.ProduceSteelBasic:
                    return new ProductionRecipeDefinition(
                        id,
                        ResourceId.IronIngot, 3f,
                        ResourceId.Coal, 2f,
                        ResourceId.None, 0f,
                        ResourceId.SteelBasic, 1f,
                        45f,
                        2);

                case ProductionRecipeId.SawPlanks:
                    return new ProductionRecipeDefinition(
                        id,
                        ResourceId.Wood, 3f,
                        ResourceId.None, 0f,
                        ResourceId.None, 0f,
                        ResourceId.Planks, 2f,
                        20f,
                        1);

                case ProductionRecipeId.SmeltCopperOre:
                    return new ProductionRecipeDefinition(
                        id,
                        ResourceId.CopperOre, 2f,
                        ResourceId.Coal, 1f,
                        ResourceId.None, 0f,
                        ResourceId.CopperIngot, 1f,
                        30f,
                        1);

                case ProductionRecipeId.SmeltGoldOre:
                    return new ProductionRecipeDefinition(
                        id,
                        ResourceId.GoldOre, 2f,
                        ResourceId.Coal, 1f,
                        ResourceId.None, 0f,
                        ResourceId.GoldIngot, 1f,
                        30f,
                        1);

                case ProductionRecipeId.CokeCoal:
                    return new ProductionRecipeDefinition(
                        id,
                        ResourceId.Coal, 2f,
                        ResourceId.None, 0f,
                        ResourceId.None, 0f,
                        ResourceId.CoalCoke, 1f,
                        45f,
                        1);

                case ProductionRecipeId.BakeBrick:
                    return new ProductionRecipeDefinition(
                        id,
                        ResourceId.Stone, 2f,
                        ResourceId.Coal, 1f,
                        ResourceId.None, 0f,
                        ResourceId.Brick, 1f,
                        40f,
                        2);

                case ProductionRecipeId.ForgeEpoch1Tools:
                    return new ProductionRecipeDefinition(
                        id,
                        ResourceId.SteelBasic, 1f,
                        ResourceId.Wood, 1f,
                        ResourceId.None, 0f,
                        ResourceId.Epoch1Tools, 1f,
                        60f,
                        2);

                case ProductionRecipeId.WeaveClothFromWheat:
                    return new ProductionRecipeDefinition(
                        id,
                        ResourceId.CropWheat, 4f,
                        ResourceId.None, 0f,
                        ResourceId.None, 0f,
                        ResourceId.Cloth, 2f,
                        40f,
                        1);

                case ProductionRecipeId.TanLeather:
                    return new ProductionRecipeDefinition(
                        id,
                        ResourceId.LivestockMeat, 2f,
                        ResourceId.Coal, 1f,
                        ResourceId.None, 0f,
                        ResourceId.Leather, 1f,
                        60f,
                        1);

                case ProductionRecipeId.CraftGunpowder:
                    return new ProductionRecipeDefinition(
                        id,
                        ResourceId.Sulfur, 1f,
                        ResourceId.Coal, 2f,
                        ResourceId.Saltpeter, 1f,
                        ResourceId.Gunpowder, 1f,
                        90f,
                        2);

                case ProductionRecipeId.CraftMusket:
                    return new ProductionRecipeDefinition(
                        id,
                        ResourceId.SteelBasic, 2f,
                        ResourceId.Wood, 1f,
                        ResourceId.None, 0f,
                        ResourceId.MusketFirearm, 1f,
                        180f,
                        3);

                case ProductionRecipeId.CraftPike:
                    return new ProductionRecipeDefinition(
                        id,
                        ResourceId.SteelBasic, 1f,
                        ResourceId.Wood, 1f,
                        ResourceId.None, 0f,
                        ResourceId.PikeWeapon, 1f,
                        60f,
                        1);

                case ProductionRecipeId.BlastFurnaceCastIron:
                    return new ProductionRecipeDefinition(
                        id,
                        ResourceId.IronOre, 5f,
                        ResourceId.CoalCoke, 3f,
                        ResourceId.None, 0f,
                        ResourceId.CastIron, 3f,
                        60f,
                        4);

                case ProductionRecipeId.BessemerSteelIndustrial:
                    return new ProductionRecipeDefinition(
                        id,
                        ResourceId.CastIron, 3f,
                        ResourceId.Quicklime, 1f,
                        ResourceId.None, 0f,
                        ResourceId.SteelIndustrial, 2f,
                        45f,
                        3);

                case ProductionRecipeId.RollingMillSteelPlate:
                    return new ProductionRecipeDefinition(
                        id,
                        ResourceId.SteelIndustrial, 2f,
                        ResourceId.None, 0f,
                        ResourceId.None, 0f,
                        ResourceId.SteelRolledPlate, 4f,
                        30f,
                        2);

                case ProductionRecipeId.RefineryCrudeToPetroleum:
                    return new ProductionRecipeDefinition(
                        id,
                        ResourceId.Oil, 10f,
                        ResourceId.None, 0f,
                        ResourceId.None, 0f,
                        ResourceId.PetroleumProducts, 10f,
                        120f,
                        2);

                case ProductionRecipeId.CraftDynamiteIndustrial:
                    return new ProductionRecipeDefinition(
                        id,
                        ResourceId.Sulfur, 2f,
                        ResourceId.Coal, 4f,
                        ResourceId.Saltpeter, 2f,
                        ResourceId.Dynamite, 2f,
                        90f,
                        2);

                case ProductionRecipeId.CalcinateQuicklime:
                    return new ProductionRecipeDefinition(
                        id,
                        ResourceId.Stone, 2f,
                        ResourceId.Coal, 1f,
                        ResourceId.None, 0f,
                        ResourceId.Quicklime, 1f,
                        40f,
                        1);

                case ProductionRecipeId.SmeltBronzeAlloy:
                    return new ProductionRecipeDefinition(
                        id,
                        ResourceId.CopperIngot, 2f,
                        ResourceId.TinOre, 1f,
                        ResourceId.None, 0f,
                        ResourceId.Bronze, 1f,
                        60f,
                        2);

                case ProductionRecipeId.CraftStoneRoundShot:
                    return new ProductionRecipeDefinition(
                        id,
                        ResourceId.Stone, 2f,
                        ResourceId.None, 0f,
                        ResourceId.None, 0f,
                        ResourceId.StoneRoundShot, 1f,
                        30f,
                        1);

                case ProductionRecipeId.CraftBronzeCannon:
                    return new ProductionRecipeDefinition(
                        id,
                        ResourceId.Bronze, 6f,
                        ResourceId.Wood, 4f,
                        ResourceId.None, 0f,
                        ResourceId.BronzeCannonEpoch1, 1f,
                        600f,
                        4);

                case ProductionRecipeId.CalibrateSteelPlate:
                    return new ProductionRecipeDefinition(
                        id,
                        ResourceId.SteelRolledPlate, 4f,
                        ResourceId.None, 0f,
                        ResourceId.None, 0f,
                        ResourceId.CalibratedSteelPlate, 6f,
                        40f,
                        2);

                case ProductionRecipeId.SewUniformEpoch1:
                    return new ProductionRecipeDefinition(
                        id,
                        ResourceId.Cloth, 1f,
                        ResourceId.Epoch1Tools, 1f,
                        ResourceId.None, 0f,
                        ResourceId.MilitaryUniformEpoch1, 1f,
                        60f,
                        2);

                case ProductionRecipeId.CraftPlywoodEpoch1:
                    return new ProductionRecipeDefinition(
                        id,
                        ResourceId.Planks, 2f,
                        ResourceId.Epoch1Tools, 1f,
                        ResourceId.None, 0f,
                        ResourceId.PlywoodEpoch1, 1f,
                        30f,
                        2);

                case ProductionRecipeId.CraftBoltActionRifleEpoch2:
                    return new ProductionRecipeDefinition(
                        id,
                        ResourceId.SteelIndustrial, 3f,
                        ResourceId.Wood, 2f,
                        ResourceId.None, 0f,
                        ResourceId.MilitaryRifleEpoch2, 1f,
                        240f,
                        3);

                case ProductionRecipeId.CraftRevolverEpoch2:
                    return new ProductionRecipeDefinition(
                        id,
                        ResourceId.SteelIndustrial, 2f,
                        ResourceId.Wood, 1f,
                        ResourceId.None, 0f,
                        ResourceId.RevolverEpoch2, 1f,
                        180f,
                        2);

                case ProductionRecipeId.CraftExplosiveShellEpoch2:
                    return new ProductionRecipeDefinition(
                        id,
                        ResourceId.SteelIndustrial, 2f,
                        ResourceId.Dynamite, 1f,
                        ResourceId.None, 0f,
                        ResourceId.ArtilleryShellExplosiveEpoch2, 1f,
                        120f,
                        2);

                case ProductionRecipeId.ElectricFurnaceArmorPiercing:
                    return new ProductionRecipeDefinition(
                        id,
                        ResourceId.SteelIndustrial, 5f,
                        ResourceId.TungstenOre, 1f,
                        ResourceId.None, 0f,
                        ResourceId.SteelArmorPiercing, 4f,
                        90f,
                        2);

                case ProductionRecipeId.ElectricFurnaceAlloyedSteel:
                    return new ProductionRecipeDefinition(
                        id,
                        ResourceId.SteelIndustrial, 5f,
                        ResourceId.ChromiteOre, 1f,
                        ResourceId.NickelOre, 1f,
                        ResourceId.SteelAlloyed, 4f,
                        120f,
                        3);

                case ProductionRecipeId.SmeltSilverOre:
                    return new ProductionRecipeDefinition(
                        id,
                        ResourceId.SilverOre, 2f,
                        ResourceId.Coal, 1f,
                        ResourceId.None, 0f,
                        ResourceId.SilverIngot, 1f,
                        30f,
                        1);

                case ProductionRecipeId.CraftHandGrenadeWW1:
                    return new ProductionRecipeDefinition(
                        id,
                        ResourceId.SteelIndustrial, 2f,
                        ResourceId.Dynamite, 1f,
                        ResourceId.None, 0f,
                        ResourceId.HandGrenadeWW1, 2f,
                        60f,
                        2);

                case ProductionRecipeId.CraftLandMineWW1:
                    return new ProductionRecipeDefinition(
                        id,
                        ResourceId.SteelIndustrial, 2f,
                        ResourceId.Dynamite, 1f,
                        ResourceId.None, 0f,
                        ResourceId.LandMineWW1, 1f,
                        120f,
                        2);

                case ProductionRecipeId.CraftSyntheticRubberEpoch2:
                    return new ProductionRecipeDefinition(
                        id,
                        ResourceId.Oil, 5f,
                        ResourceId.ChemicalReagents, 1f,
                        ResourceId.None, 0f,
                        ResourceId.SyntheticRubber, 2f,
                        60f,
                        2);

                case ProductionRecipeId.CraftChemicalArtilleryShellWW1:
                    return new ProductionRecipeDefinition(
                        id,
                        ResourceId.SteelIndustrial, 2f,
                        ResourceId.ChemicalReagents, 2f,
                        ResourceId.None, 0f,
                        ResourceId.ChemicalArtilleryShellWW1, 1f,
                        180f,
                        2);

                case ProductionRecipeId.CraftPlasticBakeliteEpoch4:
                    return new ProductionRecipeDefinition(
                        id,
                        ResourceId.Oil, 8f,
                        ResourceId.ChemicalReagents, 2f,
                        ResourceId.None, 0f,
                        ResourceId.PlasticBakelite, 4f,
                        180f,
                        3);

                case ProductionRecipeId.CraftMagazineRifleWW1:
                    return new ProductionRecipeDefinition(
                        id,
                        ResourceId.SteelIndustrial, 4f,
                        ResourceId.Wood, 2f,
                        ResourceId.None, 0f,
                        ResourceId.MilitaryRifleMagazineWW1, 1f,
                        300f,
                        3);

                case ProductionRecipeId.SmeltUraniumToNuclearMaterial:
                    return new ProductionRecipeDefinition(
                        id,
                        ResourceId.UraniumOre, 3f,
                        ResourceId.Coal, 2f,
                        ResourceId.None, 0f,
                        ResourceId.NuclearMaterial, 1f,
                        120f,
                        2);

                default:
                    return default;
            }
        }
    }
}
