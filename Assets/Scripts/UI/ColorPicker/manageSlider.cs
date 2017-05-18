using UnityEngine;
using UnityEngine.UI;

public class manageSlider : MonoBehaviour
{
    public GameObject colorSlider = null;
    Scrollbar scrollbar = null;
    InputField inputfield = null;

    int memoryScrollBar = 0;
    int memoryInputField = 0;

    // Use this for initialization
    void Start ()
    {
        scrollbar = colorSlider.transform.GetChild(1).GetComponent<Scrollbar>();
        inputfield = colorSlider.transform.GetChild(2).GetComponent<InputField>();

        inputfield.text = (scrollbar.value * 255).ToString();
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Return) || Input.GetKeyUp(KeyCode.KeypadEnter))
        {
            checkValueInputField();
        }
    }

    public void checkValueScrollBar()
    {
        int currentValueScrollBar = (int)(scrollbar.value * 255);
        if (memoryScrollBar != currentValueScrollBar)
        {
            memoryScrollBar = currentValueScrollBar;
            inputfield.text = currentValueScrollBar.ToString();
        }
    }

    public void checkValueInputField()
    {
        int currentValueInputField = int.Parse(inputfield.text);
        checkOverFlow(currentValueInputField);
        if (memoryInputField != currentValueInputField)
        {
            memoryInputField = currentValueInputField;
            scrollbar.value = ((float)currentValueInputField / 255);
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
