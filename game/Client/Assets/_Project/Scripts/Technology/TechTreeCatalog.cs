using Unity.Collections;

namespace ColonyConquest.Technology
{
    /// <summary>Компактный каталог технологий с требованиями и ветками.</summary>
    public static class TechTreeCatalog
    {
        public struct TechDefinition
        {
            public TechDefinitionId Id;
            public TechEraId Era;
            public TechBranchId Branch;
            public float CostPoints;
            public TechDefinitionId Prerequisite;
        }

        private static readonly TechDefinition[] Definitions =
        {
            // Era 1
            Def(TechDefinitionId.ImprovedSmelting, TechEraId.Era1_Foundation, TechBranchId.Economic, 500f),
            Def(TechDefinitionId.BrickProduction, TechEraId.Era1_Foundation, TechBranchId.Infrastructure, 800f),
            Def(TechDefinitionId.Steelworking, TechEraId.Era1_Foundation, TechBranchId.Economic, 1000f,
                TechDefinitionId.ImprovedSmelting),
            Def(TechDefinitionId.Gunpowder, TechEraId.Era1_Foundation, TechBranchId.Military, 1200f),
            Def(TechDefinitionId.PrintingPress, TechEraId.Era1_Foundation, TechBranchId.Scientific, 2000f),

            // Era 2
            Def(TechDefinitionId.SteamEngine, TechEraId.Era2_Industrialization, TechBranchId.Infrastructure, 2000f,
                TechDefinitionId.Steelworking),
            Def(TechDefinitionId.BlastFurnace, TechEraId.Era2_Industrialization, TechBranchId.Economic, 2500f,
                TechDefinitionId.SteamEngine),
            Def(TechDefinitionId.Railway, TechEraId.Era2_Industrialization, TechBranchId.Infrastructure, 3000f,
                TechDefinitionId.SteamEngine),
            Def(TechDefinitionId.OilExtraction, TechEraId.Era2_Industrialization, TechBranchId.Economic, 2800f),
            Def(TechDefinitionId.MassProduction, TechEraId.Era2_Industrialization, TechBranchId.Economic, 5000f,
                TechDefinitionId.BlastFurnace),

            // Era 3
            Def(TechDefinitionId.Electricity, TechEraId.Era3_WorldWar1, TechBranchId.Scientific, 5000f,
                TechDefinitionId.SteamEngine),
            Def(TechDefinitionId.InternalCombustion, TechEraId.Era3_WorldWar1, TechBranchId.Economic, 7000f,
                TechDefinitionId.OilExtraction),
            Def(TechDefinitionId.TankMk1, TechEraId.Era3_WorldWar1, TechBranchId.Military, 8000f,
                TechDefinitionId.InternalCombustion),
            Def(TechDefinitionId.Aviation, TechEraId.Era3_WorldWar1, TechBranchId.Military, 9000f,
                TechDefinitionId.InternalCombustion),
            Def(TechDefinitionId.Radio, TechEraId.Era3_WorldWar1, TechBranchId.Scientific, 7000f,
                TechDefinitionId.Electricity),

            // Era 4
            Def(TechDefinitionId.Radar, TechEraId.Era4_WorldWar2, TechBranchId.Scientific, 10000f,
                TechDefinitionId.Radio),
            Def(TechDefinitionId.JetEngine, TechEraId.Era4_WorldWar2, TechBranchId.Military, 15000f,
                TechDefinitionId.Aviation),
            Def(TechDefinitionId.NuclearPhysics, TechEraId.Era4_WorldWar2, TechBranchId.Scientific, 25000f,
                TechDefinitionId.Radar),
            Def(TechDefinitionId.Computer, TechEraId.Era4_WorldWar2, TechBranchId.Scientific, 20000f,
                TechDefinitionId.Radar),
            Def(TechDefinitionId.MediumTank, TechEraId.Era4_WorldWar2, TechBranchId.Military, 15000f,
                TechDefinitionId.TankMk1),

            // Era 5
            Def(TechDefinitionId.NuclearReactor, TechEraId.Era5_ModernFuture, TechBranchId.Infrastructure, 50000f,
                TechDefinitionId.NuclearPhysics),
            Def(TechDefinitionId.Internet, TechEraId.Era5_ModernFuture, TechBranchId.Scientific, 60000f,
                TechDefinitionId.Computer),
            Def(TechDefinitionId.CompositeMaterials, TechEraId.Era5_ModernFuture, TechBranchId.Economic, 45000f,
                TechDefinitionId.Computer),
            Def(TechDefinitionId.QuantumComputer, TechEraId.Era5_ModernFuture, TechBranchId.Scientific, 150000f,
                TechDefinitionId.Computer),
            Def(TechDefinitionId.WeakAi, TechEraId.Era5_ModernFuture, TechBranchId.Scientific, 150000f,
                TechDefinitionId.QuantumComputer),
        };

        public static NativeArray<TechDefinition> GetAll(Allocator allocator)
        {
            var arr = new NativeArray<TechDefinition>(Definitions.Length, allocator);
            for (var i = 0; i < Definitions.Length; i++)
                arr[i] = Definitions[i];
            return arr;
        }

        public static bool TryGet(TechDefinitionId id, out TechDefinition def)
        {
            for (var i = 0; i < Definitions.Length; i++)
            {
                if (Definitions[i].Id != id)
                    continue;
                def = Definitions[i];
                return true;
            }

            def = default;
            return false;
        }

        public static int CountEraTechs(TechEraId era)
        {
            var c = 0;
            for (var i = 0; i < Definitions.Length; i++)
            {
                if (Definitions[i].Era == era)
                    c++;
            }

            return c;
        }

        private static TechDefinition Def(TechDefinitionId id, TechEraId era, TechBranchId branch, float cost,
            TechDefinitionId prereq = TechDefinitionId.None)
        {
            return new TechDefinition
            {
                Id = id,
                Era = era,
                Branch = branch,
                CostPoints = cost,
                Prerequisite = prereq
            };
        }
    }
}
