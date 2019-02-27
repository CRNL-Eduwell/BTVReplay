using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public delegate void colorSliderChanged();

public class ColorSlider : MonoBehaviour
{
    public event colorSliderChanged sliderColorChange;

    Text label;
    public Scrollbar scrollbar;
    int memoryScrollBar = 0;
    public InputField inputfield;
    int memoryInputField = 0;

    void Awake ()
    {
        label = transform.GetChild(0).GetComponent<Text>();
        scrollbar = transform.GetChild(1).GetComponent<Scrollbar>();
        inputfield = transform.GetChild(2).GetComponent<InputField>();

        scrollbar.value = ((float)int.Parse(inputfield.text) / 255);
        memoryScrollBar = (int)(scrollbar.value * 255);

        scrollbar.onValueChanged.AddListener(checkValueScrollBar);
        inputfield.onEndEdit.AddListener(checkValueInputField);
    }

    void OnDestroy()
    {
        scrollbar.onValueChanged.RemoveAllListeners();
        inputfield.onEndEdit.RemoveAllListeners();
    }

    void checkValueScrollBar(float scrollbarValue)
    {
        int currentValueScrollBar = (int)(scrollbarValue * 255);
        if (memoryScrollBar != currentValueScrollBar)
        {
            memoryScrollBar = currentValueScrollBar;
            inputfield.text = currentValueScrollBar.ToString();
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
            scrollbar.value = ((float)currentValueInputField / 255);
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
