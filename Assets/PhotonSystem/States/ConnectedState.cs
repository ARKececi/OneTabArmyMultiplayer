using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fusion;
using Fusion.Sockets;
using InputSystem;
using PanelSystem;
using PanelSystem.Enums;
using PhotonSystem.Data;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PhotonSystem.States
{
    public class ConnectedState : IPhotonBaseState, INetworkRunnerCallbacks
    {
        #region Self Variables

        #region Private Variables
        
        private NetworkRunner _runner;
        private PanelManager _panel;

        #endregion
        
        #endregion
        public void EnterState(PhotonManager photonManager)
        {
            
            _runner = photonManager.ActiveRunner;
            _panel = photonManager.Panel;
            Connection();
        }

        public void ExitState(PhotonManager photonManager)
        {
            
        }

        private async void Connection()
        {
            _runner.RemoveCallbacks();
            _runner.ProvideInput = true;
            _runner.AddCallbacks(_runner.gameObject.AddComponent<InputManager>());
            _runner.AddCallbacks(this);
            await OperationCall();
            _panel.OpenPanel(PanelType.Connected);
            Debug.Log("bağlandı");
        }
        
        private async Task OperationCall()
        {
            if (_runner.IsRunning)
            {
                Debug.Log("Runner çalışmakta önce sil");
                return;
            }
            _runner.AddCallbacks(this);
            var joinLobbyResult = await _runner.JoinSessionLobby(SessionLobby.Custom, "FusionLobby");
            if (joinLobbyResult.Ok)
            {
                Debug.Log("[Fusion] Oturum lobisine başarıyla katıldın.");
                await Task.Delay(100);
            }
            else
            {
                Debug.LogError("[Fusion] Lobiye katılamadı: " + joinLobbyResult.ErrorMessage);
            }
        }
        
        private async Task LoadSceneAsync()
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("MainScene");

            while (!asyncLoad.isDone)
            {
                Debug.Log("Yükleniyor: " + (asyncLoad.progress * 100) + "%");
                await Task.Yield();
            }
        }
        
        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }

        public async void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
            await LoadSceneAsync();
        }
        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
        public void OnInput(NetworkRunner runner, NetworkInput input) { }
        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
        public void OnConnectedToServer(NetworkRunner runner) { }
        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
        public void OnSceneLoadDone(NetworkRunner runner) { }
        public void OnSceneLoadStart(NetworkRunner runner){ }
    }
}