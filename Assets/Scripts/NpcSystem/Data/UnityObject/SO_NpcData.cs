using BotSystem.Data.ValueObject;
using Fusion;
using SpawnSystem;
using SpawnSystem.Data.Enum;
using UnityEngine;
using UnityEngine.Serialization;

namespace BotSystem.Data.UnityObject
{
    [CreateAssetMenu(fileName = "SO_NpcData", menuName = "Data/SO_NpcData", order = 0)]
    public class SO_NpcData : ScriptableObject
    {
        public SerializableDictionary<NPCEnum, NpcData> NpcData;
    }
}