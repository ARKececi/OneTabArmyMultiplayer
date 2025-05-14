using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BotSystem.Animation;
using BotSystem.Controller;
using BotSystem.Controller.Weapons;
using BotSystem.Data.UnityObject;
using BotSystem.Data.ValueObject;
using Fusion;
using PlayerSystem;
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
        [Networked] public Color PlayerColor { get; set; }
        [Networked] public NetworkObject Player{ get; set; }
        [Networked] public Vector3 Position { get; set; }
        

        #endregion

        #region Serialized Variables

        [SerializeField] private NavMeshAgent _agent;
        [SerializeField] private AnimationController _animationController;
        [SerializeField] private NPCEnum _me;
        [SerializeField] private NetworkObject Weapon;
        [SerializeField] private NpcLevelObject npcSpawnObject;
        
        #endregion

        #region Private Variables

        private Vector3 Hit { get; set; }
        private AnimationEnum animationEnum;

        [Networked] public int healt { get; set; }
        [Networked] private int damage { get; set; }
        [Networked] public float attackTime { get; set; }
        private NetworkObject Parrentobj;
        
        [Networked] public bool fight { get; set; }
        [Networked] private bool wait { get; set; }
        [Networked] private float atackField { get; set; }

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
            RPC_ItemSpawn(0);
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

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void RPC_ItemSpawn(int lwl)
        {
            
            var obj = npcSpawnObject.LwlNpc[lwl].SpawnNpc;
            foreach (var VARIABLE in obj)
            {
                Parrentobj = VARIABLE.BodySpawnPoint;
                Weapon = Runner.Spawn(
                    VARIABLE.SpawnObject,
                    VARIABLE.SpawnObject.transform.position,
                    VARIABLE.SpawnObject.transform.rotation,
                    inputAuthority: Object.InputAuthority,
                    OnBeforeUpdate);
            }
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
            if (EnemyList.Count > 0)
            { 
                if (GetClosestTransform(EnemyList,transform.position) == null) return;
                Position = GetClosestTransform(EnemyList, transform.position).transform.position;
                if (!fight)_agent.destination = Position;
                
                
                var distance = Vector3.Distance(Object.transform.position, GetClosestTransform(EnemyList,transform.position).transform.position);
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
            EnemyList.Add(npcManager);
            _agent.stoppingDistance = atackField;
        }
        
        public void RemoveEnemy(NpcManager npcManager)
        {
            if (EnemyList[0] == npcManager) fight = false;
            EnemyList.Remove(npcManager);
            if (EnemyList.Count == 0)
            {
                Hit = transform.position;
                _agent.stoppingDistance = 0;
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