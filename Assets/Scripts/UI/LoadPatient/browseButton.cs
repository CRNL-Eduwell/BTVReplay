using UnityEngine;
using UnityEngine.UI;

public class browseButton : MonoBehaviour
{
    GameObject handleFileBrowser = null;
    public InputField inputfield = null;
    public Text textfield = null;
    public Button buttonBrowse = null;

    void Awake()
    {
        handleFileBrowser = gameObject;
        textfield = handleFileBrowser.transform.GetChild(0).GetComponent<Text>();
        inputfield = handleFileBrowser.transform.GetChild(1).GetComponent<InputField>();
        buttonBrowse = handleFileBrowser.transform.GetChild(2).GetComponent<Button>();
        buttonBrowse.onClick.AddListener(() => loadFile());
    }

    void OnDestroy()
    {
        buttonBrowse.onClick.RemoveAllListeners();
    }

    void loadFile()
    {
        inputfield.text = QtGUI_dll.Instance.getOpenFileName(new string[] { "tri", "pts", "trc", "eeg", "avi", "mp4", "prov", "pos", "mni" });
    }
}