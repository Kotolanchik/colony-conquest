using ColonyConquest.Economy;

namespace ColonyConquest.Agriculture
{
    /// <summary>
    /// Основной выход месторождения в экономический <see cref="ResourceId"/> — см. <c>spec/agriculture_mining_spec.md</c> §2.1 и §1.1 экономики.
    /// Типы без однозначного <see cref="ResourceId"/> в текущем перечне возвращают <c>false</c>.
    /// </summary>
    public static class MiningDepositPrimaryResource
    {
        public static bool TryGetPrimaryResource(MiningDepositKindId kind, out ResourceId resource)
        {
            switch (kind)
            {
                case MiningDepositKindId.Forest:
                    resource = ResourceId.Wood;
                    return true;
                case MiningDepositKindId.StoneQuarry:
                    resource = ResourceId.Stone;
                    return true;
                case MiningDepositKindId.Clay:
                    resource = ResourceId.RawClay;
                    return true;
                case MiningDepositKindId.Sand:
                    resource = ResourceId.Sand;
                    return true;
                case MiningDepositKindId.SilverOre:
                    resource = ResourceId.SilverOre;
                    return true;
                case MiningDepositKindId.Diamonds:
                    resource = ResourceId.RawDiamonds;
                    return true;
                case MiningDepositKindId.Helium3:
                    resource = ResourceId.Helium3;
                    return true;
                case MiningDepositKindId.IronOre:
                    resource = ResourceId.IronOre;
                    return true;
                case MiningDepositKindId.Coal:
                    resource = ResourceId.Coal;
                    return true;
                case MiningDepositKindId.CopperOre:
                    resource = ResourceId.CopperOre;
                    return true;
                case MiningDepositKindId.TinOre:
                    resource = ResourceId.TinOre;
                    return true;
                case MiningDepositKindId.LeadOre:
                    resource = ResourceId.LeadOre;
                    return true;
                case MiningDepositKindId.GoldOre:
                    resource = ResourceId.GoldOre;
                    return true;
                case MiningDepositKindId.Oil:
                    resource = ResourceId.Oil;
                    return true;
                case MiningDepositKindId.Bauxite:
                    resource = ResourceId.Aluminum;
                    return true;
                case MiningDepositKindId.Nickel:
                    resource = ResourceId.NickelOre;
                    return true;
                case MiningDepositKindId.Chromium:
                    resource = ResourceId.ChromiteOre;
                    return true;
                case MiningDepositKindId.Tungsten:
                    resource = ResourceId.TungstenOre;
                    return true;
                case MiningDepositKindId.Uranium:
                    resource = ResourceId.UraniumOre;
                    return true;
                case MiningDepositKindId.Platinum:
                    resource = ResourceId.PlatinumGroup;
                    return true;
                case MiningDepositKindId.RareEarths:
                    resource = ResourceId.RareEarthMetals;
                    return true;
                case MiningDepositKindId.AntimatterLab:
                    resource = ResourceId.Antimatter;
                    return true;
                default:
                    resource = ResourceId.None;
                    return false;
            }
        }
    }
}
