using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PhotonSystem
{
    public class NetworkObjectRegister : NetworkBehaviour
    {
        #region Self Variables

        #region Private Variables

        [SerializeField] private NetworkRunner _runner;

        #endregion

        #endregion
        
        public override void Spawned()
        {
            base.Spawned();
            Debug.Log("REGİSTER");
            // Sahnedeki aktif NetworkRunner'ı bul
            _runner = FindObjectOfType<NetworkRunner>();
            
            // Sahneyi al
            var scene = SceneManager.GetActiveScene().buildIndex;
            var sceneRef = SceneRef.FromIndex(scene);

            // Sahnedeki tüm NetworkObject'leri bul (istersen filtre de uygulayabilirsin)
            NetworkObject[] sceneObjects = FindObjectsOfType<NetworkObject>(true);

                _runner.RegisterSceneObjects(sceneRef, sceneObjects, default);
                var playerRef = Runner.LocalPlayer;
                foreach (var VARIABLE in sceneObjects)
                {
                    VARIABLE.AssignInputAuthority(playerRef);
                    Debug.Log($"My PlayerRef: {Runner.LocalPlayer}, Object Authority: {VARIABLE.InputAuthority}");
                }
        }
    }
}