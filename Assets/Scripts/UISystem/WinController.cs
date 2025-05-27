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
        [SerializeField] private GameObject _ready;

        #endregion

        #endregion

        private void OnEnable()
        {
            Subscribe();
        }

        private void Subscribe()
        {
            UISignals.Instance.onFinal += OnFinal;
            UISignals.Instance.onReadyActive += OnReadyActive;
        }

        public void OnFinal(string final)
        {
            finalPanel[final].SetActive(true);
        }

        public void Disconnect()
        {
            PlayerSignals.Instance.onDisconnect?.Invoke();
        }

        public void OnReadyActive(bool ready)
        {
            _ready.SetActive(ready);
        }

        public void OnReady()
        {
            PlayerSignals.Instance.onSetReady?.Invoke();
        }
    }
}