using UnityEngine;
using UnityEngine.UI;

public class browseButtonQT : MonoBehaviour
{
    GameObject handleFileBrowser = null;
    InputField inputfield = null;
    Text textfield = null;

    string compareStr = "";
    bool detailedCheck = false;

    void Start()
    {
        handleFileBrowser = gameObject;
        textfield = handleFileBrowser.transform.GetChild(0).GetComponent<Text>();
        inputfield = handleFileBrowser.transform.GetChild(1).GetComponent<InputField>();

        if (handleFileBrowser.name == "LHemi")
        {
            compareStr = "Lhemi.tri";
            detailedCheck = true;
            checkPath();
        }
        else if (handleFileBrowser.name == "RHemi")
        {
            compareStr = "Rhemi.tri";
            detailedCheck = true;
            checkPath();
        }
        else if (handleFileBrowser.name == "PTS")
        {
            compareStr = "MNI.pts";
            detailedCheck = true;
            checkPath();
        }
        else if (handleFileBrowser.name == "Video")
        {
            compareStr = ".AVI";
            detailedCheck = false;
            checkPath();
        }
        else if (handleFileBrowser.name == "POS")
        {
            compareStr = ".pos";
            detailedCheck = false;
            checkPath();
        }
        else if (handleFileBrowser.name == "Prov")
        {
            compareStr = ".prov";
            detailedCheck = false;
            checkPath();
        }
        else
        {
            compareStr = ".eeg";
            detailedCheck = false;
            checkPath();
        }
    }

    public void loadFile()
    {
        inputfield.text = QtGUI_dll.Instance.getOpenFileName(new string[] { "tri", "pts", "trc", "eeg", "avi", "mp4", "prov", "pos", "mni" });
    }

    public void checkPath()
    {
        bool pathOk = false;

        pathOk = checkIfCorrect(inputfield.text, compareStr, detailedCheck);
        if (pathOk == true)
        {
            textfield.color = Color.green;
        }
        else
        {
            textfield.color = Color.red;
        }
    }

    bool checkIfCorrect(string path, string compareString, bool detailedCheck)
    {
        string compareMe = "";
        string[] resultSplit = path.Split(new char[] { '_', '.' });

        if (path != "")
        {
            if (detailedCheck == true)
            {
                compareMe = resultSplit[resultSplit.Length - 2] + '.' + resultSplit[resultSplit.Length - 1];
            }
            else
            {
                compareMe = '.' + resultSplit[resultSplit.Length - 1];
            }

            return compareMe.ToLower().Equals(compareString.ToLower());
        }
        else
        {
            return false;
        }
    }
}
