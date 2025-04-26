using System;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

namespace SpawnSystem
{

    [Serializable]
    public struct NPCPrefabData
    {
        public SerializableDictionary<NPCPrefabEnum, NetworkPrefabRef> NPCPrefabs;
    }
}