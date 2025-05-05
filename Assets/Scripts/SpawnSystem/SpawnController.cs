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

        #endregion

        #endregion

        public void Awake()
        {
            prefabData = Resources.Load<SO_NPCPrefabs>("Data/SO_NPCPrefabs").NPCData;
        }

        public override void Spawned()
        {
            
        }

        
        public NetworkObject OnSpawn(Vector3 position, NPCPrefabEnum eNpcPrefabEnum)
        {
            if (Runner == null)
            {
                Debug.LogError("Runner is null");
                return null;
            }
            var npc = Runner.Spawn(prefabData.NPCPrefabs[eNpcPrefabEnum], position, Quaternion.identity);
            if (npc != null)
            {
                spawnNpc.Add(npc);
                npc.tag = Object.InputAuthority.ToString();
                BotManager npcManagger = npc.GetComponent<BotManager>();
                npcManagger.Player = Object;
            }
            
            return npc;
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