namespace ColonyConquest.Diplomacy
{
    /// <summary>Типы торговых сделок — <c>spec/diplomacy_trade_spec.md</c> §3.2.</summary>
    public enum TradeDealKindId : byte
    {
        None = 0,
        Instant = 1,
        Contract = 2,
        LongTerm = 3,
        Barter = 4
    }
}
