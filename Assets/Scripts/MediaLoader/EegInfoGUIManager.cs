using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class EegInfoGUIManager : MonoBehaviour
{
    [SerializeField] BrowseWidget _EegFile = null;
    [SerializeField] InputField _KeyField = null;

    public KeyValuePair<string, IEegFileInfo> GetEegFileInfoFromGUI()
    {
        string path = _EegFile._InputField.text;
        if (!string.IsNullOrEmpty(path))
        {
            string key = _KeyField.text;
            FileInfo fileInfo = new FileInfo(path);

            if (fileInfo.Extension == ".TRC")
            {
                return new KeyValuePair<string, IEegFileInfo>(key, new MicromedFileInfo(fileInfo.FullName));
            }
            else if (fileInfo.Extension == ".eeg")
            {
                return new KeyValuePair<string, IEegFileInfo>(key, new ElanFileInfo(fileInfo.FullName));
            }
            else if (fileInfo.Extension == ".vhdr")
            {
                return new KeyValuePair<string, IEegFileInfo>(key, new BrainvisionFileInfo(fileInfo.FullName));
            }
            else if (fileInfo.Extension == ".edf")
            {
                return new KeyValuePair<string, IEegFileInfo>(key, new EdfFileInfo(fileInfo.FullName));
            }
        }
        return new KeyValuePair<string, IEegFileInfo>();
    }

    public void SetEegFileInfoToGUI(KeyValuePair<string, IEegFileInfo> kvp)
    {
        bool isDefaultValue = kvp.Equals(default(KeyValuePair<string, IEegFileInfo>));
        _EegFile._InputField.text = isDefaultValue ? "" : kvp.Value.Files[0];
        _KeyField.text = isDefaultValue ? "" : kvp.Key;
    }
}
