using SpawnSystem.Data.Enum;
using UnityEngine;
using UnityEngine.Events;

namespace Extentions.GameSystem
{
    public class GameSignals : MonoSingleton<GameSignals>
    {
        public UnityAction onGame = delegate{};
        public UnityAction CheckAllPlayersReady = delegate { };
    }
}