using System;
using System.Collections.Generic;
using Fusion;
using PanelSystem;
using PanelSystem.Enums;
using PhotonSystem.States;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace PhotonSystem.Data
{
    [Serializable]
    public class PhotonElements
    {
        [Header("ListMenu Elements")]
        public Transform LobbyListContent;
        public Button JoinLobbyButtonPrefab;
        
        [Header("CreateMenu Elements")]
        public TMP_InputField CreateLobyName;
        public TMP_InputField CreateLobyMaxPlayer;
        public TMP_InputField CreatePassword;
        public Toggle PasswordToggle;
        public Button CreateButton;
        
        [Header("JoinMenu Elements")]
        public TMP_InputField JoinLobbyName;
        public Button JoinButton;
        public TMP_InputField JoinLobbyPassword;
        public Button PasswordButton;

        [Header("MainMenu Elements")] 
        public Button QuickGameButton;
        public Button LobbyListMenuButton;
        public Button CreateMenuButton;
        public Button JoinMenuButton;
        public Button Back;

        [Header("Connected Elements")] 
        public Button DisconnectedButton;
        
        [Header("Menus")] 
        public SerializableDictionary<PanelType, GameObject> Menus;

        // [Header("GeneralVariable")] 

    }
}