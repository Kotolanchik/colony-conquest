using Unity.Entities;

namespace ColonyConquest.Netcode
{
    /// <summary>
    /// Состояние спайка сети: 0 — транспорт не подключён (пакет Netcode for Entities — отдельная итерация).
    /// </summary>
    public struct NetcodeSpikeState : IComponentData
    {
        public byte TransportReady;
    }
}
