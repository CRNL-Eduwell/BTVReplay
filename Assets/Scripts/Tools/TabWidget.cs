using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TabWidget : MonoBehaviour
{
    [SerializeField] private List<Button> _Tabs = new List<Button>();
    [SerializeField] private List<GameObject> _Contents = new List<GameObject>();

    private Color m_DefaultColor = new Color(0.203921f, 0.203921f, 0.203921f, 1); //52
    private Color m_SelectedColor = new Color(0.203921f, 0.505882f, 0.760784f);

    private void Awake()
    {
        //check same number of tabs and contents maybe ?
        for (int i = 0; i < _Tabs.Count; i++)
        {
            ConnectButton(i);
        }
        SwitchTo(0);
    }

    private void OnDestroy()
    {
        for (int i = 0; i < _Tabs.Count; i++)
        {
            _Tabs[i].onClick.RemoveAllListeners();
        }
    }

    private void ConnectButton(int ID)
    {
        _Tabs[ID].onClick.AddListener(() => { SwitchTo(ID); });
    }

    private void SwitchTo(int ID)
    {
        for (int i = 0; i < _Contents.Count; i++)
        {
            _Tabs[i].transform.GetComponent<Image>().color = m_DefaultColor;
            _Contents[i].gameObject.SetActive(false);
        }
        _Tabs[ID].transform.GetComponent<Image>().color = m_SelectedColor;
        _Contents[ID].gameObject.SetActive(true);
    }
}
