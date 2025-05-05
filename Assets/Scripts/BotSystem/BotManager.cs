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
        [Networked]public BotManager Enemy { set; get; }
        [Networked] public NetworkObject Player{ get; set; }

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
        }

        public void Update()
        {
            SetTarget();
           
            RPC_AnimationControl(_agent.velocity.magnitude > 0.7f ? AnimationEnum.Run : AnimationEnum.Idle);
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
            if (EnemyList.Count > 0)
            {
                _agent.destination = EnemyList[0].transform.position;
            }
            else
            {
                _agent.destination = Hit;
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
        }
        
        public void RemoveEnemy(BotManager botManager)
        {
            EnemyList.Remove(botManager);
            if (EnemyList.Count == 0)
            {
                Hit = transform.position;
            }
        }
    }
}