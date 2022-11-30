using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace BTV.UI.Module3D.Tools
{
    class TfWindow : Tool
    {
        public GenericEvent<int> UpdateTfWindow = new GenericEvent<int>();

        /// <summary>
        /// </summary>
        [SerializeField]
        private InputField m_WindowSize = null;

        private int m_WindowSizeMemory = 500;

        public override void Initialize()
        {
            m_WindowSize.onEndEdit.AddListener(UpdateTimePeriod);
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
