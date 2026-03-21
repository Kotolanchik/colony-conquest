namespace ColonyConquest.Economy
{
    /// <summary>Снимок строки §1.2: базовая цена и метаданные для ECS/UI/экономики.</summary>
    public readonly struct ResourceDefinition
    {
        public readonly ResourceId Id;
        public readonly ResourceCategory Category;
        public readonly ResourceRarity Rarity;
        public readonly GameEpoch IntroducedInEpoch;
        public readonly ushort BasePrice;

        public ResourceDefinition(
            ResourceId id,
            ResourceCategory category,
            ResourceRarity rarity,
            GameEpoch introducedInEpoch,
            ushort basePrice)
        {
            Id = id;
            Category = category;
            Rarity = rarity;
            IntroducedInEpoch = introducedInEpoch;
            BasePrice = basePrice;
        }
    }
}
