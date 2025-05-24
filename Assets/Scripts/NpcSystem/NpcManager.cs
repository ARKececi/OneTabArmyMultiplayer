using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BotSystem.Animation;
using BotSystem.Controller;
using BotSystem.Controller.Weapons;
using BotSystem.Data.UnityObject;
using BotSystem.Data.ValueObject;
using Extentions.GameSystem;
using Fusion;
using PlayerSystem;
using PlayerSystem.Controller;
using Signals;
using SpawnSystem.Animation;
using SpawnSystem.Data.Enum;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

namespace BotSystem
{
    public class NpcManager : NetworkBehaviour
    {
        #region Self Variables

        #region Public Variables

        public List<NpcManager> EnemyList = new();// hedef manager;
        public TowerController TowerController;
        [Networked] public Color PlayerColor { get; set; }
        [Networked] public NetworkObject Player{ get; set; }
        [Networked] public Vector3 Position { get; set; }
        

        #endregion

        #region Serialized Variables

        [SerializeField] private NavMeshAgent _agent;
        [SerializeField] private AnimationController _animationController;
        [SerializeField] private NPCEnum _me;
        [SerializeField] private List<NetworkObject> Weapon;
        [SerializeField] private NpcLevelObject npcSpawnObject;
        
        #endregion

        #region Private Variables

        [SerializeField] private Vector3 Hit;
        private AnimationEnum animationEnum;

        [Networked] public int healt { get; set; }
        [Networked] private int damage { get; set; }
        [Networked] public float attackTime { get; set; }
        private NetworkObject Parrentobj;
        
        [Networked] public bool fight { get; set; }
        [Networked] public bool wait { get; set; }
        [Networked] private float atackField { get; set; }
        [Networked] public int lwl { get; set; }
        
        [Networked] public bool start { set; get; }

        #endregion

        #endregion

        private void DataSet()
        {
            // if (!HasStateAuthority) return;
            var data = Resources.Load<SO_NpcData>("Data/SO_NpcData").NpcData;
            healt = data[_me].Healt;
            damage = data[_me].Damage;
            attackTime = data[_me].AttackTime;
            atackField = data[_me].AttackField;
            wait = false;
        }

        public override void Spawned()
        {
            _animationController = GetComponent<AnimationController>();
            SetColor();
            DataSet();
            if (!HasInputAuthority) return;
            RPC_ItemSpawn(lwl);
            GameSignals.Instance.onFinish += RPC_OnStart;
        }
        
        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void RPC_OnStart(PlayerRef value)
        {
            TowerController = null;
        }


        public void Update()
        {
            if (healt <= 0) return;
            SetTarget();
        }
        
        public void OnHit(Vector3 MouseHit)
        {
            if (EnemyList.Count != 0) return;
            if (IsValidNavMeshPosition(MouseHit, out Vector3 validPosition))
            {
                Hit = validPosition;
            }
        }

