using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace BTV.UI.Module3D.Tools
{
    class TfAmplitude : Tool
    {
        public GenericEvent<float, float> UpdateAmplitude = new GenericEvent<float, float>();

        [SerializeField] private RangeSlider m_RangeSlider = null;

        public override void Initialize()
        {
            m_RangeSlider.onValueChanged.AddListener(UpdateAmplitudeValues);
        }

        public void SetAmplitudeWithoutNotify(float min, float max)
        {
            m_RangeSlider.minValue = min;
            m_RangeSlider.maxValue = max;
        }

        public void UpdateAmplitudeValues(float min, float max)
        {
            UpdateAmplitude.Invoke(min, max);
        }
    }
}
