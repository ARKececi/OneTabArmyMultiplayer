using System;
using Fusion;
using UnityEngine;

namespace BotSystem.Controller
{
    public class NpcPhysicsController : NetworkBehaviour
    {
        #region Self Variables

        #region Serialized Variables

        [Networked] public BotManager _manager { set; get; }

        #endregion

        #endregion
        
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(_manager.tag)) return;
            if (!other.TryGetComponent<BotManager>(out var npc)) return;
            _manager.AddEnemy(npc);
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag(_manager.tag)) return;
            if (!other.TryGetComponent<BotManager>(out var npc)) return;
            _manager.RemoveEnemy(npc);
            
        }
    }
}