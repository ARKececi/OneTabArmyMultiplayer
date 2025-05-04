using Fusion;
using PlayerSystem.Data.UnityObject;
using UnityEngine;

namespace PlayerSystem.Data.ValueObject
{
    [CreateAssetMenu(fileName = "SO_TowerLwlData", menuName = "Data/SO_TowerLwlData", order = 0)]
    public class SO_TowerLwlData : ScriptableObject
    {
        public SerializableDictionary<int, TowerLwlData> lwls;
    }
}