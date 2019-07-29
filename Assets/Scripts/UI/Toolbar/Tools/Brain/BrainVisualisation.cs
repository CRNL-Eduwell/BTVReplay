using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace BTV.UI.Module3D.Tools
{
    public class BrainVisualisation : Tool
    {
        public GenericEvent<int> UpdateBrainMeshes = new GenericEvent<int>();

        [SerializeField]
        private Toggle m_leftToggle = null;
        [SerializeField]
        private Toggle m_rightToggle = null;

        #region Public Methods
        public override void Initialize()
        {
            m_leftToggle.onValueChanged.AddListener((isOn) => 
            {
                UpdateBrainVisualisation(m_leftToggle.isOn, m_rightToggle.isOn);
            });
            m_rightToggle.onValueChanged.AddListener((isOn) => 
            {
                UpdateBrainVisualisation(m_leftToggle.isOn, m_rightToggle.isOn);
            });
        }
        #endregion

        private void OnDestroy()
        {
            m_leftToggle.onValueChanged.RemoveAllListeners();
            m_rightToggle.onValueChanged.RemoveAllListeners();
        }

        //-1 : Left Hemisphere
        // 0 : Both Hemisphere
        // 1 : Right Hemisphere
        private void UpdateBrainVisualisation(bool IsLeftOn, bool IsRightOn)
        {
            if (!IsLeftOn && IsRightOn)
            {
                UpdateBrainMeshes.Invoke(-1);
            }
            else if (IsLeftOn && IsRightOn)
            {
                UpdateBrainMeshes.Invoke(0);
            }
            else if (IsLeftOn && !IsRightOn)
            {
                UpdateBrainMeshes.Invoke(1);
            }
        }
    }
}