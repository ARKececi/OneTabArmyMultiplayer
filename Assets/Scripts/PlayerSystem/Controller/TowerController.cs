using Fusion;
using UnityEngine;

namespace PlayerSystem.Controller
{
    public class TowerController : NetworkBehaviour
    {
        #region Self Variables
        [Networked] public Color ColorToApply { set; get; }

        #endregion

        public override void Spawned()
        {
            MeshRenderer[] mesh = GetComponentsInChildren<MeshRenderer>();
            foreach (var VARIABLE in mesh)
            {
                VARIABLE.material.color = ColorToApply;
            }
        }
    }
}