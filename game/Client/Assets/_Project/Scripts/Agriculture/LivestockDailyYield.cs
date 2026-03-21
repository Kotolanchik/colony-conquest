using ColonyConquest.Economy;

namespace ColonyConquest.Agriculture
{
    /// <summary>Суточный выход §1.4 (единицы на игровой день, прототип).</summary>
    public static class LivestockDailyYield
    {
        public static bool TryGetPrimaryResource(LivestockKindId kind, out ResourceId res, out float amountPerDay)
        {
            switch (kind)
            {
                case LivestockKindId.Chickens:
                    res = ResourceId.LivestockEggs;
                    amountPerDay = 6f;
                    return true;
                case LivestockKindId.Goats:
                    res = ResourceId.LivestockMilk;
                    amountPerDay = 4f;
                    return true;
                case LivestockKindId.Sheep:
                    res = ResourceId.LivestockWool;
                    amountPerDay = 3f;
                    return true;
                case LivestockKindId.Pigs:
                    res = ResourceId.LivestockMeat;
                    amountPerDay = 2f;
                    return true;
                case LivestockKindId.Cows:
                    res = ResourceId.LivestockMilk;
                    amountPerDay = 8f;
                    return true;
                case LivestockKindId.Horses:
                    res = ResourceId.LivestockMeat;
                    amountPerDay = 0.5f;
                    return true;
                case LivestockKindId.Camels:
                    res = ResourceId.LivestockMeat;
                    amountPerDay = 0.5f;
                    return true;
                case LivestockKindId.FactoryLivestock:
                    res = ResourceId.LivestockMeat;
                    amountPerDay = 40f;
                    return true;
                default:
                    res = ResourceId.None;
                    amountPerDay = 0f;
                    return false;
            }
        }
    }
}
