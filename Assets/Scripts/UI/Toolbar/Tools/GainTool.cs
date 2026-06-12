using UnityEngine;
using UnityEngine.UI;

namespace BTV.UI.Module3D.Tools
{
    public delegate void gainChangedEventHandler(float newVal);

    /// <summary>
    /// +/- gain stepper shared by the EEG signal and video toolbars (used to exist as two
    /// near-identical copies). Fine 0.25 steps inside [-1, 1], whole steps beyond.
    /// </summary>
    public abstract class GainTool : Tool
    {
        public event gainChangedEventHandler gainHasChanged;

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
        private float m_Gain = 1;

        public override void Initialize()
        {
            RefreshLabel();
            m_AddGain.onClick.AddListener(AddGain);
            m_RemoveGain.onClick.AddListener(RemoveGain);
        }

        public void SetGainWithoutNotify(float gain)
        {
            m_Gain = gain;
            RefreshLabel();
        }

        public static float NextGain(float gain)
        {
            if (gain < 1 && gain >= -1)
                return gain + 0.25f;
            return gain + 1;
        }

        public static float PreviousGain(float gain)
        {
            if (gain <= 1 && gain > -1)
                return gain - 0.25f;
            return gain - 1;
        }

        private void AddGain()
        {
            m_Gain = NextGain(m_Gain);
            RefreshLabel();
            gainHasChanged?.Invoke(m_Gain);
        }

        private void RemoveGain()
        {
            m_Gain = PreviousGain(m_Gain);
            RefreshLabel();
            gainHasChanged?.Invoke(m_Gain);
        }

        private void RefreshLabel()
        {
            m_Label.text = "Gain : " + m_Gain;
        }
    }
}
