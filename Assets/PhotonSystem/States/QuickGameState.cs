using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fusion;
using Fusion.Sockets;
using PanelSystem;
using PanelSystem.Enums;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PhotonSystem.States
{
    public class QuickGameState: IPhotonBaseState, INetworkRunnerCallbacks
    {
        #region Self Variables

        #region Private Variables

        private NetworkRunner _runner;
        private List<SessionInfo> _sessionList;
        private PhotonManager _photonManager;
        private PanelManager _panel;

        #endregion

        #endregion
        public void EnterState(PhotonManager photonManager)
        {
            _photonManager = photonManager;
            _panel = photonManager.Panel;
            InstanceRunner();
            OperationCall();
 
        }

        public void ExitState(PhotonManager photonManager)
        {
            
        }

        private void InstanceRunner()
        {
            if (_runner != null)
            {
                Debug.LogWarning("Runner zaten mevcut, tekrar oluşturulmayacak.");
                return;
            }
            var dinamicRunnerObject = new GameObject("Dinamic Runner");
            dinamicRunnerObject.transform.SetParent(_photonManager.gameObject.transform);
            _runner = dinamicRunnerObject.AddComponent<NetworkRunner>();
            
        }
        
        private async Task OperationCall()
        {
            if (_runner.IsRunning)
            {
                Debug.Log("Runner çalışmakta önce sil");
                return;
            }
            _panel.OpenPanel(PanelType.Loading);
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
        
        // private async void JoinGame(string sessionName)
        // {
        //     var result = await _runner.StartGame(new StartGameArgs()
        //     {
        //         GameMode = GameMode.Client,
        //         SessionName = sessionName,
        //     });
        //
        //     if (result.Ok)
        //     {
        //         Debug.Log($"[Fusion] {sessionName} adlı lobiye bağlandın!");
        //     }
        //     else
        //     {
        //         Debug.LogError($"[Fusion] Lobiye bağlanma başarısız: {result.ErrorMessage}");
        //     }
        // }

        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
        {
            if (sessionList.Count == 0)
            {
                Debug.LogWarning("[Fusion] Açık oturum yok.");
                return;
            }
            _sessionList = new List<SessionInfo>(sessionList);
            foreach (var VARIABLE in sessionList)
            {
                if (VARIABLE.Properties != null && VARIABLE.Properties.TryGetValue("IsPasswordProtected", out var prop))
                {
                    _sessionList.Remove(VARIABLE);
                }
            }
            Debug.Log(_sessionList.Count);
            _photonManager.SessionList = _sessionList;
            var randomIndex = Random.Range(0, _sessionList.Count-1);
            
            _photonManager.SessionName = _sessionList[randomIndex].Name;
            _photonManager.ActiveRunner = _runner; // runner merkezde tutuluyor join yapıldığnda kullanılacak
            _photonManager.JoinButton();
        }

        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
        public void OnInput(NetworkRunner runner, NetworkInput input) { }
        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
        public void OnConnectedToServer(NetworkRunner runner) { }
        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
        public void OnSceneLoadDone(NetworkRunner runner) { }
        public void OnSceneLoadStart(NetworkRunner runner){ }
    }
}