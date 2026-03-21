using Unity.Entities;
using UnityEngine;

namespace ColonyConquest.Agriculture
{
    /// <summary>Грядка в SubScene: после baking — <see cref="CropPlotTag"/> + <see cref="CropPlotRuntime"/>.</summary>
    public class CropPlotAuthoring : MonoBehaviour
    {
        public CropKindId Crop = CropKindId.Wheat;
        public WaterSupplyKind WaterSupply = WaterSupplyKind.Normal;
        public FertilizerKindId ActiveFertilizer = FertilizerKindId.Manure;
        [Range(0.5f, 2f)] public float SoilFertility = 1f;
    }

    public sealed class CropPlotBaker : Baker<CropPlotAuthoring>
    {
        public override void Bake(CropPlotAuthoring a)
        {
            var e = GetEntity(a, TransformUsageFlags.None);
            AddComponent<CropPlotTag>(e);
            AddComponent(e, new CropPlotRuntime
            {
                Crop = a.Crop,
                Phase = CropGrowthPhase.Preparation,
                PhaseStartTick = 0,
                SoilFertility = a.SoilFertility,
                ActiveFertilizer = a.ActiveFertilizer,
                PestDamage = 0.05f,
                WeedPressure01 = 0.08f,
                FarmerSkillLevel = 2f,
                WeatherModifier = 1f,
                WaterSupply = a.WaterSupply,
                LastCareGameDayIndex = uint.MaxValue
            });
        }
    }
}
