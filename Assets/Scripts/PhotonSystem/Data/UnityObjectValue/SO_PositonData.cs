using System.Collections.Generic;
using Fusion;
using UnityEngine;
using UnityEngine.Serialization;

namespace PhotonSystem.Data.UnityObjectValue
{
    [CreateAssetMenu(fileName = "SO_PositonData", menuName = "LobbyData/SO_PositonData", order = 0)]
    public class SO_PositonData : ScriptableObject
    {
        public List<TransformData> PlayerSpawn;
    }
}