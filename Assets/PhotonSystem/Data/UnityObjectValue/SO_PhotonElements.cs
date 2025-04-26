using System;
using UnityEngine;

namespace PhotonSystem.Data
{
    [CreateAssetMenu(fileName = "Data", menuName = "Data/SO_PhotonElements", order = 0)]
    [Serializable]
    public class SO_PhotonElements : ScriptableObject
    {
        public PhotonElements PhotonElements;
    }
}