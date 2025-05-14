using System.Linq;
using Fusion;
using PlayerSystem;
using PlayerSystem.Controller;
using UnityEngine;

namespace Extentions.GameSystem
{
    public class GameManager : NetworkBehaviour
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
            GameSignals.Instance.CheckAllPlayersReady += CheckAllPlayersReady;
        }

        public void CheckAllPlayersReady()
        {
            var players = FindObjectsOfType<PlayerManager>();

            if (players.Any(player => !player.IsReady))
            {
                return;
            }
            
            RPC_StartGame();
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPC_StartGame()
        {
            Debug.Log("Tüm oyuncular hazır. Oyun başlatılıyor...");
            var players = FindObjectsOfType<MoveAndAligmentController>();
            foreach (var player in players)
            {
                player.start = true;
            }
            // Örn: runner.LoadScene("GameScene");
        }
    }
}