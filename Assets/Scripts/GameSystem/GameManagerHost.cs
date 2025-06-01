using System.Linq;
using Fusion;
using PlayerSystem;
using PlayerSystem.Controller;
using Signals;
using UnityEngine;

namespace Extentions.GameSystem
{
    public class GameManagerHost : NetworkBehaviour
    {
        #region Self Variables

        #region public Variables

        public bool Start;

        #endregion

        #endregion

        public override void Spawned()
        {
            Subscribe();
            
        }

        private void Subscribe()
        {
            GameSignals.Instance.CheckAllPlayersReady += RPC_CheckAllPlayersReady;
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        public void RPC_CheckAllPlayersReady()
        {
            var players = FindObjectsOfType<PlayerManager>();
            if (players.Length < 2) return;
            if (players.Any(player => !player.IsReady))
            {
                return;
            }
            UISignals.Instance.onReadyActive?.Invoke(false);
            RPC_StartGame();
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPC_StartGame()
        {
            Debug.Log("Tüm oyuncular hazır. Oyun başlatılıyor...");
            var SpawnControl = FindObjectsOfType<MoveAndAligmentController>();
            var players = FindObjectsOfType<PlayerManager>();
            foreach (var VARIABLE in players)
            {
                VARIABLE.RPC_OnSetactive();
            }
            foreach (var player in SpawnControl)
            {
                player.start = true;
            }
            // Örn: runner.LoadScene("GameScene");
        }
    }
}