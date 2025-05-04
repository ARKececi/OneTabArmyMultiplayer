using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BotSystem;
using Fusion;
using InputSystem.Params;
using PlayerSystem;
using PlayerSystem.States;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpawnSystem
{
    public class SpawnController : NetworkBehaviour
    {

        #region Self Variables

        #region Public Variables
        
        private List<NetworkObject> spawnNpc = new();

        #endregion

        #region Private Variables

        private NPCPrefabData prefabData;
        [Networked] public NetworkObject _npcObject
        {
            get;
            set;
        }

        #endregion

        #endregion

        public void Awake()
        {
            prefabData = Resources.Load<SO_NPCPrefabs>("Data/SO_NPCPrefabs").NPCData;
        }

        
        public NetworkObject RPC_OnSpawn(Vector3 position, NPCPrefabEnum eNpcPrefabEnum)
        {
            RPC_SpawnObject(position, prefabData.NPCPrefabs[eNpcPrefabEnum]);
            return _npcObject;
        }
        
        public void RPC_SpawnObject(Vector3 position, NetworkPrefabRef eNpcPrefabEnum)
        {   
            if (Runner == null)
            {
                Debug.LogError("Runner is null, cannot spawn.");
                return;
            }
            _npcObject = Runner.Spawn(eNpcPrefabEnum, position, Quaternion.identity);
        }
        
        public void Reset()
        {
            foreach (var VARIABLE in spawnNpc)
            {
                Runner.Despawn(VARIABLE);
            }
        }
    }
}