        public void OnSetDamage( int damage)
        {
            // Bu kontrol sadece local oyuncunun tetiklemesini engeller
            if (!HasStateAuthority) return;
            RPC_OnSetDamage(damage);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_OnSetDamage(int damage)
        {
            if (healt <= 0) return; // zaten ölü
            healt -= damage;
            if (healt > 0) return;
            RPC_AnimationControl(AnimationEnum.Dead);
            PlayerSignals.Instance.onExp?.Invoke(Object,2);
            
            OnDeSpawn();
                wait = true;
        }
        
        private async void OnDeSpawn()
        {
            await Task.Yield();
            var time = _animationController._animator.GetCurrentAnimatorStateInfo(0).length;
            await Task.Delay((int)time * 1000);
            if (Runner == null) return;
            Runner.Despawn(Object);
        }

        public void OnItemSpawn(int lwl)
        {
            if (HasStateAuthority)
                RPC_ItemSpawn(lwl);
        }

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void RPC_ItemSpawn(int lwl)
        {
            if (npcSpawnObject.LwlNpc.Count < lwl)
            {
                lwl = npcSpawnObject.LwlNpc.Count;
                Debug.Log("max lwl");
            }
            if (npcSpawnObject.LwlNpc.Count <= 0)
            {
                Debug.Log("lwl içeriği boş");
                return;
            }
            if (Weapon.Count > 0)
            {
                foreach (var VARIABLE in Weapon)
                {
                    Runner.Despawn(VARIABLE);
                }       
                Weapon.Clear();
            }
            var obj = npcSpawnObject.LwlNpc[lwl].SpawnNpc;
            foreach (var VARIABLE in obj)
            {
                Parrentobj = VARIABLE.BodySpawnPoint;
                Weapon.Add(Runner.Spawn(
                    VARIABLE.SpawnObject,
                    VARIABLE.SpawnObject.transform.position,
                    VARIABLE.SpawnObject.transform.rotation,
                    inputAuthority: Object.InputAuthority,
                    OnBeforeUpdate));
            }
            
            if (lwl == 0) return;
            float multiplier = lwl / 10f;
            healt += (int)(healt * multiplier);
            damage += (int)(damage * multiplier);
        }
        
        private void OnBeforeUpdate(NetworkRunner runner, NetworkObject networkObject)
        {
            ConvergentWeapons compA = networkObject.GetComponent<ConvergentWeapons>();
            DistancerWeapons compB = networkObject.GetComponent<DistancerWeapons>();
            
            if (compA != null)
            {
                compA.Damage = damage;
                compA.Parent = Parrentobj;
                compA.GrandParent = Object;
            }
            else if (compB != null)
            {
                compB.Damage = damage;
                compB.Parent = Parrentobj;
                compB.GrandParent = Object;
            }
        }

        private void RPC_AnimationControl(AnimationEnum animationenum)
        {
            if (!HasStateAuthority) return;
            if (animationenum == animationEnum) return;
            _animationController.SwichAnimation(animationenum,attackTime);
            animationEnum = animationenum;
        }

        private void SetTarget()
        {
            if (!HasStateAuthority) return;
            if (wait) return;
            if (EnemyList.Count > 0 || TowerController != null)
            { 
                if (GetClosestTransform(EnemyList,transform.position) == null && TowerController == null) return;
                
                float distance = 0;
                if (GetClosestTransform(EnemyList,transform.position) != null)
                {
                    Position = GetClosestTransform(EnemyList, transform.position).transform.position;
                    distance = Vector3.Distance(Object.transform.position, GetClosestTransform(EnemyList,transform.position).transform.position);
                }
                else if (TowerController != null)
                {
                    Position = TowerController.transform.position;
                    distance = Vector3.Distance(Object.transform.position, Position);
                }
                if (!fight)_agent.destination = Position;
                
                if ( _agent.velocity.magnitude > 0.7f || distance > atackField)
                {
                    if (_agent.velocity.magnitude > 0.7f)
                    {
                        RPC_AnimationControl(AnimationEnum.Run);
                    }
                    else
                    {
                        RPC_AnimationControl(AnimationEnum.Idle);
                    }
                }
                else
                {
                    fight = true;
                    RPC_AnimationControl(AnimationEnum.Fight);
                    _agent.ResetPath();
                    _agent.Warp(transform.position);
                }
            }
            else
            {
                _agent.destination = Hit;
                RPC_AnimationControl(_agent.velocity.magnitude > 0.7f ? AnimationEnum.Run : AnimationEnum.Idle);
            }
        }
        
        bool IsValidNavMeshPosition(Vector3 position, out Vector3 validPosition)
        {
            NavMeshHit hit;
            if (NavMesh.SamplePosition(position, out hit, 4.0f, NavMesh.AllAreas)) // 2.0f = Arama yarıçapı
            {
                validPosition = hit.position;
                return true;
            }

            validPosition = Vector3.zero;
            return false;
        }

        public void AddEnemy(NpcManager npcManager)
        {
            if (wait) return;
            if (EnemyList.Count == 0) Player.GetComponent<MoveAndAligmentController>().RPC_RemoveMoveList(this);
            EnemyList.Add(npcManager);
            _agent.stoppingDistance = atackField;
        }

        public void AddEnemyBase(TowerController towerController)
        {
            if (wait) return;
            if (towerController != null) Player.GetComponent<MoveAndAligmentController>().RPC_RemoveMoveList(this);
            _agent.stoppingDistance = atackField;
            TowerController = towerController;
        }
        
        public void RemoveEnemy(NpcManager npcManager)
        {
            // if (npcManager == null) return;
            // EnemyList.Remove(npcManager);
            if (EnemyList.Count == 0)
            {
                Debug.Log("düşman 0");
                
                Hit = transform.position;
                
            }
        }

        private void NullClear(NpcManager npc)
        {
            EnemyList.Remove(npc);
            fight = false;
        }
        
        private NpcManager GetClosestTransform(List<NpcManager> list, Vector3 reference)
        {
            if (list == null || list.Count == 0)
                return null;

            NpcManager closest = null;
            float minDistanceSqr = Mathf.Infinity;
            var nullList = new List<NpcManager>();

            foreach (NpcManager t in list)
            {
                if (t == null)
                {
                    nullList.Add(t);
                    continue;
                }
                
                float sqrDist = (t.transform.position - reference).sqrMagnitude;
                if (sqrDist < minDistanceSqr)
                {
                    minDistanceSqr = sqrDist;
                    closest = t;
                }
            }

            foreach (var VARIABLE in nullList)
            {
                NullClear(VARIABLE);
            }

            if (closest == null)
            {
                Player.GetComponent<MoveAndAligmentController>().RPC_AddMoveList(this);
                _agent.stoppingDistance = 0;
            }
            return closest;
        }

        private void SetColor()
        {
            SkinnedMeshRenderer[] mesh = GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (var VARIABLE in mesh)
            {
                VARIABLE.material.color = PlayerColor;
            }
        }
    }
}