using Unity.Collections;
using Unity.Entities;

namespace ColonyConquest.Diplomacy
{
    /// <summary>Маркер сущности с буферами дипломатии (отношения, сделки, союзы, профили фракций).</summary>
    public struct DiplomacySimulationSingleton : IComponentData
    {
    }

    /// <summary>Сводное состояние дипломатии и торговли.</summary>
    public struct DiplomacySimulationState : IComponentData
    {
        public uint LastProcessedDay;
        public float AverageRelations;
        public float DailyTradeProfit;
        public ushort ActiveDeals;
        public ushort ActiveAlliances;
        public ushort WarsDeclaredTotal;
    }

    /// <summary>Профиль фракции (тип, идеология и поведение ИИ).</summary>
    public struct FactionProfileEntry : IBufferElementData
    {
        public uint FactionId;
        public FactionKindId Kind;
        public FactionIdeologyId Ideology;
        public byte Personality; // 0 агрессор, 1 торговец, 2 дипломат, 3 изоляционист, 4 империалист, 5 технократ
        public float MilitaryPower;
    }

    /// <summary>Отношения между двумя фракциями на шкале -100..100.</summary>
    public struct DiplomaticRelationEntry : IBufferElementData
    {
        public uint FactionA;
        public uint FactionB;
        public float RelationScore;
        public float BorderTension;
        public float CommonEnemyBonus;
    }

    /// <summary>Активная торговая сделка между фракциями.</summary>
    public struct TradeDealEntry : IBufferElementData
    {
        public uint SellerFaction;
        public uint BuyerFaction;
        public TradeDealKindId DealKind;
        public TradeGoodsCategoryId GoodsCategory;
        public float BasePrice;
        public float DemandSupplyFactor;
        public float DistanceKm;
        public float TraderSkillLevel;
        public float VolumePerDay;
        public short RemainingDays;
    }

    /// <summary>Запись о действующем союзе.</summary>
    public struct DiplomaticAllianceEntry : IBufferElementData
    {
        public uint FactionA;
        public uint FactionB;
        public DiplomaticAllianceKindId Kind;
        public float RequiredRelation;
    }

    /// <summary>Активный военный конфликт между фракциями.</summary>
    public struct ActiveWarEntry : IBufferElementData
    {
        public uint AttackerFaction;
        public uint DefenderFaction;
        public byte WarGoal; // 0 territory, 1 reparations, 2 disarm, 3 vassalization
        public ushort RemainingDays;
    }
}
