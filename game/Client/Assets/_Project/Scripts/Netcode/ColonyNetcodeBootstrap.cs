using Unity.NetCode;
using UnityEngine.Scripting;

namespace ColonyConquest.Core
{
    /// <summary>
    /// Точка входа Netcode for Entities: клиент и сервер, авто-подключение на локальный порт (см. задачу tech-netcode-spike).
    /// </summary>
    [Preserve]
    public class ColonyNetcodeBootstrap : ClientServerBootstrap
    {
        public override bool Initialize(string defaultWorldName)
        {
            AutoConnectPort = 7979;
            return base.Initialize(defaultWorldName);
        }
    }
}
