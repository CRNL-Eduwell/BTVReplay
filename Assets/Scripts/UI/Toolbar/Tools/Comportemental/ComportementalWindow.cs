using System;
using UnityEngine;
using UnityEngine.UI;

namespace BTV.UI.Module3D.Tools
{
    public delegate void UpdateTimeComportmentWindow(int UpdatedTime);

    public class ComportementalWindow : Tool
    {
        public event UpdateTimeComportmentWindow UpdateTime;

        /// <summary>
        /// </summary>
        [SerializeField]
        private InputField m_WindowSize = null;

        private int m_WindowSizeMemory = 10;

        protected override void OnInitialize()
        {
            m_WindowSize.onEndEdit.AddListener(UpdateTimePeriod);
        }

        void UpdateTimePeriod(string UpdatedField)
        {
            int.TryParse(UpdatedField, out int time);
            if (time <= 0)
            {
                time = m_WindowSizeMemory;
                m_WindowSize.text = time.ToString();
                return;
            }
            m_WindowSizeMemory = time;
            UpdateTime?.Invoke(time);
        }
    }
}
