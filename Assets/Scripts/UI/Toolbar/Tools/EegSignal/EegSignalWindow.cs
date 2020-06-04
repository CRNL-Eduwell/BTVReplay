using System;
using UnityEngine;
using UnityEngine.UI;

namespace BTV.UI.Module3D.Tools
{
    public delegate void timePeriodChangedEventHandler(int UpdatedTime);
    public delegate void toggleGridDisplay(bool IsGridToggled);

    class EegSignalWindow : Tool
    {
        public event timePeriodChangedEventHandler timeHasChanged;
        public event toggleGridDisplay gridToggled;

        /// <summary>
        /// </summary>
        [SerializeField]
        private Toggle m_ShowTimeGrid = null;
        /// <summary>
        /// </summary>
        [SerializeField]
        private InputField m_WindowSize = null;

        private int m_WindowSizeMemory = 10;

        public override void Initialize()
        {
            m_ShowTimeGrid.onValueChanged.AddListener((isOn)=> { gridToggled(isOn); });
            m_WindowSize.onEndEdit.AddListener(UpdateTimePeriod);
        }

        public void SetIsOnWithoutNotifty(bool isOn)
        {
            m_ShowTimeGrid.SetIsOnWithoutNotify(isOn);
        }

        public void SetTimePeriodWithoutNotify(int period)
        {
            if (period > 0)
            {
                m_WindowSizeMemory = period <= 0 ? 10 : period;
                m_WindowSize.text = m_WindowSizeMemory.ToString();
            }
            else
            {
                UpdateTimePeriod("10");
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
            timeHasChanged(time);
        }
    }
}
