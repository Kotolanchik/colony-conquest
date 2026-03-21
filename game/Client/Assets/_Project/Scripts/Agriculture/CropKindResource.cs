using ColonyConquest.Economy;

namespace ColonyConquest.Agriculture
{
    /// <summary>Соответствие культуры §1.1 и ресурса склада.</summary>
    public static class CropKindResource
    {
        public static bool TryGetHarvestResource(CropKindId crop, out ResourceId resource)
        {
            switch (crop)
            {
                case CropKindId.Wheat: resource = ResourceId.CropWheat; return true;
                case CropKindId.Barley: resource = ResourceId.CropBarley; return true;
                case CropKindId.Oat: resource = ResourceId.CropOat; return true;
                case CropKindId.Rye: resource = ResourceId.CropRye; return true;
                case CropKindId.Corn: resource = ResourceId.CropCorn; return true;
                case CropKindId.Potato: resource = ResourceId.CropPotato; return true;
                case CropKindId.Vegetables: resource = ResourceId.CropVegetables; return true;
                case CropKindId.Fruits: resource = ResourceId.CropFruits; return true;
                default:
                    resource = ResourceId.None;
                    return false;
            }
        }
    }
}
