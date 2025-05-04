using System;
using Fusion;
using PhotonSystem;
using PlayerSystem.Data.UnityObject;
using PlayerSystem.Data.ValueObject;
using SpawnSystem;
using UnityEngine;

namespace BotSystem.Controller
{
    public class TowerController : NetworkBehaviour
    {

        #region Self Variables

        #region Private Variables

        private SpawnController _spawnController;
        
        public SerializableDictionary<int, TowerLwlData> _levelList;
        private int lwl;

        #endregion

        #endregion
        

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        public void RPC_TowerObject()
        {
            if (Runner == null)
            {
                Debug.LogError("Runner is null, cannot spawn.");
                return;
            }
            var tower = Runner.Spawn(
                _levelList[lwl].Tower, 
                transform.position,     
                transform.rotation);
            if (lwl == 0) return;
            Runner.Despawn(_levelList[lwl].Tower);
        }

        
    }
}