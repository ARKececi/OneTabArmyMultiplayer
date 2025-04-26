using DesignPattern;
using InputSystem.Enum;
using UnityEngine;
using UnityEngine.Events;

namespace InputSystem
{
    public class InputSignals : MonoSingleton<InputSignals>
    {
        public UnityAction<InputStateEnum> InputSwichState;
        public UnityAction<Vector2> OnMobileParams;
        public UnityAction<PCInputParams> OnPCParams;
    }
}