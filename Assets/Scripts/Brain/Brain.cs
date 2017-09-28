using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Brain : MonoBehaviour {

    [SerializeField] BTVMedia media = null;
    GameObject Brain3DHandle = null;
    BrainCamera CameraScript = null;
    Electrodes ElectrodesScript = null;

    GameObject LHBrain = null;
    GameObject RHBrain = null;
    GameObject Electrodes = null;

    void Awake()
    {
        media.loadMniBrain += new mniBrainLoadEventHandler(loadBrainAndElectrodes);
    }

    void OnDestroy()
    {
        media.loadMniBrain -= new mniBrainLoadEventHandler(loadBrainAndElectrodes);
    }

    //-1 L - 0 All - 1 R
    public static void changeVisuBrain(int codeSide)
    {
        GameObject brainHandle = GameObject.Find("BrainGameObject");
        if (brainHandle != null && brainHandle.transform.childCount > 0)
        {
            switch (codeSide)
            {
                case -1:
                    brainHandle.transform.GetChild(0).gameObject.SetActive(true);
                    brainHandle.transform.GetChild(1).gameObject.SetActive(false);
                    break;
                case 0:
                    brainHandle.transform.GetChild(0).gameObject.SetActive(true);
                    brainHandle.transform.GetChild(1).gameObject.SetActive(true);
                    break;
                case 1:
                    brainHandle.transform.GetChild(0).gameObject.SetActive(false);
                    brainHandle.transform.GetChild(1).gameObject.SetActive(true);
                    break;
                default:
                    Debug.LogError("Problem switching brain view");
                    break;
            }
        }
        else
        {
            Debug.LogError("Brain wasn't loaded or there was a problem");
        }
        
    }

    public void loadBrainAndElectrodes(string LHtri, string RHtri, string PTS)
    {
        Brain3DHandle = GameObject.Find("BrainGameObject");
        CameraScript = GameObject.Find("CameraBrain").GetComponent<BrainCamera>();

        LHBrain = new GameObject("LeftHemi", new System.Type[] { typeof(Hemisphere) });
        LHBrain.transform.parent = Brain3DHandle.transform;
        LHBrain.layer = Brain3DHandle.layer;
        LHBrain.GetComponent<Hemisphere>().InitializeData(LHtri);

        RHBrain = new GameObject("RightHemi", new System.Type[] { typeof(Hemisphere) });
        RHBrain.transform.parent = Brain3DHandle.transform;
        RHBrain.layer = Brain3DHandle.layer;
        RHBrain.GetComponent<Hemisphere>().InitializeData(RHtri);

        Electrodes = new GameObject("Electrodes", new System.Type[] { typeof(Electrodes) });
        Electrodes.transform.parent = Brain3DHandle.transform;
        ElectrodesScript = Electrodes.GetComponent<Electrodes>();
        ElectrodesScript.loadPtsFile(PTS);
        ElectrodesScript.loadElecOnBrain();

        CameraScript.initCameraPosition();
    }
}
