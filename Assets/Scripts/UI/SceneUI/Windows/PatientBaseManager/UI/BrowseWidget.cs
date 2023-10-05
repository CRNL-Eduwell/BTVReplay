using SFB;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class BrowseWidget : MonoBehaviour
{
    public UnityEvent<string> TextUpdated { get; } = new GenericEvent<string>();

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
            CheckEegFileInput(_InputField.text);
            m_memory = _InputField.text;
        }
    }
    public string TextWithoutPopUp
    {
        get
        {
            return _InputField.text;
        }
        set
        {
            _InputField.text = value;
            CheckEegFileInput(_InputField.text, false);
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
    private Image _IsOk = null;
    [SerializeField]
    private bool _OpenFile = true;
    [SerializeField]
    private bool _ShowIndicator = true;
    [SerializeField]
    private string[] _FileExtensions = null;

    private string m_memory = "";
    private Sprite m_IsOkFile = null, m_IsErrorFile = null;
    private Color m_ShowColor = new Color(1, 1, 1, 1), m_HideColor = new Color(1, 1, 1, 0);

    private void Awake()
    {
        m_IsOkFile = Resources.Load("Pictures/Ok_File", typeof(Sprite)) as Sprite;
        m_IsErrorFile = Resources.Load("Pictures/Error_File", typeof(Sprite)) as Sprite;

        if (_OpenFile) _BrowseButton.onClick.AddListener(LoadFile);
        else _BrowseButton.onClick.AddListener(SaveFile);

        _InputField.onEndEdit.AddListener((str)=> { CheckEegFileInput(str); });
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
#if UNITY_STANDALONE_OSX
        FileBrowser.GetExistingFileNameAsync((str) =>
        {
            if (!string.IsNullOrEmpty(str))
            {
                Text = str;
            }
        }, _FileExtensions);
#else
        string str = FileBrowser.GetExistingFileName(_FileExtensions);
        if (!string.IsNullOrEmpty(str))
        {
            Text = str;
        }
#endif
    }

    private void SaveFile()
    {
#if UNITY_STANDALONE_OSX
        var extensionList = new[] { new ExtensionFilter("Files", _FileExtensions) };
        FileBrowser.GetSavedFileNameAsync((str) =>
        {
            _InputField.text = str;
        }, extensionList);
#else
        var extensionList = new[] { new ExtensionFilter("Files", _FileExtensions) };
        _InputField.text = FileBrowser.GetSavedFileName(extensionList);
#endif
    }

    /// <summary>
    /// Use when a full subject is inputed from the ui
    /// </summary>
    /// <param name="str"></param>
    private void CheckEegFileInput(string str, bool popUp = true)
    {
        bool isStringOk = !string.IsNullOrEmpty(str);
        bool fileExists = false;
        bool isExtensionOk = false;
        if (isStringOk)
        {
            FileInfo fileInfo = new FileInfo(str);
            fileExists = fileInfo.Exists;
            foreach (var ext in _FileExtensions)
            {
                isExtensionOk = isExtensionOk || (fileInfo.Extension == "." + ext);
            }

            if (popUp)
            {
                if (fileInfo.Exists)
                {
                    if (!isExtensionOk)
                    {
                        ApplicationState.displayMessage("File extension is not supported", "NOK", "It seems the extension you specified is not supported yet, sorry :) ");
                        RevertText();
                    }
                }
                else
                {
                    ApplicationState.displayMessage("File does not exists", "NOK", "It seems the path you specified point to a non existing file, please check your input");
                    RevertText();
                }
            }
        }

        if(_ShowIndicator)
            ToggleIndicator(isStringOk, fileExists, isExtensionOk);

        TextUpdated.Invoke(_InputField.text);
    }

    private void ToggleIndicator(bool isStringOk, bool fileExists, bool isExtensionOk)
    {
        if (isStringOk)
        {
            _IsOk.sprite = (isExtensionOk && fileExists) ? m_IsOkFile : m_IsErrorFile;
            _IsOk.color = m_ShowColor;
        }
        else
        {
            _IsOk.sprite = m_IsErrorFile;
            _IsOk.color = m_HideColor;
        }
    }
}
