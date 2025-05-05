using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fusion;
using Fusion.Sockets;
using InputSystem;
using PanelSystem;
using PanelSystem.Enums;
using PhotonSystem.Controller;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace PhotonSystem.States
{
    public class CreateLobbyState: IPhotonBaseState, INetworkRunnerCallbacks
    {
        #region Self Variables

        #region Private Variables

        private int playerCount;
        
        private TMP_InputField _createLobyName;
        private TMP_InputField _createLobyMaxPlayer;
        private Toggle _passwordToggle;
        private TMP_InputField _createPassword;
        private Button _createButton;
        private PanelManager.PanelManager _panel;
        
        private NetworkRunner _runner;
        private PhotonManager _photonManager;
        private string _password;
        private Dictionary<string, SessionProperty> _properties;
        private byte[] _token;

        #endregion

        #endregion
        
        public void EnterState(PhotonManager photonManager)
        {
            _createLobyName = photonManager.Data.CreateLobyName;
            _createLobyMaxPlayer = photonManager.Data.CreateLobyMaxPlayer;
            _passwordToggle = photonManager.Data.PasswordToggle;
            _createPassword = photonManager.Data.CreatePassword;
            _createButton = photonManager.Data.CreateButton;
            _panel = photonManager.Panel;
            _photonManager = photonManager;
            
            _createButton.onClick.AddListener(CreateGame);
            _passwordToggle.onValueChanged.AddListener(OnToggleChanged);
            _panel.OpenPanel(PanelType.CreateMenu);
        }

        public void ExitState(PhotonManager photonManager)
        {
            
        }
        
        private Task Reset()
        {
            _createButton.onClick.RemoveListener(CreateGame);
            _passwordToggle.onValueChanged.RemoveListener(OnToggleChanged);
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

            return Task.CompletedTask;
        }

        private void OnToggleChanged(bool arg0)
        {
            _createPassword.gameObject.SetActive(arg0);
        }
        
        private void InstanceRunner()
        {
            if (_runner != null)
            {
                Debug.LogWarning("Runner zaten mevcut, tekrar oluşturulmayacak.");
                return;
            }
            var _dinamicRunnerObject = new GameObject("Dinamic Runner");
            _dinamicRunnerObject.transform.SetParent(_photonManager.gameObject.transform);
            _runner = _dinamicRunnerObject.AddComponent<NetworkRunner>();
            _runner.AddCallbacks(_dinamicRunnerObject.AddComponent<InputManager>());


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

        private void ActivePassword()
        {
            if (_passwordToggle.isOn)
            {
                _properties = new Dictionary<string, SessionProperty>()
                {
                    { "IsPasswordProtected", SessionProperty.Convert(true) },
                };
                
                _password = _createPassword.text; // Şifre input field
                _token = System.Text.Encoding.UTF8.GetBytes(_password);
            }
            else
            {
                _properties = default;
                _password = default;
            }
        }
        
        async void CreateGame()
        {
            ActivePassword();
            InstanceRunner();
            await OperationCall();
            await LoadSceneAsync();
            // Create the Fusion runner and let it know that we will be providing user input
            _runner.ProvideInput = true;
            
            // Create the NetworkSceneInfo from the current scene
            var scene = SceneRef.FromIndex(SceneManager.GetSceneByName("GameScene").buildIndex);
            var sceneInfo = new NetworkSceneInfo();
            if (scene.IsValid) {
                sceneInfo.AddSceneRef(scene, LoadSceneMode.Additive);
            }
            // Start or join (depends on gamemode) a session with a specific name
            var startGameResult = await _runner.StartGame(new StartGameArgs()
            {
                GameMode = GameMode.Host,
                SessionName = _createLobyName.text,
                Scene = scene,
                PlayerCount = int.Parse(_createLobyMaxPlayer.text),
                SceneManager = _runner.gameObject.AddComponent<NetworkSceneManagerDefault>(),
                CustomLobbyName = "FusionLobby",
                SessionProperties = _properties
            });
            if (startGameResult.Ok)
            {
                Debug.Log($"Sunucu {_createLobyName.text} başarılı şekilde başlatıldı.");
                _photonManager.ActiveRunner = _runner;
                _photonManager.ConnetedLobby();
                
                // // AuthorityAssigner prefab'ını spawn et
                // var assignerObj = _runner.Spawn(
                //     _photonManager.AuthorityAssinger,
                //     Vector3.zero,
                //     Quaternion.identity,
                //     inputAuthority: _runner.LocalPlayer  // host olarak kendini ver
                // );
            }
            else
            {
                Debug.LogError($"Sunucu başlatma başarısız: {startGameResult.ErrorMessage}");
                _photonManager.Panel.OpenPanel(PanelType.CreateMenu);
            }
        }
        
        private async Task LoadSceneAsync()
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("GameScene");

            while (!asyncLoad.isDone)
            {
                Debug.Log("Yükleniyor: " + (asyncLoad.progress * 100) + "%");
                await Task.Yield();
            }
        }
        
        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
        {
            if (_passwordToggle.isOn == false)
            {
                Debug.Log("şifre yok");
                request.Accept();
                return;
            }
            
            if (token == null || token.Length == 0)
            {
                Debug.LogWarning("Token eksik, bağlantı reddedildi.");
                request.Refuse();
                return;
            }
            
            string expectedPassword = _photonManager.Data.CreatePassword.text;
            string incomingPassword = System.Text.Encoding.UTF8.GetString(token);

            Debug.Log("kontrol başladı");
            if (incomingPassword == expectedPassword)
            {
                Debug.Log("bağlantı kabul edildi");
                request.Accept();
            }
            else
            {
                Debug.LogWarning("Bağlantı reddedildi: Hatalı şifre.");
                request.Refuse();
            }
        }
        
        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {

        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {

        }


        public async void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
            await Reset();
        }
        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
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

        public async void OnSceneLoadDone(NetworkRunner runner)
        {

        }
        public void OnSceneLoadStart(NetworkRunner runner){ }
        

        
    }
}