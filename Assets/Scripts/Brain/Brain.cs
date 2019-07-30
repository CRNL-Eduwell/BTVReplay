using Assets.Scripts.Data.Factory;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Brain : MonoBehaviour {

    [SerializeField] BTVMedia media = null;

    BrainCamera CameraScript = null;

    //Handle to corresponding game objects
    GameObject LHBrain = null;
    GameObject RHBrain = null;
    GameObject Electrodes = null;

    //Script dealing with electrodes, whether those are intra, or scalp
    IElectrodesContext m_ElectrodesContext = null;

    void Awake()
    {
        media.loadBrain += new BrainLoadEventHandler(loadBrainAndElectrodes);
        media.loadDefault += new BrainNotPresentLoadEventHandler(loadElectrodesDefault);
        Messenger.Default.Register<UiToBrainMessage>(this, OnBrainParametersMessage);
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

        Electrodes = new GameObject("Electrodes");
        Electrodes.transform.parent = gameObject.transform;
        m_ElectrodesContext = ElectrodesFactory.GetElectrodeContext(brainToLoad.GetEegTech);
        m_ElectrodesContext.LoadElectrodes(brainToLoad.pts);
        m_ElectrodesContext.LoadAtlasData(brainToLoad.atlasCSV);
        m_ElectrodesContext.LoadElectrodesOnBrain(Electrodes);

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
        
        Electrodes = new GameObject("Electrodes");
        Electrodes.transform.parent = gameObject.transform;
        m_ElectrodesContext = ElectrodesFactory.GetElectrodeContext(eeg);
        m_ElectrodesContext.LoadDefaultPearl(media.elanFiles);
        m_ElectrodesContext.LoadElectrodesOnBrain(Electrodes);

        CameraScript = GameObject.Find("CameraBrain").GetComponent<BrainCamera>();
        CameraScript.initCameraPosition();
    }

    private void OnBrainParametersMessage(UiToBrainMessage message)
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
                updateBrainMesh(media.pm.currentPatients[media.pm.idCurrentPatientLoaded].mni);
                break;
            case 1:
                LHBrain.gameObject.SetActive(true);
                RHBrain.gameObject.SetActive(true);
                updateBrainMesh(media.pm.currentPatients[media.pm.idCurrentPatientLoaded].pat);
                break;
            case 2:
                LHBrain.gameObject.SetActive(false);
                RHBrain.gameObject.SetActive(false);
                m_ElectrodesContext.UpdateElectrodesPearl(Electrodes);
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

        //TODO : in case of a change beetween ieeg and scalp eeg it will probably not work
        m_ElectrodesContext.LoadElectrodes(brainToLoad.pts);
        m_ElectrodesContext.LoadAtlasData(brainToLoad.atlasCSV);
        m_ElectrodesContext.UpdateElectrodesPosition(Electrodes);
    }

    //Left : sibling 0
    //Right : sibling 1
    private GameObject UpdateOneHemisphere(string HemisphereName, int SiblingIndex, string FilePath)
    {
        GameObject newHemisphere = new GameObject(HemisphereName, new System.Type[] { typeof(Hemisphere) });
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
