using System.Collections.Generic;
using BotSystem.Animation;
using Fusion;
using PlayerSystem;
using SpawnSystem.Animation;
using UnityEngine;
using UnityEngine.AI;

namespace BotSystem
{
    public class BotManager : NetworkBehaviour
    {
        #region Self Variables

        #region Public Variables
        
         public List<BotManager> targetEnemyList = new List<BotManager>(); // hedef manager;

         public NetworkObject Player;

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
            _agent.destination = Hit;
            RPC_AnimationControl(_agent.velocity.magnitude > 0.7f ? AnimationEnum.Run : AnimationEnum.Idle);
        }
        
        public void RPC_AnimationControl(AnimationEnum animationenum)
        {
            if (!HasStateAuthority) return;
            if (animationenum == animationEnum) return;
            _animationController.RPC_SwichAnimation(animationenum);
            animationEnum = animationenum;

        }
        
        public void OnHitTarget(Vector3 MouseHit)
        {
            if (targetEnemyList.Count != 0) return;
            if (IsValidNavMeshPosition(MouseHit, out Vector3 validPosition))
            {
                Hit = validPosition;
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
    }
}