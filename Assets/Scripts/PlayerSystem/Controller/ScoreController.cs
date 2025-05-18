using System;
using Signals;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PlayerSystem.Controller
{
    public class ScoreController : MonoBehaviour
    {
        #region Self Variables

        #region Public Variables

        public Slider ExpBar;

        #endregion

        #region Serialized Variables

        [SerializeField] private TextMeshProUGUI _exp;
        [SerializeField] private TextMeshProUGUI _lwl;

        #endregion

        #region Private Variables

        private float exp;
        private int lwl = 1;

        #endregion

        #endregion
        
        public void TowerEXP(int BotEXP)
        {
            
            exp += BotEXP;
            if (_exp == null || ExpBar == null) return;
            _exp.text = $"{exp}/{10 + 5 * lwl}";
            float maxExp = 10 + 5 * lwl;
            ExpBar.value = Mathf.Clamp01( exp / maxExp);
            

            if (!(exp >= 10 + 5 * lwl)) return;
            lwl++;
            _lwl.text = lwl.ToString();
            CardSignals.Instance.onNextLevel?.Invoke();
            exp = 0;
        }
    }
}