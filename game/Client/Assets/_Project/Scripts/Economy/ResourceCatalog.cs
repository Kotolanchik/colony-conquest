using System.Collections.Generic;
using UnityEngine;

namespace ColonyConquest.Economy
{
    /// <summary>
    /// Статический каталог ресурсов по §1.2 <c>spec/economic_system_specification.md</c>.
    /// Индекс массива = (int)<see cref="ResourceId"/>; слот 0 — неиспользуемый (None).
    /// </summary>
    public static class ResourceCatalog
    {
        public const int MaxResourceIdInclusive = (int)ResourceId.MilitaryRifleMagazineWW1;
        public const int DefinedResourceCount = MaxResourceIdInclusive;

        private static readonly ResourceDefinition[] Table = BuildTable();

        public static ResourceDefinition Get(ResourceId id)
        {
            var i = (int)id;
            if (i <= 0 || i > MaxResourceIdInclusive || i >= Table.Length)
                return default;
            return Table[i];
        }

        public static bool TryGet(ResourceId id, out ResourceDefinition def)
        {
            def = Get(id);
            return id != ResourceId.None && def.Id != ResourceId.None;
        }

        /// <summary>Все определённые ресурсы (без None), порядок по <see cref="ResourceId"/>.</summary>
        public static void GetAllNonEmpty(List<ResourceDefinition> buffer)
        {
            buffer.Clear();
            for (var i = 1; i <= MaxResourceIdInclusive; i++)
            {
                var d = Table[i];
                if (d.Id != ResourceId.None)
                    buffer.Add(d);
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ValidateTable()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            for (var i = 1; i <= MaxResourceIdInclusive; i++)
            {
                var d = Table[i];
                if ((int)d.Id != i)
                    Debug.LogError($"[Economy] ResourceCatalog: индекс {i} не совпадает с Id {d.Id}.");
            }
#endif
        }

        private static ResourceDefinition[] BuildTable()
        {
            var t = new ResourceDefinition[MaxResourceIdInclusive + 1];
            void Set(
                ResourceId id,
                ResourceCategory cat,
                ResourceRarity rarity,
                GameEpoch epoch,
                ushort price)
            {
                t[(int)id] = new ResourceDefinition(id, cat, rarity, epoch, price);
            }

            // Эпоха 1
            Set(ResourceId.IronOre, ResourceCategory.Raw, ResourceRarity.Common, GameEpoch.Epoch1_Foundation, 2);
            Set(ResourceId.CopperOre, ResourceCategory.Raw, ResourceRarity.Common, GameEpoch.Epoch1_Foundation, 3);
            Set(ResourceId.Stone, ResourceCategory.Raw, ResourceRarity.Common, GameEpoch.Epoch1_Foundation, 1);
            Set(ResourceId.Wood, ResourceCategory.Raw, ResourceRarity.Common, GameEpoch.Epoch1_Foundation, 1);
            Set(ResourceId.Coal, ResourceCategory.Raw, ResourceRarity.Common, GameEpoch.Epoch1_Foundation, 2);
            Set(ResourceId.GoldOre, ResourceCategory.Raw, ResourceRarity.Rare, GameEpoch.Epoch1_Foundation, 20);
            Set(ResourceId.IronIngot, ResourceCategory.Processed, ResourceRarity.NotApplicable, GameEpoch.Epoch1_Foundation, 5);
            Set(ResourceId.CopperIngot, ResourceCategory.Processed, ResourceRarity.NotApplicable, GameEpoch.Epoch1_Foundation, 7);
            Set(ResourceId.GoldIngot, ResourceCategory.Processed, ResourceRarity.NotApplicable, GameEpoch.Epoch1_Foundation, 50);
            Set(ResourceId.Planks, ResourceCategory.Processed, ResourceRarity.NotApplicable, GameEpoch.Epoch1_Foundation, 2);
            Set(ResourceId.Brick, ResourceCategory.Processed, ResourceRarity.NotApplicable, GameEpoch.Epoch1_Foundation, 3);
            Set(ResourceId.CoalCoke, ResourceCategory.Processed, ResourceRarity.NotApplicable, GameEpoch.Epoch1_Foundation, 4);
            Set(ResourceId.SteelBasic, ResourceCategory.Material, ResourceRarity.NotApplicable, GameEpoch.Epoch1_Foundation, 15);
            Set(ResourceId.Gunpowder, ResourceCategory.Material, ResourceRarity.NotApplicable, GameEpoch.Epoch1_Foundation, 10);
            Set(ResourceId.Cloth, ResourceCategory.Material, ResourceRarity.NotApplicable, GameEpoch.Epoch1_Foundation, 5);
            Set(ResourceId.Leather, ResourceCategory.Material, ResourceRarity.NotApplicable, GameEpoch.Epoch1_Foundation, 8);

            // Эпоха 2
            Set(ResourceId.Oil, ResourceCategory.Raw, ResourceRarity.Common, GameEpoch.Epoch2_Industrialization, 5);
            Set(ResourceId.TinOre, ResourceCategory.Raw, ResourceRarity.Rare, GameEpoch.Epoch2_Industrialization, 8);
            Set(ResourceId.LeadOre, ResourceCategory.Raw, ResourceRarity.Common, GameEpoch.Epoch2_Industrialization, 4);
            Set(ResourceId.NickelOre, ResourceCategory.Raw, ResourceRarity.Rare, GameEpoch.Epoch2_Industrialization, 12);
            Set(ResourceId.NaturalRubber, ResourceCategory.Raw, ResourceRarity.Rare, GameEpoch.Epoch2_Industrialization, 15);
            Set(ResourceId.SteelIndustrial, ResourceCategory.Material, ResourceRarity.NotApplicable, GameEpoch.Epoch2_Industrialization, 12);
            Set(ResourceId.CastIron, ResourceCategory.Material, ResourceRarity.NotApplicable, GameEpoch.Epoch2_Industrialization, 8);
            Set(ResourceId.Brass, ResourceCategory.Material, ResourceRarity.NotApplicable, GameEpoch.Epoch2_Industrialization, 18);
            Set(ResourceId.Bronze, ResourceCategory.Material, ResourceRarity.NotApplicable, GameEpoch.Epoch2_Industrialization, 20);
            Set(ResourceId.PetroleumProducts, ResourceCategory.Material, ResourceRarity.NotApplicable, GameEpoch.Epoch2_Industrialization, 10);
            Set(ResourceId.Dynamite, ResourceCategory.Material, ResourceRarity.NotApplicable, GameEpoch.Epoch2_Industrialization, 25);
            Set(ResourceId.SyntheticGunpowder, ResourceCategory.Material, ResourceRarity.NotApplicable, GameEpoch.Epoch2_Industrialization, 8);

            // Эпоха 3
            Set(ResourceId.TungstenOre, ResourceCategory.Raw, ResourceRarity.Rare, GameEpoch.Epoch3_WorldWar1, 25);
            Set(ResourceId.ChromiteOre, ResourceCategory.Raw, ResourceRarity.Rare, GameEpoch.Epoch3_WorldWar1, 20);
            Set(ResourceId.MolybdenumOre, ResourceCategory.Raw, ResourceRarity.VeryRare, GameEpoch.Epoch3_WorldWar1, 40);
            Set(ResourceId.SteelAlloyed, ResourceCategory.Material, ResourceRarity.NotApplicable, GameEpoch.Epoch3_WorldWar1, 25);
            Set(ResourceId.SteelArmorPiercing, ResourceCategory.Material, ResourceRarity.NotApplicable, GameEpoch.Epoch3_WorldWar1, 40);
            Set(ResourceId.Aluminum, ResourceCategory.Material, ResourceRarity.NotApplicable, GameEpoch.Epoch3_WorldWar1, 30);
            Set(ResourceId.SyntheticRubber, ResourceCategory.Material, ResourceRarity.NotApplicable, GameEpoch.Epoch3_WorldWar1, 20);
            Set(ResourceId.ChemicalReagents, ResourceCategory.Material, ResourceRarity.NotApplicable, GameEpoch.Epoch3_WorldWar1, 15);
            Set(ResourceId.PoisonGas, ResourceCategory.Material, ResourceRarity.NotApplicable, GameEpoch.Epoch3_WorldWar1, 35);

            // Эпоха 4
            Set(ResourceId.UraniumOre, ResourceCategory.Raw, ResourceRarity.VeryRare, GameEpoch.Epoch4_WorldWar2, 100);
            Set(ResourceId.TitaniumOre, ResourceCategory.Raw, ResourceRarity.Rare, GameEpoch.Epoch4_WorldWar2, 50);
            Set(ResourceId.ManganeseOre, ResourceCategory.Raw, ResourceRarity.Common, GameEpoch.Epoch4_WorldWar2, 15);
            Set(ResourceId.PlasticBakelite, ResourceCategory.Material, ResourceRarity.NotApplicable, GameEpoch.Epoch4_WorldWar2, 25);
            Set(ResourceId.PlasticModern, ResourceCategory.Material, ResourceRarity.NotApplicable, GameEpoch.Epoch4_WorldWar2, 20);
            Set(ResourceId.SyntheticFuel, ResourceCategory.Material, ResourceRarity.NotApplicable, GameEpoch.Epoch4_WorldWar2, 30);
            Set(ResourceId.HighOctaneGasoline, ResourceCategory.Material, ResourceRarity.NotApplicable, GameEpoch.Epoch4_WorldWar2, 25);
            Set(ResourceId.CompositeMaterials, ResourceCategory.Material, ResourceRarity.NotApplicable, GameEpoch.Epoch4_WorldWar2, 45);
            Set(ResourceId.NuclearMaterial, ResourceCategory.Material, ResourceRarity.NotApplicable, GameEpoch.Epoch4_WorldWar2, 500);

            // Эпоха 5
            Set(ResourceId.RareEarthMetals, ResourceCategory.Raw, ResourceRarity.VeryRare, GameEpoch.Epoch5_Modern, 80);
            Set(ResourceId.PlatinumGroup, ResourceCategory.Raw, ResourceRarity.VeryRare, GameEpoch.Epoch5_Modern, 150);
            Set(ResourceId.PureSilicon, ResourceCategory.Material, ResourceRarity.NotApplicable, GameEpoch.Epoch5_Modern, 60);
            Set(ResourceId.Graphene, ResourceCategory.Material, ResourceRarity.NotApplicable, GameEpoch.Epoch5_Modern, 200);
            Set(ResourceId.CarbonFiber, ResourceCategory.Material, ResourceRarity.NotApplicable, GameEpoch.Epoch5_Modern, 120);
            Set(ResourceId.CeramicComposites, ResourceCategory.Material, ResourceRarity.NotApplicable, GameEpoch.Epoch5_Modern, 100);
            Set(ResourceId.Superalloys, ResourceCategory.Material, ResourceRarity.NotApplicable, GameEpoch.Epoch5_Modern, 180);
            Set(ResourceId.QuantumMaterials, ResourceCategory.Material, ResourceRarity.NotApplicable, GameEpoch.Epoch5_Modern, 500);
            Set(ResourceId.Antimatter, ResourceCategory.Material, ResourceRarity.NotApplicable, GameEpoch.Epoch5_Modern, 5000);

            // Сельхоз продукты (agriculture_mining_spec §1.1, §1.4)
            Set(ResourceId.CropWheat, ResourceCategory.Processed, ResourceRarity.Common, GameEpoch.Epoch1_Foundation, 2);
            Set(ResourceId.CropBarley, ResourceCategory.Processed, ResourceRarity.Common, GameEpoch.Epoch1_Foundation, 2);
            Set(ResourceId.CropOat, ResourceCategory.Processed, ResourceRarity.Common, GameEpoch.Epoch1_Foundation, 2);
            Set(ResourceId.CropRye, ResourceCategory.Processed, ResourceRarity.Common, GameEpoch.Epoch1_Foundation, 2);
            Set(ResourceId.CropCorn, ResourceCategory.Processed, ResourceRarity.Common, GameEpoch.Epoch1_Foundation, 3);
            Set(ResourceId.CropPotato, ResourceCategory.Processed, ResourceRarity.Common, GameEpoch.Epoch1_Foundation, 2);
            Set(ResourceId.CropVegetables, ResourceCategory.Processed, ResourceRarity.Common, GameEpoch.Epoch1_Foundation, 4);
            Set(ResourceId.CropFruits, ResourceCategory.Processed, ResourceRarity.Common, GameEpoch.Epoch1_Foundation, 5);
            Set(ResourceId.LivestockEggs, ResourceCategory.Processed, ResourceRarity.Common, GameEpoch.Epoch1_Foundation, 3);
            Set(ResourceId.LivestockMilk, ResourceCategory.Processed, ResourceRarity.Common, GameEpoch.Epoch1_Foundation, 4);
            Set(ResourceId.LivestockWool, ResourceCategory.Processed, ResourceRarity.Common, GameEpoch.Epoch1_Foundation, 6);
            Set(ResourceId.LivestockMeat, ResourceCategory.Processed, ResourceRarity.Common, GameEpoch.Epoch1_Foundation, 8);

            Set(ResourceId.Sulfur, ResourceCategory.Raw, ResourceRarity.Common, GameEpoch.Epoch1_Foundation, 4);
            Set(ResourceId.Saltpeter, ResourceCategory.Raw, ResourceRarity.Common, GameEpoch.Epoch1_Foundation, 5);
            Set(ResourceId.Epoch1Tools, ResourceCategory.Component, ResourceRarity.Common, GameEpoch.Epoch1_Foundation, 25);
            Set(ResourceId.MusketFirearm, ResourceCategory.FinalProduct, ResourceRarity.Common, GameEpoch.Epoch1_Foundation, 80);
            Set(ResourceId.PikeWeapon, ResourceCategory.FinalProduct, ResourceRarity.Common, GameEpoch.Epoch1_Foundation, 40);

            Set(ResourceId.RawClay, ResourceCategory.Raw, ResourceRarity.Common, GameEpoch.Epoch1_Foundation, 2);
            Set(ResourceId.Sand, ResourceCategory.Raw, ResourceRarity.Common, GameEpoch.Epoch1_Foundation, 1);
            Set(ResourceId.SilverOre, ResourceCategory.Raw, ResourceRarity.Rare, GameEpoch.Epoch2_Industrialization, 25);
            Set(ResourceId.RawDiamonds, ResourceCategory.Raw, ResourceRarity.VeryRare, GameEpoch.Epoch4_WorldWar2, 200);
            Set(ResourceId.Helium3, ResourceCategory.Raw, ResourceRarity.VeryRare, GameEpoch.Epoch5_Modern, 300);

            Set(ResourceId.Quicklime, ResourceCategory.Processed, ResourceRarity.NotApplicable, GameEpoch.Epoch2_Industrialization, 6);
            Set(ResourceId.SteelRolledPlate, ResourceCategory.Material, ResourceRarity.NotApplicable, GameEpoch.Epoch2_Industrialization, 18);
            Set(ResourceId.FishCatch, ResourceCategory.Processed, ResourceRarity.Common, GameEpoch.Epoch1_Foundation, 6);
            Set(ResourceId.StoneRoundShot, ResourceCategory.Component, ResourceRarity.NotApplicable, GameEpoch.Epoch1_Foundation, 4);
            Set(ResourceId.BronzeCannonEpoch1, ResourceCategory.FinalProduct, ResourceRarity.NotApplicable, GameEpoch.Epoch1_Foundation, 400);
            Set(ResourceId.CalibratedSteelPlate, ResourceCategory.Material, ResourceRarity.NotApplicable, GameEpoch.Epoch2_Industrialization, 22);
            Set(ResourceId.MilitaryUniformEpoch1, ResourceCategory.FinalProduct, ResourceRarity.NotApplicable, GameEpoch.Epoch1_Foundation, 35);
            Set(ResourceId.PlywoodEpoch1, ResourceCategory.Processed, ResourceRarity.NotApplicable, GameEpoch.Epoch1_Foundation, 8);
            Set(ResourceId.MilitaryRifleEpoch2, ResourceCategory.FinalProduct, ResourceRarity.NotApplicable, GameEpoch.Epoch2_Industrialization, 120);
            Set(ResourceId.RevolverEpoch2, ResourceCategory.FinalProduct, ResourceRarity.NotApplicable, GameEpoch.Epoch2_Industrialization, 90);
            Set(ResourceId.ArtilleryShellExplosiveEpoch2, ResourceCategory.Component, ResourceRarity.NotApplicable, GameEpoch.Epoch2_Industrialization, 45);
            Set(ResourceId.SilverIngot, ResourceCategory.Processed, ResourceRarity.NotApplicable, GameEpoch.Epoch2_Industrialization, 35);
            Set(ResourceId.HandGrenadeWW1, ResourceCategory.FinalProduct, ResourceRarity.NotApplicable, GameEpoch.Epoch3_WorldWar1, 25);
            Set(ResourceId.LandMineWW1, ResourceCategory.FinalProduct, ResourceRarity.NotApplicable, GameEpoch.Epoch3_WorldWar1, 40);
            Set(ResourceId.ChemicalArtilleryShellWW1, ResourceCategory.FinalProduct, ResourceRarity.NotApplicable, GameEpoch.Epoch3_WorldWar1, 120);
            Set(ResourceId.MilitaryRifleMagazineWW1, ResourceCategory.FinalProduct, ResourceRarity.NotApplicable, GameEpoch.Epoch3_WorldWar1, 140);

            return t;
        }
    }
}
