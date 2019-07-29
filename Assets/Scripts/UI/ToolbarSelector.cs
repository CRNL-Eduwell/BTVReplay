using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;

namespace BTV.UI.Module3D
{
    class ToolbarSelector : MonoBehaviour
    {
        [SerializeField] BTVMedia media = null;

        /// <summary>
        /// Toolbar menu
        /// </summary>
        [SerializeField]
        private ToolbarMenu m_ToolbarMenu = null;

        /// <summary>
        /// </summary>
        [SerializeField] ExtendedToggle m_BrainToggle = null;
        [SerializeField] ExtendedToggle m_Eeg1Toggle = null;
        [SerializeField] ExtendedToggle m_Eeg2Toggle = null;
        [SerializeField] ExtendedToggle m_PerformanceToggle = null;
        [SerializeField] ExtendedToggle m_VideoToggle = null;
        [SerializeField] ExtendedToggle m_EventsToggle = null;

        private Dictionary<ExtendedToggle, Toolbar> m_Toolbars = new Dictionary<ExtendedToggle, Toolbar>();

        private void Awake()
        {
            m_Toolbars.Add(m_BrainToggle, m_ToolbarMenu.BrainToolBar);
            m_Toolbars.Add(m_Eeg1Toggle, m_ToolbarMenu.EegSignal1ToolBar);
            m_Toolbars.Add(m_Eeg2Toggle, m_ToolbarMenu.EegSignal2ToolBar);
            m_Toolbars.Add(m_PerformanceToggle, m_ToolbarMenu.PerformanceToolBar);
            m_Toolbars.Add(m_VideoToggle, m_ToolbarMenu.VideoToolBar);
            m_Toolbars.Add(m_EventsToggle, m_ToolbarMenu.EventsToolBar);

            AddListeners();
        }

        /// <summary>
        /// Add the listeners to the toggles (to change the toolbar)
        /// </summary>
        private void AddListeners()
        {
            m_BrainToggle.UpdateUiAndModuleLayout.AddListener((UiOptionIndex) =>
            {
                UnityEngine.Debug.Log("Update Brain Opt : " + UiOptionIndex);
                ChangeToolbar(m_BrainToggle, UiOptionIndex);
            });
            m_Eeg1Toggle.UpdateUiAndModuleLayout.AddListener((UiOptionIndex) =>
            {

            });
            m_Eeg2Toggle.UpdateUiAndModuleLayout.AddListener((UiOptionIndex) =>
            {

            });
            m_PerformanceToggle.UpdateUiAndModuleLayout.AddListener((UiOptionIndex) =>
            {

            });
            m_VideoToggle.UpdateUiAndModuleLayout.AddListener((UiOptionIndex) =>
            {
                UnityEngine.Debug.Log("Update Video Opt : " + UiOptionIndex);
                ChangeToolbar(m_VideoToggle, UiOptionIndex);
            });
            m_EventsToggle.UpdateUiAndModuleLayout.AddListener((UiOptionIndex) =>
            {
                UnityEngine.Debug.Log("Update Events Opt : " + UiOptionIndex);
                ChangeToolbar(m_EventsToggle, UiOptionIndex);
            });
        }

        /// <summary>
        /// Method to be called when changing the state of a toggle
        /// </summary>
        /// <param name="triggeredToggle">Toggle which value changed</param>
        private void ChangeToolbar(ExtendedToggle triggeredToggle, int UiOptionModule)
        {
            if (m_ToolbarMenu.CurrentToolbar == null)
                m_ToolbarMenu.CurrentToolbar = m_Toolbars[triggeredToggle];

            m_ToolbarMenu.CurrentToolbar.gameObject.SetActive(false);
            KeyValuePair<ExtendedToggle, Toolbar> previousKeyValuePair = m_Toolbars.First(x => x.Value == m_ToolbarMenu.CurrentToolbar);
            previousKeyValuePair.Key.ResetToggle();

            bool showTriggeredToolbar = UiOptionModule > 1 ? true : false;
            m_ToolbarMenu.CurrentToolbar = m_Toolbars[triggeredToggle];
            m_ToolbarMenu.CurrentToolbar.gameObject.SetActive(showTriggeredToolbar);
        }
    }
}
