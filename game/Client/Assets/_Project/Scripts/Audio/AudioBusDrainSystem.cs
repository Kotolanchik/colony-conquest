using ColonyConquest.Core;
using Unity.Entities;

namespace ColonyConquest.Audio
{
    /// <summary>Сбрасывает очередь SFX (заглушка «потребления» шины до FMOD/Wwise).</summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(ConstructionGhostCursorSystem))]
    public partial struct AudioBusDrainSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            // Full runtime аудио сам ingest-ит AudioBus; drain оставляем только как legacy fallback.
            if (SystemAPI.HasSingleton<AudioSimulationSingleton>())
                return;

            var buffer = SystemAPI.GetSingletonBuffer<AudioBusPendingEntry>();
            if (buffer.Length > 0)
                buffer.Clear();
        }
    }
}
