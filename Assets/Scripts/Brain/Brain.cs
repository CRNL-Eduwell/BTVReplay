using Assets.Scripts.Data.Factory;
using BTV.Services.AnatomicalDataService;
using BTV.Services.EegFileService;
using BTV.Services.SubjectInfoService;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Brain : MonoBehaviour
{
    #region Unity Scene Elements
    private BrainCamera m_BrainCamera = null;
    private GameObject m_LeftHemiBrain = null;
    private GameObject m_RightHemiBrain = null;
    private GameObject m_Electrodes = null;
    #endregion
    IElectrodesContext m_ElectrodesContext = null;
    private TraceOption m_MasterTraceOption = null;
    private int m_BrainReferentialID = -1;

    void Awake()
    {
        Messenger.Default.Register<LoaderMessage>(this, OnLoaderMessage, MessageContext.LoaderMessage);
        Messenger.Default.Register<UiToBrainMessage>(this, OnBrainParametersMessage, MessageContext.UiToBrain);
    }

    private void OnMasterTraceOptionPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        UnityEngine.Debug.Log("Brain.cs : OnMasterTraceOptionPropertyChanged");
        switch (e.PropertyName)
        {
            case "FileHandle":
                {
                    UnityEngine.Debug.Log("OnMasterTraceOptionPropertyChanged FileHandle");
                    if (m_BrainReferentialID == 2)
                    {
                        int suffix = EegFileService.GetContainerSuffix(m_MasterTraceOption.FileHandle);
                        List<AnatomicalSite> sites = AnatomicalDataService.GetSitesListFrom("ELEC", suffix);
                        UpdateBrainMesh(sites);
                    }
                    break;
                }
        }
    }

    void OnDestroy()
    {
        if(m_MasterTraceOption != null) m_MasterTraceOption.PropertyChanged -= OnMasterTraceOptionPropertyChanged;
        Messenger.Default.Unregister(this, MessageContext.LoaderMessage);
        Messenger.Default.Unregister(this, MessageContext.UiToBrain);
    }

    private void OnLoaderMessage(LoaderMessage message)
    {
        if (message.Task == LoaderMessage.LoaderTask.LoadBrain)
        {
            m_MasterTraceOption = TracesService.GetOptionsFor(0);
            m_MasterTraceOption.PropertyChanged += OnMasterTraceOptionPropertyChanged;

            UnityEngine.Debug.Log("OnLoader Message => LoadBrain");
            if (message.HasAnatomy)
            {
                LoadBrainAndElectrodes(message.Anatomy);
            }
            else
            {
                LoadElectrodesDefault(message.Techno);
            }
        }
    }

    private void LoadBrainAndElectrodes(BrainDataContainer brainToLoad)
    {
        m_LeftHemiBrain = UpdateOneHemisphere("LeftHemi", 0, brainToLoad.LeftHemisphere, brainToLoad.Transformation);

        if (brainToLoad.MeshConfiguration == MeshConfiguration.LeftRight)
        {
            m_RightHemiBrain = UpdateOneHemisphere("RightHemi", 1, brainToLoad.RightHemisphere, brainToLoad.Transformation);
        }
        else
        {
            m_RightHemiBrain = new GameObject("RightHemi");
            m_RightHemiBrain.transform.parent = gameObject.transform;
            m_RightHemiBrain.layer = gameObject.layer;
        }

        if (brainToLoad.EegTechnology == EegTechnology.Scalp)
        {
            GameObject sphere = new GameObject("HalfSphere", new System.Type[] { typeof(HalfSphere) });
            sphere.transform.parent = gameObject.transform;
            HalfSphere hs = sphere.GetComponent<HalfSphere>();
            hs.InitSphere();
            ScalpDataProjector dps = sphere.AddComponent<ScalpDataProjector>();

            //Move brain for a nice visualisation and to prevent error in uv position
            //when recalculating electrodes position this way, much simpler
            m_LeftHemiBrain.transform.position += new Vector3(0, 16, 9.85f);
            m_RightHemiBrain.transform.position += new Vector3(0, 16, 9.85f);
        }

        m_Electrodes = new GameObject("Electrodes");
        m_Electrodes.transform.parent = gameObject.transform;
        m_ElectrodesContext = ElectrodesFactory.GetElectrodeContext(brainToLoad.EegTechnology);
        KeyValuePair<string, List<AnatomicalSite>> d = AnatomicalDataService.ReturnFirstValidSitesList();
        m_BrainReferentialID = d.Key == "MNI" ? 0 : 1;

        m_ElectrodesContext.LoadElectrodesOnBrain(m_Electrodes, d.Value);

        m_BrainCamera = GameObject.Find("CameraBrain").GetComponent<BrainCamera>();
        m_BrainCamera.InitCameraPosition();
    }

    private void LoadElectrodesDefault(EegTechnology eeg)
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
        KeyValuePair<string, List<AnatomicalSite>> d = AnatomicalDataService.ReturnFirstValidSitesList();
        m_BrainReferentialID = 2;

        m_ElectrodesContext.LoadElectrodesOnBrain(m_Electrodes, d.Value);

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
                m_BrainReferentialID = message.ModelId;
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
                {
                    m_LeftHemiBrain.gameObject.SetActive(true);
                    m_RightHemiBrain.gameObject.SetActive(true);
                    BrainDataContainer mniContainer = SubjectInfoService.GetBrainDataContainer("MNI");
                    List<AnatomicalSite> sites = AnatomicalDataService.GetSitesListFrom("MNI");
                    UpdateBrainMesh(mniContainer, sites);
                    break;
                }
            case 1:
                {
                    m_LeftHemiBrain.gameObject.SetActive(true);
                    m_RightHemiBrain.gameObject.SetActive(true);
                    BrainDataContainer patContainer = SubjectInfoService.GetBrainDataContainer("MNI");
                    List<AnatomicalSite> sites = AnatomicalDataService.GetSitesListFrom("PAT");
                    UpdateBrainMesh(patContainer, sites);
                    break;
                }
            case 2:
                {
                    m_LeftHemiBrain.gameObject.SetActive(false);
                    m_RightHemiBrain.gameObject.SetActive(false);
                    int suffix = EegFileService.GetContainerSuffix(m_MasterTraceOption.FileHandle);
                    List<AnatomicalSite> sites = AnatomicalDataService.GetSitesListFrom("ELEC", suffix);
                    UpdateBrainMesh(sites);
                    break;
                }
            default:
                {
                    Debug.LogError("UpdateBrainModel => ModelId value is unknown : " + ModelId);
                    break;
                }
        }
    }

    private void UpdateBrainMesh(BrainDataContainer brainToLoad, List<AnatomicalSite> sites)
    {
        // This is a hack , we put back the brain main object at his original position and then we 
        // rmove it back at the end of the reinitialisation => TODO : check unity layer system 
        gameObject.transform.position -= new Vector3(-10000, 0, 0);
        gameObject.transform.Rotate(new Vector3(-270, 0, 0));

        Destroy(GameObject.Find("LeftHemi"));
        m_LeftHemiBrain = UpdateOneHemisphere("LeftHemi", 0, brainToLoad.LeftHemisphere, brainToLoad.Transformation);

        if (brainToLoad.MeshConfiguration == MeshConfiguration.LeftRight)
        {
            Destroy(GameObject.Find("RightHemi"));
            m_RightHemiBrain = UpdateOneHemisphere("RightHemi", 1, brainToLoad.RightHemisphere, brainToLoad.Transformation);
        }
        else
        {
            m_RightHemiBrain = new GameObject("RightHemi");
            m_RightHemiBrain.transform.parent = gameObject.transform;
            m_RightHemiBrain.layer = gameObject.layer;
        }

        Destroy(GameObject.Find("Electrodes"));
        m_Electrodes = new GameObject("Electrodes");
        m_Electrodes.transform.parent = gameObject.transform;
        m_ElectrodesContext = ElectrodesFactory.GetElectrodeContext(brainToLoad.EegTechnology);
        m_ElectrodesContext.LoadElectrodesOnBrain(m_Electrodes, sites);

        // Hack part 2 , put it back outside of the canvas
        gameObject.transform.position += new Vector3(-10000, 0, 0);
        gameObject.transform.Rotate(new Vector3(270, 0, 0));
    }

    private void UpdateBrainMesh(List<AnatomicalSite> sites)
    {
        // This is a hack , we put back the brain main object at his original position and then we 
        // rmove it back at the end of the reinitialisation => TODO : check unity layer system 
        gameObject.transform.position -= new Vector3(-10000, 0, 0);
        gameObject.transform.Rotate(new Vector3(-270, 0, 0));

        Destroy(GameObject.Find("LeftHemi"));
        m_LeftHemiBrain = new GameObject("LeftHemi");
        m_LeftHemiBrain.transform.parent = gameObject.transform;
        m_LeftHemiBrain.layer = gameObject.layer;

        Destroy(GameObject.Find("RightHemi"));
        m_RightHemiBrain = new GameObject("RightHemi");
        m_RightHemiBrain.transform.parent = gameObject.transform;
        m_RightHemiBrain.layer = gameObject.layer;

        Destroy(GameObject.Find("Electrodes"));
        m_Electrodes = new GameObject("Electrodes");
        m_Electrodes.transform.parent = gameObject.transform;

        m_ElectrodesContext.LoadElectrodesOnBrain(m_Electrodes, sites);

        // Hack part 2 , put it back outside of the canvas
        gameObject.transform.position += new Vector3(-10000, 0, 0);
        gameObject.transform.Rotate(new Vector3(270, 0, 0));
    }

    //Left : sibling 0
    //Right : sibling 1
    private GameObject UpdateOneHemisphere(string HemisphereName, int SiblingIndex, string FilePath, string trmFilePath)
    {
        GameObject newHemisphere = new GameObject(HemisphereName, new System.Type[] { typeof(Hemisphere) });
        newHemisphere.transform.parent = gameObject.transform;
        newHemisphere.transform.SetSiblingIndex(SiblingIndex);
        newHemisphere.transform.localPosition = new Vector3(0, 0, 0);
        newHemisphere.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));
        newHemisphere.layer = gameObject.layer;

        Hemisphere hemi = newHemisphere.GetComponent<Hemisphere>();
        hemi.InitializeData(FilePath, trmFilePath);
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
