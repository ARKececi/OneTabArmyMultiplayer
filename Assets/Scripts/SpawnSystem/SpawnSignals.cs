using System;
using BotSystem;
using DesignPattern;
using Fusion;
using UnityEngine;
using UnityEngine.Events;

namespace SpawnSystem
{
    public class SpawnSignals : MonoSingleton<SpawnSignals>
    {
        public Func<SignalsData,NetworkObject> OnSpawn;
    }
}