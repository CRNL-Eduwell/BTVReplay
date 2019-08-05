using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public delegate void FilterAudio();
public delegate void LoadAudio();

namespace BTV.UI.Module3D.Tools
{
    public class VideoAudiotrace : Tool
    {
        public event toggleAudioTraceEventHandler audioToggled;
        public event FilterAudio StartAudioFilter;
        public event LoadAudio StartAudioLoad;
        public event idAudioSmChangedEventHandler smAudioHasChanged;

        /// <summary>
        /// </summary>
        [SerializeField]
        private Toggle m_ShowTrace = null;

        /// <summary>
        /// </summary>
        [SerializeField]
        private Button m_FilterAudio = null;

        /// <summary>
        /// </summary>
        [SerializeField]
        private Button m_LoadTrace = null;

        /// <summary>
        /// </summary>
        [SerializeField]
        private Dropdown m_FileDropDown = null;

        public override void Initialize()
        {
            m_FileDropDown.options.Clear();
            for (int i = 0; i < 6; i++)
            {
                m_FileDropDown.options.Add(new Dropdown.OptionData("File " + i));
            }

            m_ShowTrace.onValueChanged.AddListener((bool isChecked) =>
            {
                audioToggled(isChecked);
            });
            m_FilterAudio.onClick.AddListener(FilterAudio);
            m_LoadTrace.onClick.AddListener(LoadAudio);
            m_FileDropDown.onValueChanged.AddListener(UpdateFile);
        }

        private void FilterAudio()
        {
            m_FilterAudio.interactable = false;
            m_LoadTrace.interactable = false;
            m_ShowTrace.isOn = true;
            StartAudioFilter();
        }
        
        private void LoadAudio()
        {
            m_FilterAudio.interactable = false;
            m_LoadTrace.interactable = false;
            m_ShowTrace.isOn = true;
            StartAudioLoad();
        }

        private void UpdateFile(int ID)
        {
            smAudioHasChanged(ID);
        }
    }
}