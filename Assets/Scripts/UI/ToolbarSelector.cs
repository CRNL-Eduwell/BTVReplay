using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BTV.UI.Module3D
{
    class ToolbarSelector : MonoBehaviour
    {
        [SerializeField] BTVMedia media = null;

        /// <summary>
        /// Toolbar menu
        /// </summary>
        [SerializeField]
        private ToolbarMenu m_ToolbarMenu;

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
                if (UiOptionIndex > 0) //isOn
                {
                    ChangeToolbar(m_BrainToggle);
                }
            });
            m_Eeg1Toggle.UpdateUiAndModuleLayout.AddListener((UiOptionIndex) =>
            {
                if (UiOptionIndex > 0) //isOn
                {
                    ChangeToolbar(m_Eeg1Toggle);
                }
            });
            m_Eeg2Toggle.UpdateUiAndModuleLayout.AddListener((UiOptionIndex) =>
            {
                if (UiOptionIndex > 0) //isOn
                {
                    ChangeToolbar(m_Eeg2Toggle);
                }
            });
            m_PerformanceToggle.UpdateUiAndModuleLayout.AddListener((UiOptionIndex) =>
            {
                if (UiOptionIndex > 0) //isOn
                {
                    ChangeToolbar(m_PerformanceToggle);
                }
            });
            m_VideoToggle.UpdateUiAndModuleLayout.AddListener((UiOptionIndex) =>
            {
                if (UiOptionIndex > 0) //isOn
                {
                    ChangeToolbar(m_VideoToggle);
                }
            });
            m_EventsToggle.UpdateUiAndModuleLayout.AddListener((UiOptionIndex) =>
            {
                if (UiOptionIndex > 0) //isOn
                {
                    ChangeToolbar(m_EventsToggle);
                }
            });
        }

        /// <summary>
        /// Method to be called when changing the state of a toggle
        /// </summary>
        /// <param name="triggeredToggle">Toggle which value changed</param>
        private void ChangeToolbar(ExtendedToggle triggeredToggle)
        {
            //m_ToolbarMenu.CurrentToolbar.HideToolbarCallback();
            m_ToolbarMenu.CurrentToolbar.gameObject.SetActive(false);
            m_ToolbarMenu.CurrentToolbar = m_Toolbars[triggeredToggle];
            m_ToolbarMenu.CurrentToolbar.gameObject.SetActive(true);
            //m_ToolbarMenu.CurrentToolbar.ShowToolbarCallback();
        }

        private void UpdateUiModule(int UiOptionModule)
        {
            switch (UiOptionModule)
            {
                case 0:
                    break;
                case 1:
                    break;
                case 2:
                    break;
                case 3:
                    break;
            }
        }
    }
}
