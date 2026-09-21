using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace BTV.UI.Module3D.Tools
{
    public class TfTraceToggler : Tool
    {
        public GenericEvent<int> SelectedTrace = new GenericEvent<int>();

        /// <summary>
        /// </summary>
        [SerializeField]
        private Toggle m_FirstToggle = null;
        /// <summary>
        /// </summary>
        [SerializeField]
        private Toggle m_SecondToggle = null;

        protected override void OnInitialize()
        {
            //plug only one of the toggle since they are in a toggle group it will automatically trigger the other
            m_FirstToggle.onValueChanged.AddListener((isOn) =>
            {
                BtvLog.Log("First is " + isOn);
                SelectedTrace.Invoke(isOn ? 0 : 1);
            });
        }
    }
}
