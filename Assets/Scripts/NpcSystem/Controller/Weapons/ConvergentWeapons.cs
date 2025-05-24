using System;
using Fusion;
using PlayerSystem.Controller;
using UnityEngine;

namespace BotSystem.Controller.Weapons
{
    public class ConvergentWeapons : NetworkBehaviour
    {
        #region Self Variables

        #region Public

        public int Damage;
        [Networked] public NetworkObject Parent { get; set; }
        [Networked] public NetworkObject GrandParent { get; set; }

        #endregion

        #endregion

        public override void Spawned()
        {
            transform.SetParent(Parent.transform);
            transform.localPosition = Vector3.zero;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (Parent == null) return;
            if (other.CompareTag(GrandParent.tag)) return;
            if (other.TryGetComponent<NpcManager>(out var npc))
            {
                npc.OnSetDamage(Damage);
            }

            if (other.TryGetComponent<TowerController>(out var towerController))
            {
                towerController.OnSetDamage(Damage);
            }
        }
    }
}