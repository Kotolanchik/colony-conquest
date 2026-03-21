using ColonyConquest.Netcode;
using Unity.Entities;
using Unity.NetCode;

namespace ColonyConquest.Core
{
    /// <summary>Отражает факт «в игре» по Netcode в синглтоне спайка (телеметрия/отладка).</summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct NetcodeSpikeStateSyncSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            if (!SystemAPI.HasSingleton<NetcodeSpikeState>())
                return;

            foreach (var _ in SystemAPI.Query<RefRO<NetworkStreamInGame>>())
            {
                ref var spike = ref SystemAPI.GetSingletonRW<NetcodeSpikeState>().ValueRW;
                spike.TransportReady = 1;
                return;
            }
        }
    }
}
