using UnityEngine;
using UnityEngine.UI;

public class browseButton : MonoBehaviour
{
    [SerializeField] bool open = true;

    [SerializeField] public InputField inputfield = null;
    [SerializeField] Text textfield = null;
    [SerializeField] Button buttonBrowse = null;

    void Awake()
    {
        //textfield = handleFileBrowser.transform.GetChild(0).GetComponent<Text>();
        //inputfield = handleFileBrowser.transform.GetChild(1).GetComponent<InputField>();
        //buttonBrowse = handleFileBrowser.transform.GetChild(2).GetComponent<Button>();
        buttonBrowse.onClick.AddListener(() => loadFile());
    }

    void OnDestroy()
    {
        buttonBrowse.onClick.RemoveAllListeners();
    }

    void loadFile()
    {
        if(open)
            inputfield.text = FileBrowser.getOpenFileName(new string[] { "tri", "gii", "pts", "trc", "eeg", "avi", "mp4", "prov", "pos", "mni", "csv" });
        else
            inputfield.text = FileBrowser.getSaveFileName(new string[] { "mp4" }, "Save Video To");
    }
}