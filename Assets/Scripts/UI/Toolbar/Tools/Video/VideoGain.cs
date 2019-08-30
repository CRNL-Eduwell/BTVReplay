using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BTV.UI.Module3D.Tools
{
    public delegate void gainAudioChangedEventHandler(float UpdatedGain);

    public class VideoGain : Tool
    {
        public event gainAudioChangedEventHandler gainAudioHasChanged;

        /// <summary>
        /// </summary>
        [SerializeField]
        private Text m_Label = null;
        /// <summary>
        /// </summary>
        [SerializeField]
        private Button m_AddGain = null;
        /// <summary>
        /// </summary>
        [SerializeField]
        private Button m_RemoveGain = null;
        /// <summary>
        /// </summary>
        private float m_Gain = 1;

        public override void Initialize()
        {
            m_Label.text = "Gain : " + m_Gain;
            m_AddGain.onClick.AddListener(AddGain);
            m_RemoveGain.onClick.AddListener(RemoveGain);
        }

        private void AddGain()
        {
            if (m_Gain < 1 && m_Gain >= -1)
                m_Gain += 0.25f;
            else
                m_Gain += 1;
            m_Label.text = "Gain : " + m_Gain;
            gainAudioHasChanged(m_Gain);
        }

        private void RemoveGain()
        {
            if (m_Gain <= 1 && m_Gain > -1)
                m_Gain -= 0.25f;
            else
                m_Gain -= 1;
            m_Label.text = "Gain : " + m_Gain;
            gainAudioHasChanged(m_Gain);
        }
    }
}