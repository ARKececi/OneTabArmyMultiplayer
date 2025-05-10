using System;
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

        private DistancerPhysiscsController arrow;
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

        public void OnTrigger(Collider col)
        {
            if (Parent == null) return;
            if (col.CompareTag(GrandParent.tag)) return;
            if (col.TryGetComponent<NpcManager>(out var npc))
            {
                npc.OnSetDamage(Damage);
            }
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
            var weapon = networkObject.GetComponent<DistancerPhysiscsController>();
            weapon._wepaon = this;
            arrow = weapon;

        }

        private void FireTimer()
        {
            if (timer <= 0)
            {
                timer = Timer;
                ArrowSpawn();
            }
            timer -= Time.deltaTime;
        }

        public void Update()
        {
            if (GrandParent.GetComponent<NpcManager>().fight)
            {
                FireTimer();
            }
            else
            {
                timer = Timer;
            }
        }
    }
}