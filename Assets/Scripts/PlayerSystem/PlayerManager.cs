using System.Collections.Generic;
using Extentions.GameSystem;
using Fusion;
using InputSystem.Params;
using PlayerSystem.Controller;
using PlayerSystem.Data.UnityObject;
using PlayerSystem.Data.ValueObject;
using SpawnSystem;
using UnityEngine;
using Task = System.Threading.Tasks.Task;

namespace PlayerSystem
{
    public class PlayerManager : NetworkBehaviour
    {
        #region Self Variables

        #region Public Variables

        [Networked] public Color PlayerColor { get; set; }
        [Networked] public bool IsReady { get; set; }
        
        #endregion

        #region Serialized Variables

        [SerializeField] private Transform _camTransform;
        [SerializeField] private GameObject _cameraPrefab;
        [SerializeField] private float Timer;
        [SerializeField] private Material _material;

        #endregion

        #region Controller Variables
        
        private GameObject cameraInstance;
        
        private SerializableDictionary<int, TowerLwlData> _levelList;
        private int lwl;
        private List<MeshRenderer> _meshRenderers;

        #endregion

        #region Private Variables

        [Networked] private NetworkObject tower { get; set; }
        private MoveAndAligmentController moveAndAligmentController;
        [SerializeField]private ScoreController scoreController;
        private float _timer;

        #endregion

        #endregion

        public override void Spawned()
        {
            if (!HasInputAuthority) return;
            cameraInstance = Instantiate(_cameraPrefab, transform, true);
            scoreController = FindObjectOfType<ScoreController>();
            cameraInstance.transform.localPosition = _camTransform.localPosition;
            cameraInstance.transform.rotation = _camTransform.rotation;
            RPC_TowerObject();
            RPC_SetColor();
            Subscribe();
        }

        private void Subscribe()
        {
            PlayerSignals.Instance.onExp += OnExp;
        }

        private void OnExp(NetworkObject networkObject, int exp)
        {
            if (Object.InputAuthority == networkObject.InputAuthority)return;
            scoreController.TowerEXP(exp);
        }
        
        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void RPC_TowerObject()
        {
            if (Runner == null)
            {
                Debug.LogError("Runner is null, cannot spawn.");
                return;
            }
            
            tower = Runner.Spawn(
                _levelList[lwl].Tower,
                transform.position,
                transform.rotation,
                Object.InputAuthority,
                OnBeforeUpdate);
            if (lwl == 0) return;
            Runner.Despawn(_levelList[lwl].Tower);
        }


        private void OnBeforeUpdate(NetworkRunner runner, NetworkObject networkObject)
        {
            var controller = networkObject.GetComponent<TowerController>();
            controller.ColorToApply = PlayerColor;
            controller.Parent = Object;
        }

        private void Awake()
        {
            moveAndAligmentController = GetComponent<MoveAndAligmentController>();
            _levelList = Resources.Load<SO_TowerLwlData>("Data/SO_TowerLwlData").lwls;
        }
        
        public void SetReady()
        {
            RPC_SetReady(true);
        }
        
        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPC_SetReady(bool ready)
        {
            IsReady = ready;
            GameSignals.Instance.CheckAllPlayersReady?.Invoke();
        }
        
        private Vector3 ConvertToWorldPosition(Vector2 screenPosition)
        {
            if (Camera.main != null)
            {
                Ray ray = Camera.main.ScreenPointToRay(screenPosition);
                Debug.DrawRay(ray.origin, ray.direction * 100f, Color.red, 2f);
                if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity, LayerMask.GetMask("Plane")))
                {
                    // Sadece Plane'e çarptıysa input gönder
                    return hitInfo.point;
                }
            }
            return Vector3.zero;
        }

        public override void FixedUpdateNetwork()
        {
            if (!Object.HasInputAuthority) return;
            
            if (GetInput(out NetInput mouseParams))
            {
                if (!mouseParams.İsClick) return;
                moveAndAligmentController.RPC_MoveObject(ConvertToWorldPosition(mouseParams.Input));
            }
        }  
        
        [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
        public void RPC_SetColor()
        {
            GetComponent<SpawnController>().PlayerColor = PlayerColor;
        }

        public void Reset()
        {
            moveAndAligmentController.Reset();
            Runner.Despawn(tower);
        }
    }
}