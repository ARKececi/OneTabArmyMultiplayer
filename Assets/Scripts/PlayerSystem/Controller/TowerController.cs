using System;
using Extentions.GameSystem;
using Fusion;
using UnityEngine;

namespace PlayerSystem.Controller
{
    public class TowerController : NetworkBehaviour
    {
        #region Self Variables
        [Networked] public Color ColorToApply { set; get; }
        [Networked] public int healt { get; set; }
        [Networked] public NetworkObject Parent { set; get; }

        #endregion

        public override void Spawned()
        {
            MeshRenderer[] mesh = GetComponentsInChildren<MeshRenderer>();
            foreach (var VARIABLE in mesh)
            {
                VARIABLE.material.color = ColorToApply;
            }
            Object.transform.SetParent(Parent.transform);
        }

        public void OnSetDamage(int damage)
        {
            if(HasStateAuthority)
                RPC_OnSetDamage(damage);
        }
        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_OnSetDamage(int damage)
        {
            if (healt <= 0) return; // zaten ölü
            healt -= damage;
            if (healt > 0) return;
            Debug.Log("bitti");
            PlayerRef @ref = Object.InputAuthority;
            GameSignals.Instance.onFinish?.Invoke(@ref);
            GameSignals.Instance.onGame?.Invoke(false);
            
        }
    }
}