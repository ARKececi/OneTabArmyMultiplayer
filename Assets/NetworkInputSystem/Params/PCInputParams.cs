using System.Collections.Generic;
using InputSystem.Enum;
using UnityEngine;

namespace InputSystem
{
    public class PCInputParams
    {
        public (bool, bool) MouseInput;
        public Vector2 MouseMove;
        public Dictionary<KeybordInputEnum,bool> KeybordInputMaplist;
    }
}