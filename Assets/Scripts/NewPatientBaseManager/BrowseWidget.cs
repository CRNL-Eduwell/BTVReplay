using SFB;
using UnityEngine;
using UnityEngine.UI;

public class BrowseWidget : MonoBehaviour
{
    [SerializeField]
    private Button _BrowseButton = null;
    [SerializeField]
    public InputField _InputField = null;
    [SerializeField]
    private bool _OpenFile = true;
    [SerializeField]
    private string[] _FileExtensions = null;

    private void Awake()
    {
        if (_OpenFile)
            _BrowseButton.onClick.AddListener(LoadFile);
        else
            _BrowseButton.onClick.AddListener(SaveFile);
    }

    private void OnDestroy()
    {
        _BrowseButton.onClick.RemoveAllListeners();
    }

    private void LoadFile()
    {
        _InputField.text = FileBrowser.GetExistingFileName(_FileExtensions);
    }

    private void SaveFile()
    {
        var extensionList = new[] { new ExtensionFilter("Files", _FileExtensions) };
        _InputField.text = FileBrowser.GetSavedFileName(extensionList);
    }
}
