using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextToggle : MonoBehaviour
{
    public bool HasFocus
    {
        get { return m_Toggle.isOn; }
        set { m_Toggle.isOn = value; }
    }
    public string Label
    {
        get { return m_UnselectedText.text; }
        set { m_UnselectedText.text = value; m_SelectedText.text = value; }
    }
    public bool IsVisible
    {
        get { return gameObject.activeSelf; }
        set { gameObject.SetActive(value); }
    }

    [SerializeField]
    private Toggle m_Toggle = null;
    [SerializeField]
    private Text m_UnselectedText = null;
    [SerializeField]
    private Text m_SelectedText = null;
}
