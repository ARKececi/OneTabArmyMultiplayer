using System;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

namespace BotSystem.Data.ValueObject
{
    [Serializable]
    public class NpcLevelObject
    {
        public SerializableDictionary<int, NpcSpawnObjectList> LwlNpc;
    }
    [Serializable]
    public class NpcSpawnObjectList
    {
        public List<NpcSpawnObject> SpawnNpc;
    }

    [Serializable]
    public class NpcSpawnObject
    {
        public NetworkObject BodySpawnPoint;
        public NetworkObject SpawnObject;
    }
}