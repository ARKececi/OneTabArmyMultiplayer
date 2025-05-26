using System;
using Extentions.GameSystem;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;


namespace PlayerSystem.Controller
{
    public class TowerController : NetworkBehaviour
    {
        #region Self Variables

        #region Public Variables
        [Networked] public Color ColorToApply { set; get; }
        [Networked] public int healt { get; set; }
        [Networked] public NetworkObject Parent { set; get; }
        [Networked] public Vector3 CamTransform { get; set; }

        #endregion

        #region Serialized Variables

        [SerializeField] private GameObject HealtObj;

        [SerializeField] private Image HealtBar;
        [SerializeField] public Image SpawnBar;
        [SerializeField] private TextMeshProUGUI _healtText;


        #endregion

        #region Private Variables

        [Networked] public int maxHealt { get; set; }

        #endregion

        #endregion

        private void Start()
        {
            if(HasInputAuthority)
                HealtObj.GetComponentInChildren<Canvas>().transform.rotation = Quaternion.Euler(45,0,0);
        }
    
        public override void Spawned()
        {
            MeshRenderer[] mesh = GetComponentsInChildren<MeshRenderer>();
            foreach (var VARIABLE in mesh)
            {
                VARIABLE.material.color = ColorToApply;
            }
            Object.transform.SetParent(Parent.transform);
            _healtText.text = healt.ToString();
            maxHealt = healt;

            Subscribe();
        }

        private void Subscribe()
        {
            PlayerSignals.Instance.SpawnBar += RPC_SpawnSlider;
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_SpawnSlider(PlayerRef playerRef,float timer, float maxTimer)
        {
            if(Object.InputAuthority != playerRef) return;
            SpawnBar.fillAmount = Mathf.Clamp01( timer / maxTimer);
        }
        
        public void OnSetDamage(int damage)
        {
            if(HasStateAuthority)
                RPC_OnSetDamage(damage);
        }
        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_OnSetDamage(int damage)
        {
            if (healt <= 0) return; // zaten ölü
            healt -= damage;
            _healtText.text = healt.ToString();
            float fHealt = healt;
            float fMaxHealt = maxHealt;
            HealtBar.fillAmount = Mathf.Clamp01( fHealt / fMaxHealt);
            Debug.Log(maxHealt);
            if (healt > 0) return;
            Debug.Log("bitti");
            PlayerRef @ref = Object.InputAuthority;
            GameSignals.Instance.onFinish?.Invoke(@ref);
            GameSignals.Instance.onGame?.Invoke(false);
            
        }
    }
}