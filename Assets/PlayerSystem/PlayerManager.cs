using System;
using System.Collections.Generic;
using Fusion;
using InputSystem.Params;
using PlayerSystem.States;
using SpawnSystem;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

namespace PlayerSystem
{
    public class PlayerManager : NetworkBehaviour
    {
        #region Self Variables

        #region Public Variables
        
        

        #endregion

        #region Serialized Variables

        [SerializeField] private Transform _camTransform;
        [SerializeField] private GameObject _cameraPrefab;

        #endregion

        #region Controller Variables

        private SpawnController _spawn;
        private GameObject _cameraInstance;

        #endregion

        #endregion
        
        public override void Spawned()
        {
            Debug.Log(Object.HasInputAuthority);
            if (Object.HasInputAuthority)
            {
                _cameraInstance = Instantiate(_cameraPrefab, transform, true);
                _cameraInstance.transform.localPosition = _camTransform.localPosition;
                _cameraInstance.transform.rotation = _camTransform.rotation;
            }
        }

        private void Awake()
        {
            _spawn = GetComponent<SpawnController>();
        }
        

        public override void FixedUpdateNetwork()
        {
            if (!Object.HasInputAuthority) return;
            if (GetInput(out NetInput mouseParams))
            {
                if (!mouseParams.İsClick) return;
                _spawn.Spawn(mouseParams.Input, NPCPrefabEnum.Soldier);

            }
        }
        
    }
}