using System;
using Data.ValueObject;
using UnityEngine;
using UnityEngine.Rendering;

namespace Data.UnityObject
{
    [Serializable]
    [CreateAssetMenu(fileName = "CD_Card", menuName = "Data/CD_Card", order = 0)]
    public class CD_Card : ScriptableObject
    {
        public SerializedDictionary<CardType, CardData> CardData;
    }
}