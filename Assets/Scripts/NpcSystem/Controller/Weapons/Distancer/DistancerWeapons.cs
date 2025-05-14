using System;
using System.Threading.Tasks;
using BotSystem.Controller.Weapons;
using Fusion;
using UnityEngine;

namespace BotSystem.Controller
{
    public class DistancerWeapons : NetworkBehaviour
    {
        #region Self Variables

        #region Public Variables

        public int Damage;
        [Networked] public NetworkObject Parent { get; set; }
        [Networked] public NetworkObject GrandParent { get; set; }
        public float Timer;

        #endregion

        #region Serialized Variables

        [SerializeField] private NetworkObject _arrow;
        
        #endregion

        #region Private Variables

        [Networked] private DistancerPhysiscsController bullet { get; set; }
        private float timer;

        #endregion

        #endregion

        public override void Spawned()
        {
            transform.SetParent(Parent.transform);
            transform.localPosition = Vector3.zero;
            Timer = GrandParent.GetComponent<NpcManager>().attackTime;
            timer = Timer;
        }

        public void OnTrigger(NpcManager npc)
        {
                npc.OnSetDamage(Damage);
        }

        public void ArrowSpawn()
        {
            var arrowSpawn = Runner.Spawn(
                _arrow,
                transform.position,
                transform.rotation,
                inputAuthority: Object.InputAuthority,
                OnBeforeUpdate);
            
        }

        public void OnBeforeUpdate(NetworkRunner runner, NetworkObject networkObject)
        {
            var arrow = networkObject.GetComponent<DistancerPhysiscsController>();
            arrow.Damage = Damage;
            arrow.tag = Object.InputAuthority.ToString();
            bullet = arrow;
        }

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private async void RPC_FireTimer()
        {
            if (timer <= 0)
            {
                timer = Timer;
                ArrowSpawn();
                await Task.Yield();
                bullet.Launch(GrandParent.GetComponent<NpcManager>().Position);
                bullet = null;
            }
            timer -= Time.deltaTime;
        }

        public void Update()
        {
            if (!HasInputAuthority) return;
            if (GrandParent.GetComponent<NpcManager>().fight)
            {
                RPC_FireTimer();
            }
            else
            {
                timer = Timer;
            }
        }
    }
}