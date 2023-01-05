using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BTV.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class GenericTabWidget : MonoBehaviour
{
    public UnityEvent<int, string> OnTabAdded { get; } = new GenericEvent<int, string>();
    public UnityEvent<string> OnTabClicked { get; } = new GenericEvent<string>();
    public UnityEvent<int, string> OnTabRenamed { get; } = new GenericEvent<int, string>();
    public UnityEvent<int, string> OnTabRemoved { get; } = new GenericEvent<int, string>();

    public int TabCount { get { return m_Buttons.Count; } }

    [SerializeField] private Transform _HeaderTabs = null;
    [SerializeField] private Button _AddTab = null;
    [SerializeField] private Button _RemoveTab = null;

    private GameObject m_ButtonPrefab = null;
    private List<ExtendedButton> m_Buttons = null;
    private Color m_normalColor = new Color(0.203921f, 0.203921f, 0.203921f, 1); //52
    private Color m_selectedColor = new Color(0.125490f, 0.125490f, 0.125490f, 1); //32

    private void Awake()
    {
        m_ButtonPrefab = Resources.Load("Prefabs/ExamLabel", typeof(GameObject)) as GameObject;

        m_Buttons = new List<ExtendedButton>();

        _AddTab.onClick.AddListener(() => { AddTab(); });
        _RemoveTab.onClick.AddListener(RemoveSelectedTab);
    }

    private void OnDestroy()
    {
        for (int i = 0; i < m_Buttons.Count; i++)
        {
            m_Buttons[i].OnSingleClick.RemoveAllListeners();
            m_Buttons[i].OnDoubleClick.RemoveAllListeners();
        }
        _AddTab.onClick.RemoveAllListeners();
        _RemoveTab.onClick.RemoveAllListeners();
    }

    private void ConnectButton(int ID)
    {
        m_Buttons[ID].OnSingleClick.AddListener(() => { SwitchTo(ID); });
        m_Buttons[ID].OnDoubleClick.AddListener(() =>
        {
            InputFieldWindow window = ApplicationState.SpawFrequencyChoiceWindow();
            window.Initialize("Label", "Choose a new label for you tab",
                () =>
                {
                    SetTabName(window.StringValue, ID);
                    OnTabRenamed.Invoke(ID, window.StringValue);
                    window.Close();
                }, () =>
                {
                    window.Close();
                });
            window.StringValue = m_Buttons[ID].Text;
        });
    }

    private void SwitchTo(int ID)
    {
        for (int i = 0; i < m_Buttons.Count; i++)
        {
            m_Buttons[i].SetColor(m_normalColor);
        }
        m_Buttons[ID].SetColor(m_selectedColor);

        OnTabClicked.Invoke(m_Buttons[ID].Text);
    }

    public void SetTabs(List<string> names, int index)
    {
        RemoveAllTabs();
        for (int i = 0; i < names.Count; i++)
        {
            AddTab(names[i], false);
        }
        m_Buttons[index].SetColor(m_selectedColor);
    }

    public void SetTabName(string name, int index)
    {
        m_Buttons[index].SetTabText(name);
    }

    public void AddTab(string name = "", bool sendEvent = true)
    {
        ExtendedButton addMe = Instantiate(m_ButtonPrefab, _HeaderTabs.transform).GetComponent<ExtendedButton>();
        addMe.SetColor(m_normalColor);
        addMe.SetTabText(name == "" ? "NEW TEXT" : name);

        m_Buttons.Add(addMe);
        ConnectButton(m_Buttons.Count - 1);

        if(sendEvent) OnTabAdded.Invoke(m_Buttons.Count - 1, addMe.Text);
    }

    public void RemoveSelectedTab()
    {
        for (int i = 0; i < m_Buttons.Count; i++)
        {
            if (m_Buttons[i].Color == m_selectedColor)
            {
                RemoveTab(i, true);
            }
        }
    }

    private void RemoveAllTabs()
    {
        for (int i = m_Buttons.Count - 1; i >= 0; i--)
        {
            RemoveTab(i);
        }
    }

    private void RemoveTab(int tabIndex, bool sendEvent = false)
    {
        ExtendedButton deleteMe = m_Buttons[tabIndex];
        deleteMe.OnSingleClick.RemoveAllListeners();
        deleteMe.OnDoubleClick.RemoveAllListeners();
        m_Buttons.Remove(deleteMe);

        if(sendEvent) OnTabRemoved.Invoke(tabIndex, deleteMe.Text);

        Destroy(deleteMe.gameObject);
    }
}
