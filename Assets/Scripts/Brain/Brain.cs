using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Brain : MonoBehaviour {

    [SerializeField] BTVMedia media = null;

    BrainCamera CameraScript = null;
    Electrodes ElectrodesScript = null;

    GameObject LHBrain = null;
    GameObject RHBrain = null;
    GameObject Electrodes = null;

    void Awake()
    {
        media.loadBrain += new BrainLoadEventHandler(loadBrainAndElectrodes);
        media.loadDefault += new BrainNotPresentLoadEventHandler(loadElectrodesDefault);
        Messenger.Default.Register<BrainParametersMessage>(this, OnBrainParametersMessage);
    }

    void OnDestroy()
    {
        media.loadBrain -= new BrainLoadEventHandler(loadBrainAndElectrodes);
        media.loadDefault -= new BrainNotPresentLoadEventHandler(loadElectrodesDefault);
        Messenger.Default.Unregister(this);
    }

    public void loadBrainAndElectrodes(brain_anat brainToLoad, int otherBrain)
    {
        LHBrain = UpdateOneHemisphere("LeftHemi", 0, brainToLoad.lhemi);

        if (brainToLoad.GetMeshNb == mesh_Configuration.leftright)
        {
            RHBrain = UpdateOneHemisphere("RightHemi", 1, brainToLoad.rhemi);
        }
        else
        {
            RHBrain = new GameObject("RightHemi");
            RHBrain.transform.parent = gameObject.transform;
            RHBrain.layer = gameObject.layer;
        }

        Electrodes = new GameObject("Electrodes", new System.Type[] { typeof(Electrodes) });
        Electrodes.transform.parent = gameObject.transform;
        ElectrodesScript = Electrodes.GetComponent<Electrodes>();
        ElectrodesScript.loadPtsFile(brainToLoad.pts, brainToLoad.GetEegTech);

        if(brainToLoad.atlasCSV != "")
            ElectrodesScript.loadAtlasData(brainToLoad.atlasCSV);

        ElectrodesScript.loadElecOnBrain();

        CameraScript = GameObject.Find("CameraBrain").GetComponent<BrainCamera>();
        CameraScript.initCameraPosition();
    }

    public void loadElectrodesDefault(eeg_Technology eeg)
    {
        LHBrain = new GameObject("LeftHemi", new System.Type[] { typeof(Hemisphere) });
        LHBrain.transform.parent = gameObject.transform;
        LHBrain.layer = gameObject.layer;
        RHBrain = new GameObject("RightHemi", new System.Type[] { typeof(Hemisphere) });
        RHBrain.transform.parent = gameObject.transform;
        RHBrain.layer = gameObject.layer;

        Electrodes = new GameObject("Electrodes", new System.Type[] { typeof(Electrodes) });
        Electrodes.transform.parent = gameObject.transform;
        ElectrodesScript = Electrodes.GetComponent<Electrodes>();
        ElectrodesScript.loadDefaultPearl(media.elanFiles, eeg);
        ElectrodesScript.loadElecOnBrain();

        CameraScript = GameObject.Find("CameraBrain").GetComponent<BrainCamera>();
        CameraScript.initCameraPosition();

        //hub.brainRemote.initBrainInteract(false, false, true);
    }

    //== Obsolete
    public static void changeVisuBrain(int codeSide)
    {
        UnityEngine.Debug.Log("static void changeVisuBrain(int codeSide) is obsolete, only used by optionHub.cs that osuld not be there anymore");

        //GameObject brainHandle = GameObject.Find("BrainGameObject");
        //if (brainHandle != null && brainHandle.transform.childCount > 0)
        //{
        //    switch (codeSide)
        //    {
        //        case -1:
        //            brainHandle.transform.GetChild(0).gameObject.SetActive(true);
        //            brainHandle.transform.GetChild(1).gameObject.SetActive(false);
        //            break;
        //        case 0:
        //            brainHandle.transform.GetChild(0).gameObject.SetActive(true);
        //            brainHandle.transform.GetChild(1).gameObject.SetActive(true);
        //            break;
        //        case 1:
        //            brainHandle.transform.GetChild(0).gameObject.SetActive(false);
        //            brainHandle.transform.GetChild(1).gameObject.SetActive(true);
        //            break;
        //        default:
        //            Debug.LogError("Problem switching brain view");
        //            break;
        //    }
        //}
        //else
        //{
        //    Debug.LogError("Brain wasn't loaded or there was a problem");
        //}

    }
    //== Obsolete

    private void OnBrainParametersMessage(BrainParametersMessage message)
    {
        UnityEngine.Debug.Log("Brain Message, yata");
        switch (message.TaskToExecute)
        {
            case 0:
                Debug.Log("Update Brain Model");
                UpdateBrainModel(message.ModelId);
                break;
            case 1:
                Debug.Log("Update Brain Visu");
                UpdateDisplayedMeshes(message.MeshesToDisplay);
                break;
            case 2:
                Debug.Log("Update BrainGain, not done yet");
                break;
            default:
                Debug.LogError("Id of action to execute does not exist : " + message.TaskToExecute);
                break;
        }
    }

    private void UpdateBrainModel(int ModelId)
    {
        switch (ModelId)
        {
            case 0:
                LHBrain.gameObject.SetActive(true);
                RHBrain.gameObject.SetActive(true);
                //hub.brainRemote.setBrainInteract(true);
                updateBrainMesh(media.pm.currentPatients[media.pm.idCurrentPatientLoaded].mni);
                break;
            case 1:
                LHBrain.gameObject.SetActive(true);
                RHBrain.gameObject.SetActive(true);
                //hub.brainRemote.setBrainInteract(true);
                updateBrainMesh(media.pm.currentPatients[media.pm.idCurrentPatientLoaded].pat);
                break;
            case 2:
                LHBrain.gameObject.SetActive(false);
                RHBrain.gameObject.SetActive(false);
                //hub.brainRemote.setBrainInteract(false);
                ElectrodesScript.updateElecPearl();
                break;
            default:
                Debug.LogError("UpdateBrainModel => ModelId value is unknown : " + ModelId);
                break;
        }
    }

    private void updateBrainMesh(brain_anat brainToLoad)
    {
        Destroy(GameObject.Find("LeftHemi"));
        LHBrain = UpdateOneHemisphere("LeftHemi", 0, brainToLoad.lhemi);

        if (brainToLoad.GetMeshNb == mesh_Configuration.leftright)
        {
            Destroy(GameObject.Find("RightHemi"));
            RHBrain = UpdateOneHemisphere("RightHemi", 1, brainToLoad.rhemi);
        }
        else
        {
            RHBrain = new GameObject("RightHemi");
            RHBrain.transform.parent = gameObject.transform;
            RHBrain.layer = gameObject.layer;
        }

        ElectrodesScript.loadPtsFile(brainToLoad.pts, brainToLoad.GetEegTech);

        if (brainToLoad.atlasCSV != "")
            ElectrodesScript.loadAtlasData(brainToLoad.atlasCSV);

        ElectrodesScript.updateElecPosition();
    }

    //Left : sibling 0
    //Right : sibling 1
    private GameObject UpdateOneHemisphere(string HemisphereName, int SiblingIndex, string FilePath)
    {
        GameObject newHemisphere = new GameObject("HemisphereName", new System.Type[] { typeof(Hemisphere) });
        newHemisphere.transform.parent = gameObject.transform;
        newHemisphere.transform.SetSiblingIndex(SiblingIndex);
        newHemisphere.transform.localPosition = new Vector3(0, 0, 0);
        newHemisphere.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));
        newHemisphere.layer = gameObject.layer;

        Hemisphere hemi = newHemisphere.GetComponent<Hemisphere>();
        hemi.InitializeData(FilePath);
        for (int i = 0; i < hemi.MeshesGameObjects.Count; i++)
        {
            hemi.MeshesGameObjects[i].transform.parent = newHemisphere.transform;
            hemi.MeshesGameObjects[i].transform.localPosition = new Vector3(0, 0, 0);
            hemi.MeshesGameObjects[i].transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));
        }

        return newHemisphere;
    }

    private void UpdateDisplayedMeshes(int MeshesId)
    {
        switch (MeshesId)
        {
            case -1:
                LHBrain.gameObject.SetActive(true);
                RHBrain.gameObject.SetActive(false);
                break;
            case 0:
                LHBrain.gameObject.SetActive(true);
                RHBrain.gameObject.SetActive(true);
                break;
            case 1:
                LHBrain.gameObject.SetActive(false);
                RHBrain.gameObject.SetActive(true);
                break;
            default:
                Debug.LogError("UpdateDisplayedMeshes => MeshesId value is unknown : " + MeshesId);
                break;
        }
    }
}
