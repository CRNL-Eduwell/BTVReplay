using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class ExamGuiManager : MonoBehaviour
{
    public bool IsInteractable
    {
        get
        {
            return _Video.IsInteractable;
        }
        set
        {
            foreach (var eeg in _EegFiles)
                eeg.IsInteractable = value;
            _Video.IsInteractable = value;
        }
    }
    [SerializeField] private EegInfoGUIManager[] _EegFiles = new EegInfoGUIManager[6] { null, null, null, null, null, null, };
    [SerializeField] private BrowseWidget _Video = null;

    private void Awake()
    {
        foreach (var eeg in _EegFiles)
        {
            eeg.onEndEditKey.AddListener((str) => { IsKeyOk(str, eeg); });
        }
    }

    private void OnDestroy()
    {
        foreach (var eeg in _EegFiles)
        {
            eeg.onEndEditKey.RemoveAllListeners();
        }
    }

    private void IsKeyOk(string str, EegInfoGUIManager eeg)
    {
        if (string.IsNullOrEmpty(str))
        {
            ApplicationState.displayMessage("Key Error", "NOK", "Error ading Eeg File : you need to define a key for the eeg file that is not a null/empty string");
            eeg.RevertKeyField();
            return;
        }

        List<string> keys = new List<string>();
        int fileCount = _EegFiles.Length;
        for (int i = 0; i < fileCount; i++)
        {
            if (_EegFiles[i] == eeg) continue; //if this is the one modified , we don't want to take it into account
            KeyValuePair<string, IEegFileInfo> kvp = _EegFiles[i].GetEegFileInfoFromGUI();
            keys.Add(kvp.Key);
        }

        if (keys.Contains(str))
        {
            ApplicationState.displayMessage("Key Error", "NOK", "Error ading Eeg File : you need to have a different key for each eeg file");
            eeg.RevertKeyField();
            return;
        }
    }

    public void SetEegFiles(Dictionary<string, IEegFileInfo> eegFiles)
    {
        for (int i = 0; i < _EegFiles.Length; i++)
        {
            KeyValuePair<string, IEegFileInfo> kvp = eegFiles.ElementAtOrDefault(i);
            bool isDefaultValue = kvp.Equals(default(KeyValuePair<string, IEegFileInfo>));
            string key = isDefaultValue ? "" : kvp.Key;
            string filePath = isDefaultValue ? "" : kvp.Value.Files[0];

            _EegFiles[i].SetEegFileInfoToGUI(key, filePath);
        }
    }

    public Dictionary<string, IEegFileInfo> GetEegFiles()
    {
        Dictionary<string, IEegFileInfo> eegFiles = new Dictionary<string, IEegFileInfo>();
        for (int i = 0; i < _EegFiles.Length; i++)
        {
            KeyValuePair<string, IEegFileInfo> kvp = _EegFiles[i].GetEegFileInfoFromGUI();
            if (!kvp.Equals(default(KeyValuePair<string, IEegFileInfo>)))
            {
                if (string.IsNullOrEmpty(kvp.Key))
                {
                    ApplicationState.displayMessage("Key Error", "NOK", "Error ading Eeg File : you need to define a key for the eeg file that is not a null/empty string");
                    continue;
                }
                else if (eegFiles.ContainsKey(kvp.Key))
                {
                    ApplicationState.displayMessage("Key Error", "NOK", "Error ading Eeg File : you need to have a different key for each eeg file");
                    continue;
                }
                else
                {
                    eegFiles.Add(kvp.Key, kvp.Value);
                }
            }
        }
        return eegFiles;
    }

    public void SetVideoFilePath(string path)
    {
        _Video.Text = path;
    }

    public string GetVideoFilePath()
    {
        return _Video.Text;
    }
}
