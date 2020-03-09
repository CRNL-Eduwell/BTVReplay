using SFB;
using UnityEngine;
using UnityEngine.UI;

public class browseButton : MonoBehaviour
{
    [SerializeField] private bool open = true;
    [SerializeField] public InputField inputfield = null;
    [SerializeField] private Button buttonBrowse = null;

    private void Awake()
    {
        buttonBrowse.onClick.AddListener(() => loadFile());
    }

    private void OnDestroy()
    {
        buttonBrowse.onClick.RemoveAllListeners();
    }

    private void loadFile()
    {
        if (open)
            inputfield.text = FileBrowser.GetExistingFileName(new string[] { "tri", "gii", "trm", "pts", "trc", "eeg", "avi", "mp4", "prov", "pos", "mni", "csv" });
        else
        {
            var extensionList = new[] { new ExtensionFilter("Video File", "mp4") };
            inputfield.text = FileBrowser.GetSavedFileName(extensionList, "Save Video To");
        }
    }
}