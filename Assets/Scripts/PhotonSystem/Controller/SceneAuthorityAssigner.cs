using Fusion;
using UnityEngine;

namespace PhotonSystem.Controller
{
    public class SceneAuthorityAssigner : NetworkBehaviour
    {
        public override void Spawned()
        {
            // Sadece sahibi (host/server) çalıştırır
            if (!HasStateAuthority)
                return;

            Debug.Log("[AuthorityAssigner] Sahne NetworkObject'leri için input authority atama başladı.");

            // Kendine ait NetworkObject referansı
            NetworkObject selfObj = GetComponent<NetworkObject>();
        
            // Sahnedeki tüm NetworkObject'leri bul
            NetworkObject[] allObjects = FindObjectsOfType<NetworkObject>(true);
            foreach (var obj in allObjects)
            {
                // // Kendini atla
                // if (obj == selfObj)
                //     continue;

                // Sadece state authority sahibi (sunucu) atayabilir
                if (!obj.HasStateAuthority)
                    continue;

                // Eğer henüz bir input authority atanmadıysa
                if (obj.InputAuthority == PlayerRef.None)
                {
                    obj.AssignInputAuthority(Runner.LocalPlayer);
                    Debug.Log($"[AuthorityAssigner] {obj.name} için InputAuthority verildi.");
                }
            }

            // İşlem tamamlandı, kendini sahneden kaldır
            Runner.Despawn(selfObj);
            Debug.Log("[AuthorityAssigner] Kendini despawn etti.");
            Debug.Log("[AuthorityAssigner] Kendini despawn etti.");
        }
    }
}