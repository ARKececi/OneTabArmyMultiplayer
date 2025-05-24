using System;
using Fusion;
using PlayerSystem.Controller;
using UnityEngine;

namespace BotSystem.Controller
{
    public class NpcPhysicsController : NetworkBehaviour
    {
        #region Self Variables

        #region Serialized Variables

        [Networked] public NpcManager _manager { set; get; }

        #endregion

        #endregion
        
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(_manager.tag)) return;
            if (other.TryGetComponent<TowerController>(out var tower))
            {
                _manager.AddEnemyBase(tower);
            }
            if (!other.TryGetComponent<NpcManager>(out var npc)) return;
            _manager.AddEnemy(npc);
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag(_manager.tag)) return;
            if (!other.TryGetComponent<NpcManager>(out var npc)) return;
            _manager.RemoveEnemy(npc);
            
        }
    }
}