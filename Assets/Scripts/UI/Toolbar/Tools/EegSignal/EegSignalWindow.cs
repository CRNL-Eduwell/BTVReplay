using System;
using UnityEngine;
using UnityEngine.UI;

namespace BTV.UI.Module3D.Tools
{
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

        public override void Initialize()
        {
            m_ShowTimeGrid.onValueChanged.AddListener((isOn)=> { gridToggled(isOn); });
            m_WindowSize.onEndEdit.AddListener(UpdateTimePeriod);
        }

        void UpdateTimePeriod(string UpdatedField)
        {
            int.TryParse(UpdatedField, out int time);
            timeHasChanged(time);
        }
    }
}
