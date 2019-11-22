using Assets.Scripts.Data.Factory;
using UnityEngine;

public class Brain : MonoBehaviour
{
    #region Unity Scene Elements
    private BrainCamera m_BrainCamera = null;
    private GameObject m_LeftHemiBrain = null;
    private GameObject m_RightHemiBrain = null;
    private GameObject m_Electrodes = null;
    #endregion
    IElectrodesContext m_ElectrodesContext = null;

    void Awake()
    {
        Messenger.Default.Register<LoaderToBrainMessage>(this, OnBrainLoaderMessage, MessageContext.LoaderToBrain);
        Messenger.Default.Register<UiToBrainMessage>(this, OnBrainParametersMessage, MessageContext.UiToBrain);
    }

    void OnDestroy()
    {
        Messenger.Default.Unregister(this, MessageContext.LoaderToBrain);
        Messenger.Default.Unregister(this, MessageContext.UiToBrain);
    }

    private void OnBrainLoaderMessage(LoaderToBrainMessage message)
    {
        UnityEngine.Debug.Log("Onbrainloadermessage");
        if (message.HasAnatomy)
        {
            LoadBrainAndElectrodes(message.Anatomy);
        }
        else
        {
            LoadElectrodesDefault(message.Techno);
        }
    }

    private void LoadBrainAndElectrodes(brain_anat brainToLoad)
    {
        m_LeftHemiBrain = UpdateOneHemisphere("LeftHemi", 0, brainToLoad.lhemi);

        if (brainToLoad.GetMeshNb == mesh_Configuration.leftright)
        {
            m_RightHemiBrain = UpdateOneHemisphere("RightHemi", 1, brainToLoad.rhemi);
        }
        else
        {
            m_RightHemiBrain = new GameObject("RightHemi");
            m_RightHemiBrain.transform.parent = gameObject.transform;
            m_RightHemiBrain.layer = gameObject.layer;
        }

        m_Electrodes = new GameObject("Electrodes");
        m_Electrodes.transform.parent = gameObject.transform;
        m_ElectrodesContext = ElectrodesFactory.GetElectrodeContext(brainToLoad.GetEegTech);
        m_ElectrodesContext.LoadElectrodes(brainToLoad.pts);
        m_ElectrodesContext.LoadAtlasData(brainToLoad.atlasCSV);
        m_ElectrodesContext.LoadElectrodesOnBrain(m_Electrodes);

        m_BrainCamera = GameObject.Find("CameraBrain").GetComponent<BrainCamera>();
        m_BrainCamera.InitCameraPosition();
    }

    private void LoadElectrodesDefault(eeg_Technology eeg)
    {
        m_LeftHemiBrain = new GameObject("LeftHemi", new System.Type[] { typeof(Hemisphere) });
        m_LeftHemiBrain.transform.parent = gameObject.transform;
        m_LeftHemiBrain.layer = gameObject.layer;
        m_RightHemiBrain = new GameObject("RightHemi", new System.Type[] { typeof(Hemisphere) });
        m_RightHemiBrain.transform.parent = gameObject.transform;
        m_RightHemiBrain.layer = gameObject.layer;

        m_Electrodes = new GameObject("Electrodes");
        m_Electrodes.transform.parent = gameObject.transform;
        m_ElectrodesContext = ElectrodesFactory.GetElectrodeContext(eeg);
        m_ElectrodesContext.LoadDefaultPearl();
        m_ElectrodesContext.LoadElectrodesOnBrain(m_Electrodes);

        m_BrainCamera = GameObject.Find("CameraBrain").GetComponent<BrainCamera>();
        m_BrainCamera.InitCameraPosition();
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
                //manage by each site individually
                break;
            default:
                Debug.LogError("Brain.cs : Id of action to execute does not exist : " + message.TaskToExecute);
                break;
        }
    }

    private void UpdateBrainModel(int ModelId)
    {
        switch (ModelId)
        {
            case 0:
                m_LeftHemiBrain.gameObject.SetActive(true);
                m_RightHemiBrain.gameObject.SetActive(true);
                UpdateBrainMesh(ApplicationState.Module3D.Patient.mni);
                break;
            case 1:
                m_LeftHemiBrain.gameObject.SetActive(true);
                m_RightHemiBrain.gameObject.SetActive(true);
                UpdateBrainMesh(ApplicationState.Module3D.Patient.pat);
                break;
            case 2:
                m_LeftHemiBrain.gameObject.SetActive(false);
                m_RightHemiBrain.gameObject.SetActive(false);
                m_ElectrodesContext.UpdateElectrodesPearl(m_Electrodes);
                break;
            default:
                Debug.LogError("UpdateBrainModel => ModelId value is unknown : " + ModelId);
                break;
        }
    }

    private void UpdateBrainMesh(brain_anat brainToLoad)
    {
        Destroy(GameObject.Find("LeftHemi"));
        m_LeftHemiBrain = UpdateOneHemisphere("LeftHemi", 0, brainToLoad.lhemi);

        if (brainToLoad.GetMeshNb == mesh_Configuration.leftright)
        {
            Destroy(GameObject.Find("RightHemi"));
            m_RightHemiBrain = UpdateOneHemisphere("RightHemi", 1, brainToLoad.rhemi);
        }
        else
        {
            m_RightHemiBrain = new GameObject("RightHemi");
            m_RightHemiBrain.transform.parent = gameObject.transform;
            m_RightHemiBrain.layer = gameObject.layer;
        }

        //TODO : in case of a change beetween ieeg and scalp eeg it will probably not work
        m_ElectrodesContext.LoadElectrodes(brainToLoad.pts);
        m_ElectrodesContext.LoadAtlasData(brainToLoad.atlasCSV);
        m_ElectrodesContext.UpdateElectrodesPosition(m_Electrodes);
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
                m_LeftHemiBrain.gameObject.SetActive(true);
                m_RightHemiBrain.gameObject.SetActive(false);
                break;
            case 0:
                m_LeftHemiBrain.gameObject.SetActive(true);
                m_RightHemiBrain.gameObject.SetActive(true);
                break;
            case 1:
                m_LeftHemiBrain.gameObject.SetActive(false);
                m_RightHemiBrain.gameObject.SetActive(true);
                break;
            default:
                Debug.LogError("UpdateDisplayedMeshes => MeshesId value is unknown : " + MeshesId);
                break;
        }
    }
}
