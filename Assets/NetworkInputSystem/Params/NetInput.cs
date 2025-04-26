using Fusion;
using Vector3 = UnityEngine.Vector3;

namespace InputSystem.Params
{
    public struct NetInput : INetworkInput
    {
        public bool İsClick;
        public Vector3 Input;
    }
}