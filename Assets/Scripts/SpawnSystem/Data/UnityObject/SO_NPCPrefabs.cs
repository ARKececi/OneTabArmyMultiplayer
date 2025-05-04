using System.Collections.Generic;
using Fusion;
using UnityEngine;

namespace SpawnSystem
{
    [CreateAssetMenu(fileName = "SO_NPCPrefabs", menuName = "Data/SO_NPCPrefabs", order = 0)]
    public class SO_NPCPrefabs : ScriptableObject
    {
        public NPCPrefabData NPCData;
    }
}