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
        media.loadDefault += new BrainNotPresentLoadEventHandler(loadElectrodesDefault);
        media.loadTrace += new initTrace(() =>
        {
            hub.brainRemote.needToChangeBrain += new brainChangeEventHandler(changeBrain);
        });
    }

    void OnDestroy()
    {
        media.loadBrain -= new BrainLoadEventHandler(loadBrainAndElectrodes);
        media.loadDefault -= new BrainNotPresentLoadEventHandler(loadElectrodesDefault);
        media.loadTrace -= new initTrace(() =>
        {
            hub.brainRemote.needToChangeBrain -= new brainChangeEventHandler(changeBrain);
        });
    }

    public void loadBrainAndElectrodes(brain_anat brainToLoad, int otherBrain)
    {
        Brain3DHandle = GameObject.Find("BrainGameObject");

        LHBrain = new GameObject("LeftHemi", new System.Type[] { typeof(Hemisphere) });
        LHBrain.transform.parent = Brain3DHandle.transform;
        LHBrain.layer = Brain3DHandle.layer;
        Hemisphere lhemi = LHBrain.GetComponent<Hemisphere>();
        lhemi.InitializeData(brainToLoad.lhemi);
        for (int i = 0; i < lhemi.brainMeshes.Count; i++)
            lhemi.brainMeshes[i].transform.parent = LHBrain.transform;

        RHBrain = new GameObject("RightHemi", new System.Type[] { typeof(Hemisphere) });
        RHBrain.transform.parent = Brain3DHandle.transform;
        RHBrain.layer = Brain3DHandle.layer;
        Hemisphere rhemi = RHBrain.GetComponent<Hemisphere>();
        rhemi.InitializeData(brainToLoad.rhemi);
        for (int i = 0; i < rhemi.brainMeshes.Count; i++)
            rhemi.brainMeshes[i].transform.parent = RHBrain.transform;

        Electrodes = new GameObject("Electrodes", new System.Type[] { typeof(Electrodes) });
        Electrodes.transform.parent = Brain3DHandle.transform;
        ElectrodesScript = Electrodes.GetComponent<Electrodes>();
        ElectrodesScript.loadPtsFile(brainToLoad.pts);

        if(brainToLoad.atlasCSV != null)
            ElectrodesScript.loadAtlasData(brainToLoad.atlasCSV);

        ElectrodesScript.loadElecOnBrain();

        CameraScript = GameObject.Find("CameraBrain").GetComponent<BrainCamera>();
        CameraScript.initCameraPosition();

        if(otherBrain == 0)
            hub.brainRemote.initBrainInteract(true, false, true);
        else if(otherBrain == 1)
            hub.brainRemote.initBrainInteract(true, true, true);
        else if(otherBrain == 2)
            hub.brainRemote.initBrainInteract(false, true, true);

    }

    public void loadElectrodesDefault()
    {
        Brain3DHandle = GameObject.Find("BrainGameObject");

        LHBrain = new GameObject("LeftHemi", new System.Type[] { typeof(Hemisphere) });
        LHBrain.transform.parent = Brain3DHandle.transform;
        LHBrain.layer = Brain3DHandle.layer;
        RHBrain = new GameObject("RightHemi", new System.Type[] { typeof(Hemisphere) });
        RHBrain.transform.parent = Brain3DHandle.transform;
        RHBrain.layer = Brain3DHandle.layer;

        Electrodes = new GameObject("Electrodes", new System.Type[] { typeof(Electrodes) });
        Electrodes.transform.parent = Brain3DHandle.transform;
        ElectrodesScript = Electrodes.GetComponent<Electrodes>();
        ElectrodesScript.loadDefaultPearl(media.elanFiles);
        ElectrodesScript.loadElecOnBrain();

        CameraScript = GameObject.Find("CameraBrain").GetComponent<BrainCamera>();
        CameraScript.initCameraPosition();

        hub.brainRemote.initBrainInteract(false, false, true);
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
        switch (idBrain)
        {
            case 0:
                LHBrain.gameObject.SetActive(true);
                RHBrain.gameObject.SetActive(true);
                hub.brainRemote.setBrainInteract(true);
                updateBrainMesh(media.pm.currentPatients[media.pm.idCurrentPatientLoaded].mni);
                break;
            case 1:
                LHBrain.gameObject.SetActive(true);
                RHBrain.gameObject.SetActive(true);
                hub.brainRemote.setBrainInteract(true);
                updateBrainMesh(media.pm.currentPatients[media.pm.idCurrentPatientLoaded].pat);
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

    void updateBrainMesh(brain_anat brainToLoad)
    {
        if (Brain3DHandle == null)
            Brain3DHandle = GameObject.Find("BrainGameObject");

        Destroy(GameObject.Find("LeftHemi"));
        LHBrain = new GameObject("LeftHemi", new System.Type[] { typeof(Hemisphere) });
        LHBrain.transform.parent = Brain3DHandle.transform;
        LHBrain.transform.SetSiblingIndex(0);
        LHBrain.transform.localPosition = new Vector3(0, 0, 0);
        LHBrain.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));
        LHBrain.layer = Brain3DHandle.layer;
        Hemisphere lhemi = LHBrain.GetComponent<Hemisphere>();
        lhemi.InitializeData(brainToLoad.lhemi);
        for (int i = 0; i < lhemi.brainMeshes.Count; i++)
        {
            lhemi.brainMeshes[i].transform.parent = LHBrain.transform;
            lhemi.brainMeshes[i].transform.localPosition = new Vector3(0, 0, 0);
            lhemi.brainMeshes[i].transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));
        }

        Destroy(GameObject.Find("RightHemi"));
        RHBrain = new GameObject("RightHemi", new System.Type[] { typeof(Hemisphere) });
        RHBrain.transform.parent = Brain3DHandle.transform;
        RHBrain.transform.SetSiblingIndex(1);
        RHBrain.transform.localPosition = new Vector3(0, 0, 0);
        RHBrain.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));
        RHBrain.layer = Brain3DHandle.layer;
        Hemisphere rhemi = RHBrain.GetComponent<Hemisphere>();
        rhemi.InitializeData(brainToLoad.rhemi);
        for (int i = 0; i < rhemi.brainMeshes.Count; i++)
        {
            rhemi.brainMeshes[i].transform.parent = RHBrain.transform;
            rhemi.brainMeshes[i].transform.localPosition = new Vector3(0, 0, 0);
            rhemi.brainMeshes[i].transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));
        }
        ElectrodesScript.loadPtsFile(brainToLoad.pts);

        if (brainToLoad.atlasCSV != null)
            ElectrodesScript.loadAtlasData(brainToLoad.atlasCSV);

        ElectrodesScript.updateElecPosition();
    }

}
