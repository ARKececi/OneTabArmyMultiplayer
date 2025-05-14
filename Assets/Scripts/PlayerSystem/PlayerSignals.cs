using Extentions;
using SpawnSystem.Data.Enum;
using UnityEngine;
using UnityEngine.Events;

namespace PlayerSystem
{
    public class PlayerSignals : MonoSingleton<PlayerSignals>
    {
        public UnityAction onGame = delegate{};
        public UnityAction<CardType,int> onSpawnEnum = delegate{};
    }
}