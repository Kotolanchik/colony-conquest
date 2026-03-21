using Unity.Entities;

namespace ColonyConquest.Agriculture
{
    /// <summary>Состояние грядки: этапы §1.2 и множители §1.3.</summary>
    public struct CropPlotRuntime : IComponentData
    {
        public CropKindId Crop;
        public CropGrowthPhase Phase;
        /// <summary>Тик начала текущего этапа (глобальный счётчик симуляции).</summary>
        public ulong PhaseStartTick;

        /// <summary>Плодородие как множитель 0.5…2.0 (§1.2).</summary>
        public float SoilFertility;
        public FertilizerKindId ActiveFertilizer;
        /// <summary>Потери от вредителей/болезней 0…0.5 (§1.2).</summary>
        public float PestDamage;
        /// <summary>Потери от сорняков 0…0.5; гербициды снижают ежедневно в системе ухода.</summary>
        public float WeedPressure01;
        public float FarmerSkillLevel;
        public float WeatherModifier;
        public WaterSupplyKind WaterSupply;
        /// <summary>Последний игровой день, в котором применялся ежедневный уход (§1.2 «ежедневно» в фазе роста).</summary>
        public uint LastCareGameDayIndex;

        /// <summary>Индекс игрового года (<c>DayIndex / DaysPerGameYear</c>), после которого уже применялось годовое плодородие §2.3.</summary>
        public uint LastSoilAnnualGameYearIndex;
    }
}
