using System.Collections;
using System.Collections.Generic;
using BTV.Services.VideoService;
using UnityEngine;
using UnityEngine.UI;

public delegate void ToggleAudioTrace(bool IsAudioChecked);
public delegate void AudioFileIDUpdated(int ID);

namespace BTV.UI.Module3D.Tools
{
    public class VideoAudiotrace : Tool
    {
        public event ToggleAudioTrace ToggleTraceAudio;
        public event AudioFileIDUpdated UpdateAudioFileID;

        /// <summary>
        /// </summary>
        [SerializeField]
        private Toggle m_ShowTrace = null;
        /// <summary>
        /// </summary>
        [SerializeField]
        private Dropdown m_FileDropDown = null;
        /// <summary>
        /// </summary>
        private int[] m_WindowSmoothinginMs = { 0, 250, 500, 1000, 2500, 5000 };

        protected override void OnInitialize()
        {
            m_FileDropDown.options.Clear();
            VideoService.SubscribeAudioDataLoaded(PatientSession, LoadDropDownData);
            m_ShowTrace.onValueChanged.AddListener((bool isChecked) => { ToggleTraceAudio?.Invoke(isChecked); });
        }

        private void OnDestroy()
        {
            m_ShowTrace.onValueChanged.RemoveAllListeners();
            if (PatientSession != null)
                VideoService.UnsubscribeAudioDataLoaded(PatientSession, LoadDropDownData);
            m_FileDropDown.onValueChanged.RemoveAllListeners();
        }

        private void LoadDropDownData()
        {
            m_FileDropDown.options.Clear();
            for (int i = 0; i < 6; i++)
            {
                string Label = "SM " + m_WindowSmoothinginMs[i];
                m_FileDropDown.options.Add(new Dropdown.OptionData(Label));
            }
            m_FileDropDown.onValueChanged.AddListener((value) => UpdateAudioFileID?.Invoke(value));

            m_ShowTrace.isOn = true;
            m_FileDropDown.captionText.text = m_FileDropDown.options[m_FileDropDown.value].text;
            m_FileDropDown.onValueChanged.Invoke(0);
        }
    }
}
