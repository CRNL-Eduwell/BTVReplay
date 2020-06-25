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

    [SerializeField] BrowseWidget _EegFile = null;
    [SerializeField] InputField _KeyField = null;
    private string m_memory = "";

    private void Awake()
    {
        _EegFile.onEndEdit.AddListener(CheckEegFileInput);
        _KeyField.onEndEdit.AddListener((str) => { onEndEditKey.Invoke(str); m_memory = _KeyField.text; });
    }

    private void OnDestroy()
    {
        _EegFile.onEndEdit.RemoveAllListeners();
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

    public void SetEegFileInfoToGUI(KeyValuePair<string, IEegFileInfo> kvp)
    {
        bool isDefaultValue = kvp.Equals(default(KeyValuePair<string, IEegFileInfo>));
        _EegFile.Text = isDefaultValue ? "" : kvp.Value.Files[0];
        _KeyField.text = isDefaultValue ? "" : kvp.Key;
    }

    public void RevertKeyField()
    {
        _KeyField.text = m_memory;
    }

    private void CheckEegFileInput(string str)
    {
        if (!string.IsNullOrEmpty(str))
        {
            FileInfo fileInfo = new FileInfo(str);
            if (fileInfo.Exists)
            {
                if (!((fileInfo.Extension == ".TRC") || (fileInfo.Extension == ".eeg") || (fileInfo.Extension == ".vhdr") || (fileInfo.Extension == ".edf")))
                {
                    ApplicationState.displayMessage("File extension is not supported", "NOK", "It seems the extension you specified is not supported yet, sorry :) ");
                    _EegFile.RevertText();
                }
            }
            else
            {
                ApplicationState.displayMessage("File does not exists", "NOK", "It seems the path you specified point to a non existing file, please check your input");
                _EegFile.RevertText();
            }
        }
    }
}
