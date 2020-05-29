using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.IO;
using System.Collections;

public class FolderSelector : MonoBehaviour
{
    public string Text
    {
        get
        {
            return _InputField.text;
        }
        set
        {
            _InputField.text = value;
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

    private bool m_MoveCaret = false;

    private void Awake()
    {
        _BrowseButton.onClick.AddListener(ChooseFolder);
    }

    private void OnDestroy()
    {
        _BrowseButton.onClick.RemoveAllListeners();
    }
    private void ChooseFolder()
    {
        string path = FileBrowser.GetExistingDirectoryName("Select a directory", Text);
        if (!string.IsNullOrEmpty(path))
        {
            if (Directory.Exists(path))
            {
                Text = path;
            }
        }
    }
}