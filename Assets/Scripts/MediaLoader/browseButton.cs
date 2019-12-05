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
        if(open)
            inputfield.text = FileBrowser.GetExistingFileName(new string[] { "tri", "gii", "pts", "trc", "eeg", "avi", "mp4", "prov", "pos", "mni", "csv" });
        else
            inputfield.text = FileBrowser.GetSavedFileName(new string[] { "mp4" }, "Save Video To");
    }
}