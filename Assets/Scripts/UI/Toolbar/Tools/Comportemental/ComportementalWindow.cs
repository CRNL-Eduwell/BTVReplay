using System;
using UnityEngine;
using UnityEngine.UI;

namespace BTV.UI.Module3D.Tools
{
    public class ComportementalWindow : Tool
    {
        public event timePeriodChangedEventHandler timeHasChanged;

        /// <summary>
        /// </summary>
        [SerializeField]
        private InputField m_WindowSize = null;

        public override void Initialize()
        {
            m_WindowSize.onEndEdit.AddListener(UpdateTimePeriod);
        }

        void UpdateTimePeriod(string UpdatedField)
        {
            int.TryParse(UpdatedField, out int time);
            timeHasChanged(time);
        }
    }
}