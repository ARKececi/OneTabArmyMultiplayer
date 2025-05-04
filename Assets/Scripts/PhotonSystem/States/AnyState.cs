using System.Collections.Generic;
using Fusion;
using UnityEngine;

namespace PhotonSystem.States
{
    public class AnyState: IPhotonBaseState
    {
        #region Self Variables

        #region Private Variables

        private PhotonManager _photonManager;

        #endregion

        #endregion
        public void EnterState(PhotonManager photonManager)
        {
            _photonManager = photonManager;
            Reset();
            Debug.Log("İşlem yok");
        }

        public void ExitState(PhotonManager photonManager)
        {
            Debug.Log("İşlem Başlatıldı");
        }
        
        private void Reset()
        {
            if (_photonManager.ActiveRunner == null) return;
            if (_photonManager.ActiveRunner .IsRunning)
            {
                _photonManager.ActiveRunner .Shutdown(); // bu async değilse beklemene gerek yok
            }

            Object.Destroy(_photonManager.ActiveRunner .gameObject);
            _photonManager.ActiveRunner  = null;
            _photonManager.SessionList = null;
            _photonManager.SessionName = null;
            Debug.Log("[Fusion] Runner temizlendi.");
        }
    }
}