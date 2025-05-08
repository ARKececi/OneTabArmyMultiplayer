using System;
using System.Collections.Generic;
using Fusion;
using SpawnSystem.Data.Enum;
using UnityEngine;

namespace SpawnSystem
{
    [Serializable]
    public struct NPCPrefabData
    {
        public SerializableDictionary<NPCEnum, NetworkPrefabRef> NPCPrefabs;
    }
}