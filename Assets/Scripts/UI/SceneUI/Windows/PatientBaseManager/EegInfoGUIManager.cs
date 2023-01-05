using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class EegInfoGUIManager : MonoBehaviour
{
    public StringEvent onEndEditKey = new StringEvent();

    public bool IsInteractable
    {
        get
        {
            return _EegFile.IsInteractable && _KeyField.interactable;
        }
        set
        {
            _EegFile.IsInteractable = value;
            _KeyField.interactable = value;
        }
    }

    [SerializeField]
    private BrowseWidget _EegFile = null;
    [SerializeField]
    private InputField _KeyField = null;

    private string m_memory = "";

    private void Awake()
    {
        _KeyField.onEndEdit.AddListener((str) => { onEndEditKey.Invoke(str); m_memory = _KeyField.text; });
    }

    private void OnDestroy()
    {
        _KeyField.onEndEdit.RemoveAllListeners();
    }

    public KeyValuePair<string, IEegFileInfo> GetEegFileInfoFromGUI()
    {
        string path = _EegFile.Text;
        if (string.IsNullOrEmpty(path)) return new KeyValuePair<string, IEegFileInfo>();

        string key = _KeyField.text;
        FileInfo fileInfo = new FileInfo(path);
        if (fileInfo.Extension == ".TRC")
        {
            return new KeyValuePair<string, IEegFileInfo>(key, new MicromedFileInfo(path));
        }
        else if (fileInfo.Extension == ".eeg")
        {
            return new KeyValuePair<string, IEegFileInfo>(key, new ElanFileInfo(path));
        }
        else if (fileInfo.Extension == ".vhdr")
        {
            return new KeyValuePair<string, IEegFileInfo>(key, new BrainvisionFileInfo(path));
        }
        else if (fileInfo.Extension == ".edf")
        {
            return new KeyValuePair<string, IEegFileInfo>(key, new EdfFileInfo(path));
        }
        else
        {
            return new KeyValuePair<string, IEegFileInfo>();
        }
    }

    public void SetEegFileInfoToGUI(string key, string filePath)
    {
        _EegFile.TextWithoutPopUp = filePath;
        _KeyField.text = key;
    }

    public void RevertKeyField()
    {
        _KeyField.text = m_memory;
    }
}
