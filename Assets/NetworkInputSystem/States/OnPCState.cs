using System.Collections.Generic;
using InputSystem.Enum;
using UnityEngine;

namespace InputSystem
{
    public class OnPCState : IInputBaseState
    {
        #region Self Variables

        #region Private Variables

        private Dictionary<KeyCode, KeybordInputEnum> keyMappings = new Dictionary<KeyCode, KeybordInputEnum>();
        private Dictionary<KeybordInputEnum, bool> keybordInput = new Dictionary<KeybordInputEnum, bool>();
        private (bool, bool) mouseInput;
        private Vector2 mouseMove;

        #endregion

        #endregion
        
        public void EnterState(InputManager ınputManager)
        {
            keyMappings = new Dictionary<KeyCode, KeybordInputEnum>
            {
                { KeyCode.W, KeybordInputEnum.Move },
                { KeyCode.A, KeybordInputEnum.Left },
                { KeyCode.S, KeybordInputEnum.Back },
                { KeyCode.D, KeybordInputEnum.Right },
                { KeyCode.Space, KeybordInputEnum.Jump }
            };
            foreach (KeybordInputEnum VARIABLE in System.Enum.GetValues(typeof(KeybordInputEnum)))
            {
                keybordInput.Add(VARIABLE,false);
            }
        }

        public void UpdateState(InputManager inputManager)
        {
            // mouse'un sol tıklamaları için yazılmış script
            switch (GetMouseLeftInputState())
            {
                case MouseState.Down:
                    // Sol tık basıldı (ilk an)
                    mouseInput.Item1 = true;
                    break;
                case MouseState.Held:
                    // Sol tık basılı tutuluyor
                    break;
                case MouseState.Released:
                    // Sol tık bırakıldı
                    mouseInput.Item1 = false;
                    break;
            }
            
            // mouse'un sağ tıklamaları için yazılmış script
            switch (GetMouseRightInputState())
            {
                case MouseState.Down:
                    // Sağ tık basıldı (ilk an)
                    mouseInput.Item2 = true;
                    break;
                case MouseState.Held:
                    // Sağ tık basılı tutuluyor
                    break;
                case MouseState.Released:
                    // Sağ tık bırakıldı
                    mouseInput.Item2 = false;
                    break;
            }
            
            // mouse'un hareketleri için yazılmış script
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");
            mouseMove = new Vector2(mouseX, mouseY);
               
            // Klavyeden gelen girdilerin kontrol edildiği yer
            foreach (var key in keyMappings)
            {
                keybordInput[key.Value] = Input.GetKey(key.Key);
            }
            
            InputSignals.Instance.OnPCParams?.Invoke(new PCInputParams()
            {
                KeybordInputMaplist = keybordInput,
                MouseInput = mouseInput,
                MouseMove = mouseMove
            });
        }
        
        // Mouse giriş durumunu döndüren yardımcı fonksiyon
        MouseState GetMouseLeftInputState()
        {
            if (Input.GetMouseButtonDown(0)) return MouseState.Down;
            if (Input.GetMouseButton(0))     return MouseState.Held;
            if (Input.GetMouseButtonUp(0))   return MouseState.Released;
    
            return MouseState.None;
        }
        
        MouseState GetMouseRightInputState()
        {
            if (Input.GetMouseButtonDown(1)) return MouseState.Down;
            if (Input.GetMouseButton(1))     return MouseState.Held;
            if (Input.GetMouseButtonUp(1))   return MouseState.Released;
    
            return MouseState.None;
        }
    }
}