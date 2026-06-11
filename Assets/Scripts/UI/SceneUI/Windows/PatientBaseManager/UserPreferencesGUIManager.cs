using UnityEngine;
using UnityEngine.UI;
using BTV.Services.UserPreferencesService;

public class UserPreferencesGUIManager : MonoBehaviour
{
    [SerializeField] private Button _HeaderClose = null;
    [SerializeField] private FolderSelector _DBPathSelector = null;
    [SerializeField] private Button _Save = null;
    [SerializeField] private Button _Cancel = null;

    private void Awake()
    {
        _HeaderClose.onClick.AddListener(Close);
        _DBPathSelector.Text = UserPreferencesService.UserPreferences.DatabasePreferences.Path;
        _Save.onClick.AddListener(Save);
        _Cancel.onClick.AddListener(Close);
    }

    private void OnDestroy()
    {
        _HeaderClose.onClick.RemoveAllListeners();
        _Save.onClick.RemoveAllListeners();
        _Cancel.onClick.RemoveAllListeners();
    }

    private void Save()
    {
        UserPreferencesService.UserPreferences.DatabasePreferences.Path = _DBPathSelector.Text;
        UserPreferencesService.SavePreferences();
        Close();
    }

    private void Close()
    {
        Destroy(gameObject);
    }
}