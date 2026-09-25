using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace BTV.UI.Module3D.Tools
{
    class TfWindow : Tool
    {
        public GenericEvent<int, int> UpdateFrequencyBand = new GenericEvent<int, int>();
        public GenericEvent<int> UpdateTfWindow = new GenericEvent<int>();

        /// <summary>
        /// </summary>
        [SerializeField]
        private InputField m_HighFrequency = null;

        /// <summary>
        /// </summary>
        [SerializeField]
        private InputField m_LowFrequency = null;

        /// <summary>
        /// </summary>
        [SerializeField]
        private InputField m_WindowSize = null;

        private int m_WindowSizeMemory = 500;
        private int m_LowFrequencyMemory = 0;
        private int m_HighFrequencyMemory = 256;

        protected override void OnInitialize()
        {
            m_LowFrequency.onEndEdit.AddListener(UpdateLowFrequency);
            m_HighFrequency.onEndEdit.AddListener(UpdateHighFrequency);
            m_WindowSize.onEndEdit.AddListener(UpdateTimePeriod);
        }

        public void SetFrequencyBandWithoutNotify(int low, int high)
        {
            m_LowFrequencyMemory = low;
            m_HighFrequencyMemory = high;
            m_LowFrequency.text = m_LowFrequencyMemory.ToString();
            m_HighFrequency.text = m_HighFrequencyMemory.ToString();
        }

        public void SetTimePeriodWithoutNotify(int period)
        {
            if (period > 0)
            {
                m_WindowSizeMemory = period;
                m_WindowSize.text = m_WindowSizeMemory.ToString();
            }
            else
            {
                UpdateTimePeriod("500");
            }
        }

        private void UpdateLowFrequency(string UpdatedField)
        {
            if (int.TryParse(UpdatedField, out int time))
            {
                if (time >= 0 && time < m_HighFrequencyMemory)
                {
                    m_LowFrequencyMemory = time;
                    UpdateFrequencyBand.Invoke(m_LowFrequencyMemory, m_HighFrequencyMemory);
                }
                else
                {
                    m_LowFrequency.text = m_LowFrequencyMemory.ToString();
                }
            }
            else
            {
                m_LowFrequency.text = m_LowFrequencyMemory.ToString();
            }
        }

        private void UpdateHighFrequency(string UpdatedField)
        {
            if (int.TryParse(UpdatedField, out int time))
            {
                if (time > m_LowFrequencyMemory && time <= 256) //see for maybe a high frequency threshold later
                {
                    m_HighFrequencyMemory = time;
                    UpdateFrequencyBand.Invoke(m_LowFrequencyMemory, m_HighFrequencyMemory);
                }
            }
            else
            {
                m_HighFrequency.text = m_HighFrequencyMemory.ToString();
            }
        }

        private void UpdateTimePeriod(string UpdatedField)
        {
            int.TryParse(UpdatedField, out int time);
            if (time <= 0)
            {
                time = m_WindowSizeMemory;
                m_WindowSize.text = time.ToString();
                return;
            }
            m_WindowSizeMemory = time;
            UpdateTfWindow.Invoke(time);
        }
    }
}
