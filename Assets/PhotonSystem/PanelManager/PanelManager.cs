using System;
using Fusion;
using PanelSystem.Enums;
using UnityEngine;

namespace PanelSystem
{
    public class PanelManager
    {
        public PanelManager(SerializableDictionary<PanelType, GameObject> OrjinePanels)
        {
            panels = OrjinePanels;
        }
        [SerializeField] public SerializableDictionary<PanelType, GameObject> panels;

        private PanelType currentPanel;
        
        public void OpenPanel(PanelType panelType)
        {
            foreach (var kvp in panels)
            {
                    kvp.Value.SetActive(kvp.Key == panelType);
            }

            currentPanel = panelType;
        }
    }
}