using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Data.ValueObject
{
     [Serializable]
    public class CardData
    {
        public List<GameObject> List = new(); // Her kart türü için farklı seviyelerde kartları tutan sözlük
    }
}