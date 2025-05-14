using Extentions;
using UnityEngine;
using UnityEngine.Events;

namespace Signals
{
    public class CardSignals : MonoSingleton<CardSignals>
    {
        public UnityAction onNextLevel = delegate { };
    }
}