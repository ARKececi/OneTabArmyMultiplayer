using Fusion;
using SpawnSystem.Animation;
using UnityEngine;

namespace BotSystem.Animation
{
    public class AnimationController : NetworkBehaviour
    {
        #region Self Variables

        #region Serialized Variables
        
        private AnimationEnum CurrentAnim { get; set; }
        
        [SerializeField] private Animator _animator;

        #endregion

        #endregion

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        public void RPC_SwichAnimation(AnimationEnum animation)
        {
            switch (animation)
            {
                case AnimationEnum.Idle:
                    ActionAnimation(false,true,false,false);
                    break;
                case AnimationEnum.Dead:
                    ActionAnimation(false,false,false,true);
                    break;
                case AnimationEnum.Fight:
                    ActionAnimation(true,false,false,false);
                    break;
                case AnimationEnum.Run: 
                    ActionAnimation(false,false,true,false);
                    break;
            }
        }
        
        private void ActionAnimation(bool fight, bool idle, bool run, bool dead)
        {
            _animator.SetBool("Fight", fight);
            _animator.SetBool("Idle", idle);
            _animator.SetBool("Run", run);
            _animator.SetBool("Dead", dead);
        }
    }
}