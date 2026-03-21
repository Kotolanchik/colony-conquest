namespace ColonyConquest.Diplomacy
{
    /// <summary>Типы союзов — <c>spec/diplomacy_trade_spec.md</c> §3.3.</summary>
    public enum DiplomaticAllianceKindId : byte
    {
        None = 0,
        Trade = 1,
        Offensive = 2,
        Defensive = 3,
        Full = 4,
        Federation = 5
    }
}
