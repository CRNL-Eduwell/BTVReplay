using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TabWidget : MonoBehaviour
{
    public BrainDataContainer MNIAnat
    {
        get
        {
            BrainDataContainer anat = m_Panels[0].GetDataContainer();
            if (anat.HasAnat)
                return anat;
            else
                return null;
        }
    }
    public BrainDataContainer PatientAnat
    {
        get
        {
            BrainDataContainer anat = m_Panels[1].GetDataContainer();
            if (anat.HasAnat)
                return anat;
            else
                return null;
        }
    }

    [SerializeField] Transform header = null;
    [SerializeField] Transform content = null;

    private Button[] m_Buttons = null;
    private BrainAnatGUIManager[] m_Panels = null;
    private Color normalColor = new Color(0.203921f, 0.203921f, 0.203921f, 1); //52
    private Color selectedColor = new Color(0.125490f, 0.125490f, 0.125490f, 1); //32

    private void Awake()
    {
        m_Buttons = header.transform.GetComponentsInChildren<Button>();
        m_Panels = content.transform.GetComponentsInChildren<BrainAnatGUIManager>(true);
        for (int i = 0; i < m_Buttons.Length; i++)
            connectButton(i);
    }

    private void Start()
    {
        m_Buttons[0].transform.GetComponent<Image>().color = selectedColor;
    }

    private void OnDestroy()
    {
        for (int i = 0; i < m_Buttons.Length; i++)
        {
            m_Buttons[i].onClick.RemoveAllListeners();
        }
    }

    private void connectButton(int ID)
    {
        m_Buttons[ID].onClick.AddListener(() => 
        {
            switchTo(ID);
        }); 
    }

    private void switchTo(int ID)
    {
        for (int i = 0; i < m_Panels.Length; i++)
        {
            m_Buttons[i].transform.GetComponent<Image>().color = normalColor;
            m_Panels[i].gameObject.SetActive(false);
        }
        m_Buttons[ID].transform.GetComponent<Image>().color = selectedColor;
        m_Panels[ID].gameObject.SetActive(true);
    }
}
