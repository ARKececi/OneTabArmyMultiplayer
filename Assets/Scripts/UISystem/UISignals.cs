using Extentions;
using UnityEngine;
using UnityEngine.Events;

namespace Signals
{
    public class UISignals : MonoSingleton<UISignals>
    {
        public UnityAction onNextLevel = delegate { };
        public UnityAction<string> onFinal = delegate { };
    }
}