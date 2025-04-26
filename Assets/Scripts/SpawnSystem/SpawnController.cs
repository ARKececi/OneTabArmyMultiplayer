using System;
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

        #region Serialized Variables

        

        #endregion

        #region Private Variables

        private NPCPrefabData prefabData;

        #endregion

        #endregion

        private void Awake()
        {
            prefabData = Resources.Load<SO_NPCPrefabs>("Data/SO_NPCPrefabs").NPCData;
        }

        public void Spawn(Vector3 mouseParams, NPCPrefabEnum prefabEnum )
        {
            var positon = ConvertToWorldPosition(mouseParams);
            RPC_SpawnObject(positon, prefabData.NPCPrefabs[prefabEnum]);
        }
        
        private Vector3 ConvertToWorldPosition(Vector2 screenPosition)
        {
            if (Camera.main != null)
            {
                Ray ray = Camera.main.ScreenPointToRay(screenPosition);
                Debug.DrawRay(ray.origin, ray.direction * 100f, Color.red, 2f);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    return hit.point; // Dünya pozisyonu
                }
            }
            return Vector3.zero;
        }
        
        
        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void RPC_SpawnObject(Vector3 position, NetworkPrefabRef prefabRef)
        {
            if (Runner == null)
            {
                Debug.LogError("Runner is null, cannot spawn.");
                return;
            }
        
            Runner.Spawn(prefabRef, position, Quaternion.identity);
        }
    }
}