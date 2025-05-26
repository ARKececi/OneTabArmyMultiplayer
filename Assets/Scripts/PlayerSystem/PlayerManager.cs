using System.Collections.Generic;
using Extentions.GameSystem;
using Fusion;
using InputSystem.Params;
using PlayerSystem.Controller;
using PlayerSystem.Data.UnityObject;
using PlayerSystem.Data.ValueObject;
using Signals;
using SpawnSystem;
using UnityEngine;
using UnityEngine.EventSystems;

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
        [SerializeField] private GameObject _eventSystem;
        [SerializeField] private float Timer;
        [SerializeField] private Material _material;
        [SerializeField]private ScoreController scoreController;
        [SerializeField] private WinController _winController;
        [SerializeField] private GameObject _button;

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
        
        private float _timer;

        #endregion

        #endregion

        public override void Spawned()
        {
            Subscribe();

            if (!HasInputAuthority) return;
            var system = Instantiate(_eventSystem, transform, true);
            cameraInstance = Instantiate(_cameraPrefab, transform, true);
            scoreController = FindObjectOfType<ScoreController>();
            _winController = FindObjectOfType<WinController>();
            cameraInstance.transform.localPosition = _camTransform.localPosition;
            cameraInstance.transform.rotation = _camTransform.rotation;
            RPC_TowerObject(1);
            RPC_SetColor();
            
        }

        private void Subscribe()
        {
            PlayerSignals.Instance.onExp += OnExp;
            GameSignals.Instance.onFinish += Onfinish;
            PlayerSignals.Instance.onDisconnect += OnDisconnect;
            PlayerSignals.Instance.onSpawnEnum += TowerObject;
        }

        private void OnDisconnect()
        {
                Runner.Shutdown();
        }

        private void Onfinish(PlayerRef playerRef)
        {
            RPC_OnFinish(playerRef);
        }
        
        [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
        public void RPC_OnFinish(PlayerRef playerRef)
        {
            
            if(Object.InputAuthority == playerRef)
                _winController.OnFinal("Lose");
            else
                _winController.OnFinal("Win");
        }

        private void OnExp(NetworkObject networkObject, int exp)
        {
            if(!HasInputAuthority) return;
            if (Object.InputAuthority == networkObject.InputAuthority)return;
            scoreController.TowerEXP(exp);
        }

        private void TowerObject(CardType cardType ,int lwl)
        {
            if(!HasInputAuthority) return;
            if (cardType == CardType.Castle)
            {
                lwl++;
                RPC_TowerObject(lwl);
            }
        }
        
        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void RPC_TowerObject(int lwl)
        {
            this.lwl = lwl;
            if (Runner == null)
            {
                Debug.LogError("Runner is null, cannot spawn.");
                return;
            }

            if (lwl != 1)
            {
                Runner.Despawn(tower);
            }
            tower = Runner.Spawn(
                _levelList[lwl].Tower,
                transform.position,
                transform.rotation,
                Object.InputAuthority,
                OnBeforeUpdate);

        }

        private void OnBeforeUpdate(NetworkRunner runner, NetworkObject networkObject)
        {
            var controller = networkObject.GetComponent<TowerController>();
            controller.ColorToApply = PlayerColor;
            controller.Parent = Object;
            controller.tag = Object.InputAuthority.ToString();
            controller.healt = _levelList[lwl].Healt;
            controller.CamTransform = _camTransform.localPosition;
        }

        private void Awake()
        {
            moveAndAligmentController = GetComponent<MoveAndAligmentController>();
            _levelList = Resources.Load<SO_TowerLwlData>("Data/SO_TowerLwlData").lwls;
        }
        
        public void SetReady()
        {   
            RPC_SetReady(true);
            _button.SetActive(false);
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
                    if(IsPointerOverUIObject())
                        moveAndAligmentController.RPC_MoveObject(ConvertToWorldPosition(mouseParams.Input));
                    else
                    {
                        Debug.Log(IsPointerOverUIObject());
                    }
            }
        }  
        
        [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
        public void RPC_SetColor()
        {
            GetComponent<SpawnController>().PlayerColor = PlayerColor;
        }
        
        private bool IsPointerOverUIObject()
        {
            PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
            eventDataCurrentPosition.position =  new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
            return results.Count <= 0;
        }

        public void Reset()
        {
            moveAndAligmentController.Reset();
            Runner.Despawn(tower);
        }
    }
}