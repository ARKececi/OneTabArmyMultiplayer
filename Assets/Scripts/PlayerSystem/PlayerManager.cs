using Fusion;
using InputSystem.Params;
using PlayerSystem.Controller;
using PlayerSystem.Data.UnityObject;
using PlayerSystem.Data.ValueObject;
using SpawnSystem;
using UnityEngine;

namespace PlayerSystem
{
    public class PlayerManager : NetworkBehaviour
    {
        #region Self Variables

        #region Serialized Variables

        [SerializeField] private Transform _camTransform;
        [SerializeField] private GameObject _cameraPrefab;
        [SerializeField] private float Timer;

        #endregion

        #region Controller Variables
        
        private GameObject cameraInstance;
        
        private SerializableDictionary<int, TowerLwlData> _levelList;
        private int lwl;

        #endregion

        #region Private Variables

        private NetworkObject tower;
        private MoveAndAligmentController moveAndAligmentController;
        private float _timer;

        #endregion

        #endregion
        
        private void TimerClass()
        {
            _timer -= Time.fixedDeltaTime;
            if (_timer <= 0)
            {
                NpcSpawn(NPCPrefabEnum.Soldier);
                _timer = Timer;
            }
        }

        public override void Spawned()
        {
            if (!HasInputAuthority) return;
            cameraInstance = Instantiate(_cameraPrefab, transform, true);
            cameraInstance.transform.localPosition = _camTransform.localPosition;
            cameraInstance.transform.rotation = _camTransform.rotation;
            RPC_TowerObject();
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
                transform.rotation);
            if (lwl == 0) return;
            Runner.Despawn(_levelList[lwl].Tower);
        }

        private void Awake()
        {
            moveAndAligmentController = GetComponent<MoveAndAligmentController>();
            _levelList = Resources.Load<SO_TowerLwlData>("Data/SO_TowerLwlData").lwls;
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
        
        public void NpcSpawn(NPCPrefabEnum npcEnum)
        {
            moveAndAligmentController.RPC_SpawnObject(NPCPrefabEnum.Soldier);
        }

        public void Reset()
        {
            moveAndAligmentController.Reset();
            Runner.Despawn(tower);
        }
    }
}