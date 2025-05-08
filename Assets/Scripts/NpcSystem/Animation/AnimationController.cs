using System.Threading.Tasks;
using Fusion;
using SpawnSystem.Animation;
using UnityEngine;

namespace BotSystem.Animation
{
    public class AnimationController : NetworkBehaviour
    {
        #region Self Variables

        #region Serialized Variables

        [Networked] private AnimationEnum CurrentAnim { get; set; }

        [SerializeField] public Animator _animator;

        #endregion

        #endregion

        
        public void SwichAnimation(AnimationEnum animation, float desiredDuration = -1)
        {
            switch (animation)
            {
                case AnimationEnum.Idle:
                    RPC_ActionAnimation(false, true, false, false);
                    break;
                case AnimationEnum.Dead:
                    RPC_ActionAnimation(false, false, false, true);
                    break;
                case AnimationEnum.Fight:
                    RPC_ActionAnimation(true, false, false, false, desiredDuration);
                    break;
                case AnimationEnum.Run:
                    RPC_ActionAnimation(false, false, true, false);
                    break;
            }
        }
        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private async void RPC_ActionAnimation(bool fight, bool idle, bool run, bool dead, float desiredDuration = -1)
        {
            _animator.SetBool("Fight", fight);
            _animator.SetBool("Idle", idle);
            _animator.SetBool("Run", run);
            _animator.SetBool("Dead", dead);

            await Task.Yield(); // Bir frame bekle

            AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
            float currentLength = stateInfo.length;
            
            if (desiredDuration > 0f && currentLength > 0f)
            {
                _animator.speed = currentLength / desiredDuration;
            }
            else
            {
                _animator.speed = 1f;
            }
        }
        

    }
}