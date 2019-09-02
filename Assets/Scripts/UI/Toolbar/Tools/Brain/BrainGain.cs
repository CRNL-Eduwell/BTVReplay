using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BTV.UI.Module3D.Tools
{
    public delegate void BrainGainChangedEventHandler(int UpdatedGain);

    public class BrainGain : Tool
    {
        public event BrainGainChangedEventHandler gainHasChanged;

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
        private int m_Gain = 1;

        #region Public Methods
        public override void Initialize()
        {
            //UnityEngine.Debug.Log("Init brain gain");
            m_Label.text = "Gain : " + m_Gain;
            m_AddGain.onClick.AddListener(() =>
            {
                m_Gain += 1;
                m_Label.text = "Gain : " + m_Gain;
                gainHasChanged(m_Gain);

            });
            m_RemoveGain.onClick.AddListener(() =>
            {
                if (m_Gain - 1 > 0)
                {
                    m_Gain -= 1;
                    m_Label.text = "Gain : " + m_Gain;
                    gainHasChanged(m_Gain);
                }
            });
        }
        #endregion
    }
}