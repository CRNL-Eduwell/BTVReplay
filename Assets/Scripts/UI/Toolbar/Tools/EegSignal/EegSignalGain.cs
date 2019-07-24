using UnityEngine;
using UnityEngine.UI;

namespace BTV.UI.Module3D.Tools
{
    class EegSignalGain : Tool
    {
        public event gainChangedEventHandler gainHasChanged;

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
            gainHasChanged(m_Gain);
        }

        void RemoveGain()
        {
            if (m_Gain <= 1 && m_Gain > -1)
                m_Gain -= 0.25f;
            else
                m_Gain -= 1;
            m_Label.text = "Gain : " + m_Gain;
            gainHasChanged(m_Gain);
        }
    }
}
