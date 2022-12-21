using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class GenericTabWidget : MonoBehaviour
{
    public UnityEvent<string> OnTabClicked { get; } = new GenericEvent<string>();

    public int TabCount { get { return m_Buttons.Length; } }

    [SerializeField] private Transform _HeaderTabs = null;

    private Button[] m_Buttons = null;
    private Color m_normalColor = new Color(0.203921f, 0.203921f, 0.203921f, 1); //52
    private Color m_selectedColor = new Color(0.125490f, 0.125490f, 0.125490f, 1); //32

    private void Awake()
    {
        m_Buttons = _HeaderTabs.transform.GetComponentsInChildren<Button>();
        for (int i = 0; i < m_Buttons.Length; i++)
        {
            ConnectButton(i);
        }
    }

    private void OnDestroy()
    {
        for (int i = 0; i < m_Buttons.Length; i++)
        {
            m_Buttons[i].onClick.RemoveAllListeners();
        }
    }

    private void ConnectButton(int ID)
    {
        m_Buttons[ID].onClick.AddListener(() => { SwitchTo(ID); });
    }

    private void SwitchTo(int ID)
    {
        for (int i = 0; i < m_Buttons.Length; i++)
        {
            m_Buttons[i].transform.GetComponent<Image>().color = m_normalColor;
        }
        m_Buttons[ID].transform.GetComponent<Image>().color = m_selectedColor;

        Text t = m_Buttons[ID].transform.GetChild(0).GetComponent<Text>();
        OnTabClicked.Invoke(t.text);
    }

    public void SetTabName(string name, int index)
    {
        Text t = m_Buttons[index].transform.GetChild(0).GetComponent<Text>();
        t.text = name;
    }
}
