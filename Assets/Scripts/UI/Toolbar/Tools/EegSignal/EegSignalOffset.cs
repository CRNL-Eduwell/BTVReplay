using UnityEngine;
using UnityEngine.UI;

namespace BTV.UI.Module3D.Tools
{
    public delegate void offsetChangedEventHandler(float newVal);

    class EegSignalOffset : Tool
    {  
        public event offsetChangedEventHandler offsetHasChanged;

        /// <summary>
        /// </summary>
        [SerializeField]
        private Text m_Label = null;
        /// <summary>
        /// </summary>
        [SerializeField]
        private Button m_AddOffset = null;
        /// <summary>
        /// </summary>
        [SerializeField]
        private Button m_RemoveOffset = null;
        /// <summary>
        /// </summary>
        private float m_Offset = 1;

        public override void Initialize()
        {
            m_Label.text = "Offset : " + m_Offset + "%";
            m_AddOffset.onClick.AddListener(AddOffset);
            m_RemoveOffset.onClick.AddListener(RemoveOffset);
        }

        private void AddOffset()
        {
            if (m_Offset + 1 <= 5)
            {
                m_Offset += 1;
                m_Label.text = "Offset : " + (m_Offset * 10) + "%";
                offsetHasChanged(m_Offset);
            }
        }

        void RemoveOffset()
        {
            if (m_Offset - 1 >= -5)
            {
                m_Offset -= 1;
                m_Label.text = "Offset : " + (m_Offset * 10) + "%";
                offsetHasChanged(m_Offset);
            }
        }
    }
}
