using System;
using System.Collections.Generic;
using Fusion;
using PhotonSystem.Data;
using PhotonSystem.Data.UnityObjectValue;
using PlayerSystem;
using SpawnSystem;
using Unity.VisualScripting;
using UnityEngine;

namespace PhotonSystem
{
    public class RegisterController : NetworkBehaviour, IPlayerJoined, IPlayerLeft
    {

        #region Self Variables

        #region Networked Variables

        [Networked, Capacity(12)] public NetworkDictionary<PlayerRef, NetworkObject> PlayerData => default;

        #endregion

        #region Serialized Variables

        [SerializeField]private NetworkPrefabRef PlayerPrefab;

        #endregion

        #region Private Variables

        private List<TransformData> transformData;
        
        #endregion

        #endregion

        private void Awake()
        {
            var data = Resources.Load<SO_PositonData>("Data/SO_PositonData").PlayerSpawn;
            transformData = data;
        }

        public void PlayerJoined(PlayerRef player)
        {
            if (!Runner.IsServer) return;
            
                if (PlayerData.Count <= transformData.Count)
                {
                    Vector3 spawnPositon = transformData[PlayerData.Count].PlayerPosition;
                    Quaternion spawnRotation = Quaternion.Euler(transformData[PlayerData.Count].PlayerRotation);
                    NetworkObject playerObj = Runner.Spawn(
                        PlayerPrefab,
                        spawnPositon,
                        spawnRotation,
                        inputAuthority: player // 👈 BU ÇOK ÖNEMLİ
                    );
                    Debug.Log($"Spawned player object for {player}, InputAuthority: {playerObj.InputAuthority}");
                    PlayerData.Add(player, playerObj);
                }
        }

        public void PlayerLeft(PlayerRef player)
        {
            if (!Runner.IsServer) return;
            Debug.Log("çıktı");
            PlayerData[player].GetComponent<PlayerManager>().Reset();
            Runner.Despawn(PlayerData[player]);
            PlayerData.Remove(player);
        }
    }
}