using InputSystem.Enum;
using InputSystem.Params;
using UnityEngine;

namespace InputSystem.States
{
    public class MobileState : IInputBaseState
    {

        #region Self Variables

        #region Private Variables

        private NetInput ınputParams;
        private float defaultTimer = .5f;
        private float timer;
        private bool falseInput;

        #endregion

        #endregion
        
        public void EnterState(InputManager ınputManager)
        {
            timer = defaultTimer;
        }

        public void UpdateState(InputManager inputManager)
        {
            switch (GetMouseInputState())
            {
                case MouseState.Down:
                    // Sol tık basıldı (ilk an)
                    falseInput = false;
                    break;
                case MouseState.Held:
                    // Sol tık basılı tutuluyor
                    Timer();
                    break;
                case MouseState.Released:
                    // Sol tık bırakıldı
                    timer = defaultTimer;
                    if (falseInput) break;
                    inputManager.Input.Input = Input.mousePosition;
                    inputManager.Input.İsClick = true;
                    break;
            }
        }
        
        // Mouse giriş durumunu döndüren yardımcı fonksiyon
        MouseState GetMouseInputState()
        {
            if (Input.GetMouseButtonDown(0)) return MouseState.Down;
            if (Input.GetMouseButton(0)) return MouseState.Held;
            if (Input.GetMouseButtonUp(0)) return MouseState.Released;
    
            return MouseState.None;
        }

        private void Timer()
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                falseInput = true;
            }
        }
    }
}