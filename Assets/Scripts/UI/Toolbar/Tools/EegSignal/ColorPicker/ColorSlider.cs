using UnityEngine;
using UnityEngine.UI;

namespace BTV.UI.Module3D.Tools
{
    public delegate void colorSliderChanged();

    public class ColorSlider : MonoBehaviour
    {
        public event colorSliderChanged sliderColorChange;

        public float ColorValue
        {
            get
            {
                return m_Scrollbar.value;
            }
            set
            {
                m_Scrollbar.value = value;
            }
        }

        [SerializeField]
        private Scrollbar m_Scrollbar = null;
        [SerializeField]
        private InputField m_Inputfield = null;

        int memoryScrollBar = 0;
        int memoryInputField = 0;

        public void Init()
        {
            m_Scrollbar.value = ((float)int.Parse(m_Inputfield.text) / 255);
            memoryScrollBar = (int)(m_Scrollbar.value * 255);

            m_Scrollbar.onValueChanged.AddListener(checkValueScrollBar);
            m_Inputfield.onEndEdit.AddListener(checkValueInputField);
        }

        void OnDestroy()
        {
            m_Scrollbar.onValueChanged.RemoveAllListeners();
            m_Inputfield.onEndEdit.RemoveAllListeners();
        }

        void checkValueScrollBar(float scrollbarValue)
        {
            int currentValueScrollBar = (int)(scrollbarValue * 255);
            if (memoryScrollBar != currentValueScrollBar)
            {
                memoryScrollBar = currentValueScrollBar;
                m_Inputfield.text = currentValueScrollBar.ToString();
                sliderColorChange();
            }
        }

        void checkValueInputField(string inputfieldValue)
        {
            int currentValueInputField = int.Parse(inputfieldValue);
            checkOverFlow(currentValueInputField);
            if (memoryInputField != currentValueInputField)
            {
                memoryInputField = currentValueInputField;
                m_Scrollbar.value = ((float)currentValueInputField / 255);
                sliderColorChange();
            }
        }

        void checkOverFlow(int colorValue)
        {
            if (colorValue > 255)
            {
                colorValue = 255;
            }
            else if (colorValue < 0)
            {
                colorValue = 0;
            }
        }
    }
}
