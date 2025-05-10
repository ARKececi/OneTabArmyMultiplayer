using System;
using Fusion;
using UnityEngine;

namespace BotSystem.Controller.Weapons
{
    public class DistancerPhysiscsController : NetworkBehaviour
    {
        [Networked] public DistancerWeapons Wepaon { get; set; }

        [SerializeField] private float travelDuration = 1.5f;
        [SerializeField] private float arcHeightMultiplier = 1f;

        [Networked]private bool isLaunched { get; set; }
        [Networked]private Vector3 start { get; set; }
        [Networked]private Vector3 end { get; set; }
        
        private Vector3 previousPosition;
        private float elapsedTime;

        public override void Spawned()
        {
            // Rigidbody ayarlarını burada güvenli şekilde yap

        }

        public void Launch(Vector3 target)
        {
            start = transform.position;
            end = target;
            elapsedTime = 0f;
            isLaunched = true;
            previousPosition = start;
        }

        public override void FixedUpdateNetwork()
        {
            if (!HasStateAuthority || !isLaunched) return;
            
            elapsedTime += Runner.DeltaTime;
            float t = Mathf.Clamp01(elapsedTime / travelDuration);

            // Lineer pozisyon
            Vector3 linearPos = Vector3.Lerp(start, end, t);

            // Yay eğrisi
            float arc = arcHeightMultiplier * Mathf.Sin(Mathf.PI * t);
            Vector3 heightOffset = Vector3.up * arc;

            Vector3 currentPosition = linearPos + heightOffset;
            transform.position = currentPosition;

            // Yönünü hareket yönüne göre ayarla
            Vector3 moveDirection = (currentPosition - previousPosition).normalized;
            if (moveDirection.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.LookRotation(moveDirection);
            }

            previousPosition = currentPosition;

            if (t >= 1f)
            {
                isLaunched = false;
                // Burada hedefe ulaştıktan sonra bir şeyler yapılabilir
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!HasStateAuthority) return;

            if (other.CompareTag("Plane") || Wepaon == null)
            {
                GetComponent<Collider>().enabled = false; return;
            }
            if (Wepaon.Parent == null) return;
            if (other.CompareTag(Wepaon.GrandParent.tag)) return;
            if (other.TryGetComponent<NpcManager>(out var npc))
            {
                Debug.Log($"Arrow hit: {other.tag}");
                Wepaon.OnTrigger(npc);
                // Ok çarptığında yok et
            }
            Runner.Despawn(Object);
        }
    }
}