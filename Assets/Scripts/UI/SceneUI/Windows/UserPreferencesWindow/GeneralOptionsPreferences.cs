using BTV.Services.UserPreferencesService;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GeneralOptionsPreferences : MonoBehaviour
{
    [SerializeField] private Button _HeaderClose = null;
    [SerializeField] private GameObject _TabSelectorRoot = null;
    [SerializeField] private GameObject _TabContentRoot = null;
    [SerializeField] private Button _Save = null;
    [SerializeField] private Button _Cancel = null;

    private List<KeyValuePair<Toggle, GameObject>> m_GraphicalElements = null;
    private GeneralPreferences m_Preferences = null;

    void Start()
    {
        m_Preferences = new GeneralPreferences(UserPreferencesService.UserPreferences.GeneralPreferences);

        _HeaderClose.onClick.AddListener(Close);

        int elements = _TabSelectorRoot.transform.childCount;
        m_GraphicalElements = new List<KeyValuePair<Toggle, GameObject>>();
        for (int i = 0; i < elements; i++)
        {
            Toggle toggle = _TabSelectorRoot.transform.GetChild(i).GetComponent<Toggle>();
            Transform trm = _TabContentRoot.transform.GetChild(i);

            toggle.onValueChanged.AddListener((bool isVisible) => { trm.gameObject.SetActive(isVisible); });
            m_GraphicalElements.Add(new KeyValuePair<Toggle, GameObject>(toggle, trm.gameObject));

            switch (i)
            {
                case 0:
                    {
                        FolderSelector fs = trm.GetChild(1).GetChild(1).GetComponent<FolderSelector>();
                        fs.Text = m_Preferences.ExportPath;
                        break;
                    }
                case 1:
                    {
                        FolderSelector fs = trm.GetChild(1).GetChild(1).GetComponent<FolderSelector>();
                        fs.Text = m_Preferences.VlcPath;
                        break;
                    }
            }
        }


        _Save.onClick.AddListener(Save);
        _Cancel.onClick.AddListener(Close);
    }

    private void OnDestroy()
    {
        _HeaderClose.onClick.RemoveAllListeners();
        foreach (KeyValuePair<Toggle, GameObject> kvp in m_GraphicalElements)
        {
            kvp.Key.onValueChanged.RemoveAllListeners();
        }
        _Save.onClick.RemoveAllListeners();
        _Cancel.onClick.RemoveAllListeners();
    }

    private void Save()
    {
        int count = 0;
        foreach (KeyValuePair<Toggle, GameObject> kvp in m_GraphicalElements)
        {
            switch (count)
            {
                case 0:
                    {
                        FolderSelector fs = kvp.Value.transform.GetChild(1).GetChild(1).GetComponent<FolderSelector>();
                        m_Preferences.ExportPath = fs.Text;
                        break;
                    }
                case 1:
                    {
                        FolderSelector fs = kvp.Value.transform.GetChild(1).GetChild(1).GetComponent<FolderSelector>();
                        m_Preferences.VlcPath = fs.Text;
                        break;
                    }
            }
            count++;
        }

        UserPreferencesService.UserPreferences.GeneralPreferences = m_Preferences;
        if (!UserPreferencesService.SavePreferences(out string error))
            ApplicationState.displayMessage("Preferences not saved", "NOK", error);
        Close();
    }

    private void Close()
    {
        Destroy(gameObject);
    }
}
