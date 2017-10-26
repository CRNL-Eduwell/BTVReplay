using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Brain : MonoBehaviour {

    [SerializeField] BTVMedia media = null;
    [SerializeField] optionsHub hub = null;
    GameObject Brain3DHandle = null;
    BrainCamera CameraScript = null;
    Electrodes ElectrodesScript = null;

    GameObject LHBrain = null;
    GameObject RHBrain = null;
    GameObject Electrodes = null;

    void Awake()
    {
        media.loadBrain += new BrainLoadEventHandler(loadBrainAndElectrodes);
        media.loadTrace += new initTrace(() => 
        {
            hub.brainRemote.needToChangeBrain += new brainChangeEventHandler(changeBrain);
        });
    }

    void OnDestroy()
    {
        media.loadBrain -= new BrainLoadEventHandler(loadBrainAndElectrodes);
        media.loadTrace -= new initTrace(() =>
        {
            hub.brainRemote.needToChangeBrain -= new brainChangeEventHandler(changeBrain);
        });
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

    public void changeBrain(int idBrain)
    {
        string l = "", r = "", p = "";

        switch (idBrain)
        {
            case 0:
                LHBrain.gameObject.SetActive(true);
                RHBrain.gameObject.SetActive(true);
                hub.brainRemote.setBrainInteract(true);
                l = media.pm.currentPatients[media.pm.idCurrentPatientLoaded].lhemi_MNI;
                r = media.pm.currentPatients[media.pm.idCurrentPatientLoaded].rhemi_MNI;
                p = media.pm.currentPatients[media.pm.idCurrentPatientLoaded].pts_MNI;
                updateBrainMesh(l, r, p);
                break;
            case 1:
                LHBrain.gameObject.SetActive(true);
                RHBrain.gameObject.SetActive(true);
                hub.brainRemote.setBrainInteract(true);
                l = media.pm.currentPatients[media.pm.idCurrentPatientLoaded].lhemi_PAT;
                r = media.pm.currentPatients[media.pm.idCurrentPatientLoaded].rhemi_PAT;
                p = media.pm.currentPatients[media.pm.idCurrentPatientLoaded].pts_PAT;
                updateBrainMesh(l, r, p);
                break;
            case 2:
                LHBrain.gameObject.SetActive(false); 
                RHBrain.gameObject.SetActive(false);
                hub.brainRemote.setBrainInteract(false);
                ElectrodesScript.updateElecPearl();
                break;
            default:
                break;
        }
    }

    public void loadBrainAndElectrodes(string LHtri, string RHtri, string PTS)
    {
        Brain3DHandle = GameObject.Find("BrainGameObject");

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

        CameraScript = GameObject.Find("CameraBrain").GetComponent<BrainCamera>();
        CameraScript.initCameraPosition();
    }

    void updateBrainMesh(string LHtri, string RHtri, string PTS)
    {
        if(Brain3DHandle == null)
            Brain3DHandle = GameObject.Find("BrainGameObject");

        Destroy(GameObject.Find("LeftHemi"));
        LHBrain = new GameObject("LeftHemi", new System.Type[] { typeof(Hemisphere) });
        LHBrain.transform.parent = Brain3DHandle.transform;
        LHBrain.transform.SetSiblingIndex(0);
        LHBrain.transform.localPosition = new Vector3(0, 0, 0);
        LHBrain.transform.Rotate(new Vector3(-90, 0, 0));
        LHBrain.layer = Brain3DHandle.layer;
        LHBrain.GetComponent<Hemisphere>().InitializeData(LHtri);

        Destroy(GameObject.Find("RightHemi"));
        RHBrain = new GameObject("RightHemi", new System.Type[] { typeof(Hemisphere) });
        RHBrain.transform.parent = Brain3DHandle.transform;
        RHBrain.transform.SetSiblingIndex(1);
        RHBrain.transform.localPosition = new Vector3(0, 0, 0);
        RHBrain.transform.Rotate(new Vector3(-90, 0, 0));
        RHBrain.layer = Brain3DHandle.layer;
        RHBrain.GetComponent<Hemisphere>().InitializeData(RHtri);

        ElectrodesScript.loadPtsFile(PTS);
        ElectrodesScript.updateElecPosition();
    }
}
