using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using InputSystem.Enum;
using InputSystem.Params;
using InputSystem.States;
using UnityEngine;

namespace InputSystem
{
    public class InputManager : SimulationBehaviour,INetworkRunnerCallbacks
    {
        #region Self Variables

        #region Public Variables

        public NetInput Input;

        #endregion

        #region Serialized Variables

        [SerializeField] private InputStateEnum _selectedState; // Sadece tek bir değer saklanacak!

        #endregion

        #region State Variables
        
        private MobileState               mobileState;
        private OnPCState                 onPCState;
        private AnyState                  anyState;
        private IInputBaseState           curretnBaseState;

        private bool inputReset;

        #endregion

        #endregion

        #region Subscribe

        private void OnEnable()
        {
            InputSignals.Instance.InputSwichState += SwichState;
        }

        private void OnDisable()
        {
            InputSignals.Instance.InputSwichState -= SwichState;
        }

        #endregion

        private void Awake()
        {
            mobileState               = new MobileState();
            onPCState                 = new OnPCState();
            anyState                  = new AnyState();     
            curretnBaseState          = anyState;
            SwichState(_selectedState);
        }

        public void Update()
        {
            if (inputReset)
            {
                Input = default;
                inputReset = false;
            }
            curretnBaseState.UpdateState(this);
        }

        private void SwichState(InputStateEnum ınputStateEnum)
        {
            curretnBaseState = ınputStateEnum switch
            {
                InputStateEnum.OnMobileStrategyState   => mobileState,
                InputStateEnum.OnPcfpsState => onPCState,
                InputStateEnum.AnyState     => anyState,
                _ => curretnBaseState
            };
            curretnBaseState.EnterState(this);
        }
    
        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
        {
        }

        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
        {
        }

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
        }

        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
            input.Set(Input);
            inputReset = true;
        }

        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
        {
        }

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
        }

        public void OnConnectedToServer(NetworkRunner runner)
        {
        }

        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
        {
        }

        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
        {
        }

        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
        {
        }

        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
        {
        }

        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
        {
        }

        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
        {
        }

        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
        {
        }

        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
        {
        }

        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
        {
        }

        public void OnSceneLoadDone(NetworkRunner runner)
        {
        }

        public void OnSceneLoadStart(NetworkRunner runner)
        {
        }
    }
}
