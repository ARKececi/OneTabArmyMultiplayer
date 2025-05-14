using System.Collections.Generic;
using BotSystem;
using Fusion;
using SpawnSystem.Data.Enum;
using UnityEngine;

namespace SpawnSystem
{
    public class SpawnController : NetworkBehaviour
    {

        #region Self Variables

        #region Public Variables

        [Networked] public Color PlayerColor { get; set; }

        #endregion

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

        
        public NetworkObject OnSpawn(Vector3 position, NPCEnum eNpcEnum)
        {
            if (Runner == null)
            {
                Debug.LogError("Runner is null");
                return null;
            }
            var npc = Runner.Spawn(
                prefabData.NPCPrefabs[eNpcEnum],
                position, Quaternion.identity,
                inputAuthority: Object.InputAuthority,
                OnBeforeUpdate);
            if (npc != null)
            {
                spawnNpc.Add(npc);
                npc.tag = Object.InputAuthority.ToString();
                NpcManager npcManagger = npc.GetComponent<NpcManager>();
                npcManagger.Player = Object;
            }
            
            return npc;
        }

        private void OnBeforeUpdate(NetworkRunner runner, NetworkObject networkObject)
        {
            var manager = networkObject.GetComponent<NpcManager>();
            manager.PlayerColor = PlayerColor;
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