using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fusion;
using Fusion.Sockets;
using PanelSystem;
using PanelSystem.Enums;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace PhotonSystem.States
{
    public class ListLobby: IPhotonBaseState, INetworkRunnerCallbacks
    {
        #region Self Variables

        #region Private Variables

        private NetworkRunner _runner;
        private List<SessionInfo> _sessionList;
        private PhotonManager _photonManager;
        private PanelManager.PanelManager _panel;
        
        private Transform _lobbyListContent;
        private Button _joinLobbyButtonPrefab;
        
        #endregion

        #endregion
        public async void EnterState(PhotonManager photonManager)
        {
            Reset();
            _photonManager = photonManager;
            _lobbyListContent = photonManager.Data.LobbyListContent;
            _joinLobbyButtonPrefab = photonManager.Data.JoinLobbyButtonPrefab;  
            _panel = photonManager.Panel;
            InstanceRunner();
            await OperationCall();
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
        
        private void Reset()
        {
            if (_runner != null)
            {
                if (_runner.IsRunning)
                {
                    _runner.Shutdown(); // bu async değilse beklemene gerek yok
                }
                Object.Destroy(_runner.gameObject);
                _runner = null;
                Debug.Log("[Fusion] Runner temizlendi.");
            }
        }
        
        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
        {
            _panel.OpenPanel(PanelType.ListMenu);
            _photonManager.SessionList = sessionList; // listeler merkezde tutuluyor join yapıldığnda kullanılacak
            // UI'de eski lobileri temizle
            foreach (Transform child in _lobbyListContent)
            {
                Object.Destroy(child.gameObject);
            }

            // Yeni lobileri ekleyelim
            foreach (var session in sessionList)
            {
                Debug.Log(session);
                Button button = Object.Instantiate(_joinLobbyButtonPrefab, _lobbyListContent);
                button.GetComponentInChildren<TextMeshProUGUI>().text = session.Name;
                button.onClick.AddListener(() =>
                {
                    InstanceRunner();
                    _photonManager.SessionName = session.Name;
                    _photonManager.ActiveRunner = _runner; // runner merkezde tutuluyor join yapıldığnda kullanılacak
                    _photonManager.JoinButton();
                });
            }
            // Lobi listesi ekranını aç
        }
        
        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
        {
            if (reason == NetConnectFailedReason.ServerRefused)
            {
                Debug.Log("Liste içerisinden Bağlantı reddedildi: Yanlış şifre.");
                _panel.OpenPanel(PanelType.ListMenu);
            }
        }
        
        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
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