using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fusion;
using Fusion.Sockets;
using InputSystem;
using PanelSystem;
using PanelSystem.Enums;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace PhotonSystem.States
{
    public class JoinLobbyState: IPhotonBaseState, INetworkRunnerCallbacks
    {
        #region Self Variables

        #region Private Variables
        
        private NetworkRunner _runner;
        private List<SessionInfo> _sessionList;
        private PhotonManager _photonManager;
        private PanelManager.PanelManager _panel;
        private SessionInfo _session;
        
        private TMP_InputField _joinLobbyName;
        private TMP_InputField _joinLobbyPassword;
        private Button _passwordButton;
        private Button _joinButton;
        private PanelManager.PanelManager _panelManager; // buna bak
        
        #endregion

        #endregion
        public void EnterState(PhotonManager photonManager)
        {
            _photonManager = photonManager;
            _panel = photonManager.Panel;
            
            _joinLobbyName = photonManager.Data.JoinLobbyName;
            _joinLobbyPassword = photonManager.Data.JoinLobbyPassword;
            _passwordButton = photonManager.Data.PasswordButton;
            _joinButton = photonManager.Data.JoinButton;
            Reset();

            if (photonManager.ActiveRunner != null)
            {
                _runner = photonManager.ActiveRunner;
                _sessionList = photonManager.SessionList;
                TryJoinByNameWithPassword();
            }
            else _panel.OpenPanel(PanelType.JoinMenu);
            
            _joinButton.onClick.AddListener(TryJoinByNameWithPassword);
            
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
            _runner.gameObject.AddComponent<InputManager>();
        }

        private async Task OperationCall()
        {
            Debug.Log("burada");
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
        
        /// <summary>
        /// Fusion üzerinden oyuna katılınır.
        /// </summary>
        private async void JoinGame(string sessionName)
        {
            _panel.OpenPanel(PanelType.Loading);

            if (_joinLobbyPassword.text == "")
                _joinLobbyPassword.text = "NP";
            
            byte[] token = System.Text.Encoding.UTF8.GetBytes(_joinLobbyPassword.text);
            var result = await _runner.StartGame(new StartGameArgs()
            {
                GameMode = GameMode.Client,
                SessionName = sessionName,
                ConnectionToken = token,
            });
            if (result.Ok)
            {
                _photonManager.ActiveRunner= _runner;
                Debug.Log($"[Fusion] {sessionName} adlı lobiye bağlandın!");
                _photonManager.ConnetedLobby();
            }
            else
            {
                Debug.Log($"[Fusion] Lobiye bağlanma başarısız: {result.ErrorMessage}");
            }
        }

        public async void TryJoinByNameWithPassword()
        {
            if (_photonManager.ActiveRunner == null)
            {
                InstanceRunner();
                await OperationCall();
            }
            _panel.OpenPanel(PanelType.Any);
            if (_sessionList == null)
            {
                _panel.OpenPanel(PanelType.MainMenu);
                Debug.Log("Session listesi oluşturulmadı.");
                await _runner.Shutdown();
                return; 
            }
            Debug.Log(_photonManager.SessionName);
            _session = _photonManager.ActiveRunner == null ? _sessionList.Find(x => x.Name == _joinLobbyName.text) : _sessionList.Find(x => x.Name == _photonManager.SessionName);

            if (_session == null)
            {
                _panel.OpenPanel(PanelType.JoinMenu);
                Debug.Log("Belirtilen isimde oturum yok.");
                await _runner.Shutdown();
                _joinButton.onClick.AddListener(TryJoinByNameWithPassword);
                
                return;
            }
            if (_session.Properties != null && _session.Properties.TryGetValue("IsPasswordProtected", out var prop))
            {
                if (prop)
                {
                    Debug.Log("şifre var");
                    _passwordButton.onClick.AddListener(()=>JoinGame(_joinLobbyName.text));
                    _panel.OpenPanel(PanelType.PasswordMenu);
                    return;
                }
            }

            JoinGame(_photonManager.ActiveRunner == null ? _joinLobbyName.text : _photonManager.SessionName);
        }

        private void Reset()
        {
            _passwordButton.onClick.RemoveAllListeners();
            _joinButton.onClick.RemoveAllListeners();
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
            _sessionList = sessionList;
            Debug.Log("session dinleyiciye girdi");
        }

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) // kontrol et daha bitmedi:
        {
            Reset();
            Debug.Log($"[Fusion] Runner shutdown edildi. Sebep: {shutdownReason}");
        }

        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
        {
            if (reason == NetConnectFailedReason.ServerRefused)
            {
                Debug.Log("Bağlantı reddedildi: Yanlış şifre.");
                _panel.OpenPanel(PanelType.MainMenu);
                EnterState(_photonManager);
            }
        }
        
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
        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    }
}