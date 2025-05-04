using Fusion;
using PanelSystem.Enums;
using UnityEngine;

namespace PhotonSystem.PanelManager
{
    public class PanelManager
    {
        public PanelManager(SerializableDictionary<PanelType, GameObject> OrjinePanels)
        {
            panels = OrjinePanels;
        }

        public SerializableDictionary<PanelType, GameObject> panels;

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