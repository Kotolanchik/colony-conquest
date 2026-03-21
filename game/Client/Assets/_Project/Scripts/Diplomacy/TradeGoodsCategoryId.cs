namespace ColonyConquest.Diplomacy
{
    /// <summary>Категории торговых товаров — <c>spec/diplomacy_trade_spec.md</c> §3.2.</summary>
    public enum TradeGoodsCategoryId : byte
    {
        None = 0,
        Raw = 1,
        Materials = 2,
        Consumer = 3,
        Military = 4,
        Technology = 5,
        Labor = 6,
        Information = 7
    }
}
