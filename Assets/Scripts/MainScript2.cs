using UnityEngine;
using UnityEngine.UI;

public class MainScript2 : MonoBehaviour
{
    public GameObject colorPicker = null;
    public GameObject colorPicker2 = null;
    public GameObject telecommandeCurve = null;
    public GameObject telecommandeCurve2 = null;
    public GameObject brain3D = null;

    public PhyObject3D brain3DScript = null;
    public VLCSharp vlcScript = null;
    public TVCurve curveScript = null;
    public TVCurve curveScript2 = null;
    public TVCurvePerf curvePerfScript = null;
    public Dropdown dropDownScript = null;
    public Dropdown dropDownScript2 = null;
    public TVDropDown tvDropDownScript = null;
    public TVDropDown tvDropDownScript2 = null;
    public BTVMedia btvMedia = null;

    public elanFile eHandle = null;

    public bool init = false;

    void Start ()
    {
        colorPicker.SetActive(false);
        telecommandeCurve.SetActive(false);
        colorPicker2.SetActive(false);
        telecommandeCurve2.SetActive(false);
    }

    void Update ()
    {
        if (init == false)
        {
            if (btvMedia.loaded == true)
            {
                eHandle = btvMedia.e0;
                curveScript.init();
                curveScript2.init();
                curvePerfScript.init();
                loadDropDownUI();
                load3DObject();
                init = true;
            }
        }
        else
        {
            if (vlcScript.videoPaused == false)
            {
                curveScript.updateDraw(eHandle.eegData[dropDownScript.value], (int)(vlcScript.totalTimeMSec * 0.064) - 640);
                curveScript2.updateDraw(eHandle.eegData[dropDownScript2.value], (int)(vlcScript.totalTimeMSec * 0.064) - 640);
            }
        }
	}

    void loadDropDownUI()
    {
        tvDropDownScript.loadElectrodeListInDropDown(eHandle.nameElectrode.ToArray(), eHandle.nameElectrode.Count);
        tvDropDownScript2.loadElectrodeListInDropDown(eHandle.nameElectrode.ToArray(), eHandle.nameElectrode.Count);
    }

    void load3DObject()
    {
        string LHemi = btvMedia.Lhemi.transform.GetChild(1).GetComponent<InputField>().text;
        string RHemi = btvMedia.Rhemi.transform.GetChild(1).GetComponent<InputField>().text;
        string PTS = btvMedia.PTS.transform.GetChild(1).GetComponent<InputField>().text;

        brain3DScript.loadBrainAndElectrodes(LHemi, RHemi, PTS);
    }

    public void OnClicked(Button button)
    {
        if (button.name == "Buttonsm0" && btvMedia.e0 != null)
        {
            eHandle = btvMedia.e0;
        }
        else if (button.name == "Buttonsm250" && btvMedia.e250 != null)
        {
            eHandle = btvMedia.e250;
        }
        else if (button.name == "Buttonsm500" && btvMedia.e500 != null)
        {
            eHandle = btvMedia.e500;
        }
        else if (button.name == "Buttonsm1000" && btvMedia.e1000 != null)
        {
            eHandle = btvMedia.e1000;
        }
        else if (button.name == "Buttonsm2500" && btvMedia.e2500 != null)
        {
            eHandle = btvMedia.e2500;
        }
        else if (button.name == "Buttonsm5000" && btvMedia.e5000 != null)
        {
            eHandle = btvMedia.e5000;
        }
    }
}
