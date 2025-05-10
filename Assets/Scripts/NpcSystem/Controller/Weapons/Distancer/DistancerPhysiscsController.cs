using System;
using Fusion;
using UnityEngine;

namespace BotSystem.Controller.Weapons
{
    public class DistancerPhysiscsController :  NetworkBehaviour
    {
        #region Self Variables

        #region Public Variables
        
        [Networked]public DistancerWeapons _wepaon { get; set; }

        #endregion

        #region Serialized Variables

        [SerializeField] private Rigidbody rb;
        [SerializeField] private float gravityMultiplier = 1f;

        #endregion

        #region Private Variables

        private bool isLaunched = false;

        #endregion

        #endregion
        
        public void Launch(Vector3 target)
        {
            if (isLaunched) return;

            isLaunched = true;
            Physics.gravity *= gravityMultiplier;

            Vector3 startPos = transform.position;
            Vector3 direction = target - startPos;

            // Yatay ve düşey farkları ayır
            Vector3 horizontal = new Vector3(direction.x, 0, direction.z);
            float distance = horizontal.magnitude;
            float height = direction.y;

            float arcHeight = distance / 2f; // 🎯 Yükseklik = mesafenin yarısı

            // Parabolik kuvvet hesaplaması
            float velocityY = Mathf.Sqrt(2 * Physics.gravity.magnitude * arcHeight);
            float timeToApex = velocityY / Physics.gravity.magnitude;
            float totalTime = timeToApex + Mathf.Sqrt(2 * Mathf.Max(0.1f, (arcHeight - height)) / Physics.gravity.magnitude);

            Vector3 velocityXZ = horizontal / totalTime;
            Vector3 finalVelocity = velocityXZ + Vector3.up * velocityY;

            rb.AddForce(finalVelocity, ForceMode.VelocityChange);
        }

        public void OnTriggerEnter(Collider other)
        {
            _wepaon.OnTrigger(other);
        }
    }
}