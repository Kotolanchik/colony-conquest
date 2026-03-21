using Unity.Entities;

namespace ColonyConquest.Diplomacy
{
    /// <summary>Инициализация демо-фракций, отношений, торговых сделок и союзов.</summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct DiplomacyBootstrapSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            if (SystemAPI.HasSingleton<DiplomacySimulationSingleton>())
                return;

            var entity = state.EntityManager.CreateEntity();
            state.EntityManager.AddComponent<DiplomacySimulationSingleton>(entity);
            state.EntityManager.AddComponentData(entity, new DiplomacySimulationState
            {
                LastProcessedDay = uint.MaxValue,
                AverageRelations = 0f,
                DailyTradeProfit = 0f,
                ActiveDeals = 0,
                ActiveAlliances = 0,
                WarsDeclaredTotal = 0
            });

            var factions = state.EntityManager.AddBuffer<FactionProfileEntry>(entity);
            factions.Add(new FactionProfileEntry
            {
                FactionId = 1u,
                Kind = FactionKindId.Player,
                Ideology = FactionIdeologyId.Technocracy,
                Personality = 2,
                MilitaryPower = 120f
            });
            factions.Add(new FactionProfileEntry
            {
                FactionId = 2u,
                Kind = FactionKindId.AiFaction,
                Ideology = FactionIdeologyId.Capitalism,
                Personality = 1,
                MilitaryPower = 95f
            });
            factions.Add(new FactionProfileEntry
            {
                FactionId = 3u,
                Kind = FactionKindId.AiFaction,
                Ideology = FactionIdeologyId.Militarism,
                Personality = 0,
                MilitaryPower = 150f
            });

            var relations = state.EntityManager.AddBuffer<DiplomaticRelationEntry>(entity);
            relations.Add(new DiplomaticRelationEntry
            {
                FactionA = 1u,
                FactionB = 2u,
                RelationScore = 32f,
                BorderTension = 0.3f,
                CommonEnemyBonus = 0.2f
            });
            relations.Add(new DiplomaticRelationEntry
            {
                FactionA = 1u,
                FactionB = 3u,
                RelationScore = -35f,
                BorderTension = 0.8f,
                CommonEnemyBonus = 0f
            });
            relations.Add(new DiplomaticRelationEntry
            {
                FactionA = 2u,
                FactionB = 3u,
                RelationScore = -20f,
                BorderTension = 0.5f,
                CommonEnemyBonus = 0f
            });

            var deals = state.EntityManager.AddBuffer<TradeDealEntry>(entity);
            deals.Add(new TradeDealEntry
            {
                SellerFaction = 1u,
                BuyerFaction = 2u,
                DealKind = TradeDealKindId.Contract,
                GoodsCategory = TradeGoodsCategoryId.Materials,
                BasePrice = 110f,
                DemandSupplyFactor = 1.2f,
                DistanceKm = 180f,
                TraderSkillLevel = 3f,
                VolumePerDay = 12f,
                RemainingDays = 20
            });

            var alliances = state.EntityManager.AddBuffer<DiplomaticAllianceEntry>(entity);
            alliances.Add(new DiplomaticAllianceEntry
            {
                FactionA = 1u,
                FactionB = 2u,
                Kind = DiplomaticAllianceKindId.Trade,
                RequiredRelation = 25f
            });

            state.EntityManager.AddBuffer<ActiveWarEntry>(entity);
        }
    }
}
