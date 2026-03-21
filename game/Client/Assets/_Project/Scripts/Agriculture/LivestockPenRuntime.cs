using Unity.Entities;

namespace ColonyConquest.Agriculture
{
    /// <summary>Загон: вид скота и день последней выдачи продукции §1.4.</summary>
    public struct LivestockPenRuntime : IComponentData
    {
        public LivestockKindId Kind;
        /// <summary><c>uint.MaxValue</c> — ещё не выдавали в этом сохранении.</summary>
        public uint LastYieldDayIndex;
    }
}
