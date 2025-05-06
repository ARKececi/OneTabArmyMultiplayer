using System;
using System.Collections.Generic;
using BotSystem.Animation;
using Fusion;
using PlayerSystem;
using SpawnSystem.Animation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

namespace BotSystem
{
    public class BotManager : NetworkBehaviour
    {
        #region Self Variables

        #region Public Variables

        public List<BotManager> EnemyList = new();// hedef manager;
        [Networked] public Color PlayerColor { get; set; }
        [Networked] public NetworkObject Player{ get; set; }
        [Networked] public bool Fight { get; set; }

        #endregion

        #region Serialized Variables

        [SerializeField] private NavMeshAgent _agent;
        [SerializeField] private AnimationController _animationController;
        
        #endregion

        #region Private Variables

        private Vector3 Hit { get; set; }
        private AnimationEnum animationEnum;

        #endregion

        #endregion
        
        public override void Spawned()
        {
            _animationController = GetComponent<AnimationController>();
            SetColor();
        }

        public void Update()
        {
            SetTarget();
        }
        
        public void RPC_AnimationControl(AnimationEnum animationenum)
        {
            if (!HasStateAuthority) return;
            if (animationenum == animationEnum) return;
            _animationController.RPC_SwichAnimation(animationenum);
            animationEnum = animationenum;
        }
        
        public void OnHit(Vector3 MouseHit)
        {
            if (EnemyList.Count != 0) return;
            if (IsValidNavMeshPosition(MouseHit, out Vector3 validPosition))
            {
                Hit = validPosition;
            }
        }

        private void SetTarget()
        {
            if (!HasStateAuthority) return;
            
            if (EnemyList.Count > 0)
            {
                if (GetClosestTransform(EnemyList,transform.position) != null){
                    if (!Fight)_agent.destination = GetClosestTransform(EnemyList,transform.position).transform.position;
                }
                else
                {
                    NullClear();
                    return;
                }
                
                var distance = Vector3.Distance(Object.transform.position, GetClosestTransform(EnemyList,transform.position).transform.position);
                if ( _agent.velocity.magnitude > 0.7f || distance > 1f)
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
                    Fight = true;
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

        public void AddEnemy(BotManager botManager)
        {
            EnemyList.Add(botManager);
            _agent.stoppingDistance = 1f;
        }
        
        public void RemoveEnemy(BotManager botManager)
        {
            if (EnemyList[0] == botManager) Fight = false;
            EnemyList.Remove(botManager);
            if (EnemyList.Count == 0)
            {
                Hit = transform.position;
                _agent.stoppingDistance = 0;
            }
        }

        private void NullClear()
        {
            EnemyList.Remove(EnemyList[0]);
        }
        
        private BotManager GetClosestTransform(List<BotManager> list, Vector3 reference)
        {
            if (list == null || list.Count == 0)
                return null;

            BotManager closest = null;
            float minDistanceSqr = Mathf.Infinity;

            foreach (BotManager t in list)
            {
                if (t == null) continue;
                float sqrDist = (t.transform.position - reference).sqrMagnitude;
                if (sqrDist < minDistanceSqr)
                {
                    minDistanceSqr = sqrDist;
                    closest = t;
                }
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