using System;
using PlayerSystem;
using UnityEngine;

namespace Signals
{
    public class WinController : MonoBehaviour
    {
        #region Self Variables

        #region Serialized Variables

        [SerializeField] private SerializedDictionary<string, GameObject> finalPanel;
        [SerializeField] private GameObject _disconnect;

        #endregion

        #endregion

        private void OnEnable()
        {
            Subscribe();
        }

        private void Subscribe()
        {
            UISignals.Instance.onFinal += OnFinal;
        }

        public void OnFinal(string final)
        {
            finalPanel[final].SetActive(true);
        }

        public void Disconnect()
        {
            PlayerSignals.Instance.onDisconnect?.Invoke();
        }
    }
}