namespace ColonyConquest.Diplomacy
{
    /// <summary>Типы фракций — <c>spec/diplomacy_trade_spec.md</c> §1.2.</summary>
    public enum FactionKindId : byte
    {
        Player = 0,
        AiFaction = 1,
        Neutral = 2,
        Wild = 3,
        Ancient = 4
    }
}
