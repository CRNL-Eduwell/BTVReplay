using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    class ToolbarSelector : MonoBehaviour
    {
        [SerializeField] BTVMedia media = null;

        [SerializeField] BrainToolbar m_BrainToolbar = null;
        [SerializeField] EegSignal m_EegToolbar1 = null;
        [SerializeField] EegSignal m_EegToolbar2 = null;
        [SerializeField] CompPerformanceToolbar m_PerformanceToolbar = null;
        [SerializeField] VideoToolbar m_VideoToolbar = null;
        [SerializeField] EventsToolbar m_EventsToolbar = null;

        private Dictionary<Toggle, Toolbar> m_Toolbars = new Dictionary<Toggle, Toolbar>();

        private void Awake()
        {
            //m_Toolbars.Add(m_ConfigurationToggle, m_ToolbarMenu.ConfigurationToolbar);

            AddListeners();
        }

        /// <summary>
        /// Add the listeners to the toggles (to change the toolbar)
        /// </summary>
        private void AddListeners()
        {
            //m_ConfigurationToggle.onValueChanged.AddListener((isOn) =>
            //{
            //    if (isOn)
            //    {
            //        ChangeToolbar(m_ConfigurationToggle);
            //    }
            //});
        }
    }
}
