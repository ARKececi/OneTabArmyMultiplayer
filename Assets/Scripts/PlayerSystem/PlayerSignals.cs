using Extentions;
using Fusion;
using UnityEngine.Events;

namespace PlayerSystem
{
    public class PlayerSignals : MonoSingleton<PlayerSignals>
    {
        public UnityAction<CardType,int> onSpawnEnum = delegate{};
        public UnityAction<NetworkObject, int> onExp = delegate { };
        public UnityAction onDisconnect = delegate { };
        public UnityAction<PlayerRef, float, float> SpawnBar = delegate { };
        public UnityAction<bool> onTower = delegate { };
        public UnityAction onSetReady = delegate { };
    }
}