namespace InputSystem
{
    public interface IInputBaseState
    {
        public void EnterState(InputManager ınputManager);
        public void UpdateState(InputManager inputManager);
    }
}