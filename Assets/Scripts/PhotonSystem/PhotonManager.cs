using System.Collections.Generic;
using Fusion;
using PanelSystem;
using PanelSystem.Enums;
using PhotonSystem.Data;
using PhotonSystem.Data.UnityObjectValue;
using PhotonSystem.States;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace PhotonSystem
{
    public class PhotonManager : NetworkBehaviour
    {
        #region Self Variables

        #region Public Variables
        
        // Oyuncuların bilgilerini tutacak özel bir yapı
        [Networked, Capacity(12)] public NetworkDictionary<PlayerRef, NetworkObject> PlayerData => default;
        
        public NetworkPrefabRef PlayerPrefab;
        
        public PhotonElements Data = new PhotonElements();
        public SerializableDictionary<TransformData,bool> transformData = new SerializableDictionary<TransformData, bool>();
        
        public PanelManager.PanelManager Panel;
        public NetworkRunner ActiveRunner;
        public List<SessionInfo> SessionList;
        public string SessionName;

        public GameObject AuthorityAssinger;

        #endregion

        #region Private Variables
        
        private string assetPath = "Assets/PhotonSystem/Data/Data.asset"; // sonradan kullanılacak

        #endregion
        
        #region State Variables

        private AnyState _anyState;
        private CreateLobbyState _createLobbyState;
        private JoinLobbyState _joinLobbyState;
        private IPhotonBaseState _currentState;
        private QuickGameState _quickPlayState;
        private ListLobby _listLobby;
        private ConnectedState _connectedState;

        #endregion

        #endregion

        #region Button Class
        
        public void JoinButton()
        {
            SwicthState(PhotonStateEnum.JoinLobbyState);
        }

        public void ConnetedLobby()
        {
            SwicthState(PhotonStateEnum.ConnectedState);
        }

        public void OnMainMenuButton()
        {
            SwicthState(PhotonStateEnum.AnyState);
            Panel.OpenPanel(PanelType.MainMenu);
            Data.Back.gameObject.SetActive(false);
        }

        #endregion
        
        private void Awake()
        {
            var data = Resources.Load<SO_PositonData>("Data/SO_PositonData").PlayerSpawn;
            
            Data.JoinMenuButton.onClick.AddListener(()=>SwicthState(PhotonStateEnum.JoinLobbyState));
            Data.CreateMenuButton.onClick.AddListener(()=>SwicthState(PhotonStateEnum.CreateLobbyState));
            Data.QuickGameButton.onClick.AddListener(()=>SwicthState(PhotonStateEnum.QuickPlayState));
            Data.LobbyListMenuButton.onClick.AddListener(()=>SwicthState(PhotonStateEnum.ListLobbyState));
            Data.Back.onClick.AddListener(()=> OnMainMenuButton());
            Data.DisconnectedButton.onClick.AddListener(()=> OnMainMenuButton());
            
            Data.CreatePassword.gameObject.SetActive(false); // Başta kapalı

            Panel = new PanelManager.PanelManager(Data.Menus);
            _createLobbyState = new CreateLobbyState();
            _joinLobbyState = new JoinLobbyState();
            _quickPlayState = new QuickGameState();
            _listLobby = new ListLobby();
            _connectedState = new ConnectedState();
            _anyState = new AnyState();
            
            Panel.OpenPanel(PanelType.MainMenu);
            
            DontDestroyOnLoad(this);
            
        }

        private void SwicthState(PhotonStateEnum photonStateEnum)
        {
            if (photonStateEnum != PhotonStateEnum.AnyState) {Data.Back.gameObject.SetActive(true);}
            if (photonStateEnum == PhotonStateEnum.ConnectedState) {Data.Back.gameObject.SetActive(false);}
            _currentState = photonStateEnum switch
            {
                PhotonStateEnum.CreateLobbyState => _createLobbyState,
                PhotonStateEnum.AnyState       => _anyState,
                PhotonStateEnum.JoinLobbyState => _joinLobbyState,
                PhotonStateEnum.QuickPlayState => _quickPlayState,
                PhotonStateEnum.ListLobbyState => _listLobby,
                PhotonStateEnum.ConnectedState => _connectedState,
                _ => _currentState
            };
            _currentState.EnterState(this);
        }

        public void RegisterPlayer(PlayerRef playerRef, NetworkObject networkObject)
        {
            if (!PlayerData.ContainsKey(playerRef))
            {
                PlayerData.Add(playerRef, networkObject);
                Debug.Log($"[PhotonManager] [Register] Oyuncu {playerRef} kaydedildi.");
            }
            else
            {
                Debug.LogWarning($"[PhotonManager] [Register] Oyuncu {playerRef} zaten kayıtlı, tekrar eklenmedi.");
            }
        }
        
        public void UnregisterPlayer(PlayerRef playerRef)
        {
            if (PlayerData.ContainsKey(playerRef))
            {
                PlayerData.Remove(playerRef);
                Debug.Log($"[PhotonManager] [Unregister] Oyuncu {playerRef} kayıttan silindi.");
            }
            else
            {
                Debug.Log($"[PhotonManager] [Unregister] Oyuncu {playerRef} zaten kayıtlı değildi.");
            }
        }
    }
}
