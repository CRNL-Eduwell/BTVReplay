using SFB;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class BrowseWidget : MonoBehaviour
{
    public StringEvent onEndEdit = new StringEvent();

    public bool IsInteractable
    {
        get
        {
            return _BrowseButton.interactable && _InputField.interactable;
        }
        set
        {
            _BrowseButton.interactable = value;
            _InputField.interactable = value;
        }
    }
    public string Text
    {
        get
        {
            return _InputField.text;
        }
        set
        {
            _InputField.text = value;
            m_memory = _InputField.text;
        }
    }
    public string PlaceholderText
    {
        get { return _InputField.placeholder.GetComponent<Text>().text; }
        set { _InputField.placeholder.GetComponent<Text>().text = value; }
    }
    
    [SerializeField]
    private Button _BrowseButton = null;
    [SerializeField]
    private InputField _InputField = null;
    [SerializeField]
    private bool _OpenFile = true;
    [SerializeField]
    private string[] _FileExtensions = null;

    private string m_memory = "";

    private void Awake()
    {
        if (_OpenFile) _BrowseButton.onClick.AddListener(LoadFile);
        else _BrowseButton.onClick.AddListener(SaveFile);

        _InputField.onEndEdit.AddListener((str) => { onEndEdit.Invoke(str); });
    }

    private void OnDestroy()
    {
        _BrowseButton.onClick.RemoveAllListeners();
        _InputField.onEndEdit.RemoveAllListeners();
    }

    public void RevertText()
    {
        _InputField.text = m_memory;
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
