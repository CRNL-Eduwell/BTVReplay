using Assets.Scripts.Data.Factory;
using BTV.Services;
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
    private Session m_PatientSession = null;

    void Awake()
    {
        Messenger.Default.Register<LoaderMessage>(this, OnLoaderMessage, MessageContext.LoaderMessage);
        Messenger.Default.Register<UiToBrainMessage>(this, OnBrainParametersMessage, MessageContext.UiToBrain);
    }

    private void OnMasterTraceOptionPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (!Session.IsCurrent(m_PatientSession)) return;
        BtvLog.Log("Brain.cs : OnMasterTraceOptionPropertyChanged");
        switch (e.PropertyName)
        {
            case "FileHandle":
                {
                    BtvLog.Log("OnMasterTraceOptionPropertyChanged FileHandle");
                    if (m_BrainReferentialID == 2)
                    {
                        int suffix = EegFileService.GetContainerSuffix(m_PatientSession, m_MasterTraceOption.FileHandle);
                        List<AnatomicalSite> sites = AnatomicalDataService.GetSitesListFrom(m_PatientSession, "ELEC", suffix);
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
        if (message.Task == LoaderMessage.LoaderTask.LoadBrain && Session.IsCurrent(message.PatientSession))
        {
            m_PatientSession = message.PatientSession;
            m_MasterTraceOption = TracesService.GetOptionsFor(message.PatientSession, 0);
            m_MasterTraceOption.PropertyChanged += OnMasterTraceOptionPropertyChanged;

            BtvLog.Log("OnLoader Message => LoadBrain");
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
            m_RightHemiBrain = CreateChild("RightHemi");
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

        m_Electrodes = CreateChild("Electrodes");
        m_ElectrodesContext = ElectrodesFactory.GetElectrodeContext(brainToLoad.EegTechnology);
        KeyValuePair<string, List<AnatomicalSite>> d = AnatomicalDataService.ReturnFirstValidSitesList(m_PatientSession);
        m_BrainReferentialID = d.Key == "MNI" ? 0 : 1;

        m_ElectrodesContext.LoadElectrodesOnBrain(m_Electrodes, d.Value, InitializeSite);

        m_BrainCamera = GameObject.Find("CameraBrain").GetComponent<BrainCamera>();
        m_BrainCamera.InitCameraPosition();
    }

    private void InitializeSite(Site site, AnatomicalSite anatomicalSite)
    {
        site.Init(m_PatientSession, anatomicalSite);
    }

    private void LoadElectrodesDefault(EegTechnology eeg)
    {
        m_LeftHemiBrain = CreateChild("LeftHemi", typeof(Hemisphere));
        m_RightHemiBrain = CreateChild("RightHemi", typeof(Hemisphere));

        m_Electrodes = CreateChild("Electrodes");
        m_ElectrodesContext = ElectrodesFactory.GetElectrodeContext(eeg);
        KeyValuePair<string, List<AnatomicalSite>> d = AnatomicalDataService.ReturnFirstValidSitesList(m_PatientSession);
        m_BrainReferentialID = 2;

        m_ElectrodesContext.LoadElectrodesOnBrain(m_Electrodes, d.Value, InitializeSite);

        m_BrainCamera = GameObject.Find("CameraBrain").GetComponent<BrainCamera>();
        m_BrainCamera.InitCameraPosition();
    }

    private void OnBrainParametersMessage(UiToBrainMessage message)
    {
        if (!Session.IsCurrent(m_PatientSession)) return;
        BtvLog.Log("Brain Message, yata");
        switch (message.TaskToExecute)
        {
            case UiToBrainMessage.Task.ChangeReferential:
                BtvLog.Log("Update Brain Model");
                m_BrainReferentialID = message.ModelId;
                UpdateBrainModel(message.ModelId);
                break;
            case UiToBrainMessage.Task.ChangeMeshDisplay:
                BtvLog.Log("Update Brain Visu");
                UpdateDisplayedMeshes(message.MeshesToDisplay);
                break;
            case UiToBrainMessage.Task.UpdateGain:
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
                    BrainDataContainer mniContainer = SubjectInfoService.GetBrainDataContainer(m_PatientSession, "MNI");
                    List<AnatomicalSite> sites = AnatomicalDataService.GetSitesListFrom(m_PatientSession, "MNI");
                    UpdateBrainMesh(mniContainer, sites);
                    break;
                }
            case 1:
                {
                    m_LeftHemiBrain.gameObject.SetActive(true);
                    m_RightHemiBrain.gameObject.SetActive(true);
                    BrainDataContainer patContainer = SubjectInfoService.GetBrainDataContainer(m_PatientSession, "PAT");
                    List<AnatomicalSite> sites = AnatomicalDataService.GetSitesListFrom(m_PatientSession, "PAT");
                    UpdateBrainMesh(patContainer, sites);
                    break;
                }
            case 2:
                {
                    m_LeftHemiBrain.gameObject.SetActive(false);
                    m_RightHemiBrain.gameObject.SetActive(false);
                    int suffix = EegFileService.GetContainerSuffix(m_PatientSession, m_MasterTraceOption.FileHandle);
                    List<AnatomicalSite> sites = AnatomicalDataService.GetSitesListFrom(m_PatientSession, "ELEC", suffix);
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

    // Rebuilds build in the brain's local space (hemispheres, CreateChild, the electrode
    // contexts), so they no longer move the brain back from its off-canvas x=-10000 pose and
    // then out again around the rebuild.
    private void UpdateBrainMesh(BrainDataContainer brainToLoad, List<AnatomicalSite> sites)
    {
        DestroyBuiltChildren();

        m_LeftHemiBrain = UpdateOneHemisphere("LeftHemi", 0, brainToLoad.LeftHemisphere, brainToLoad.Transformation);

        if (brainToLoad.MeshConfiguration == MeshConfiguration.LeftRight)
            m_RightHemiBrain = UpdateOneHemisphere("RightHemi", 1, brainToLoad.RightHemisphere, brainToLoad.Transformation);
        else
            m_RightHemiBrain = CreateChild("RightHemi");

        m_Electrodes = CreateChild("Electrodes");
        m_ElectrodesContext = ElectrodesFactory.GetElectrodeContext(brainToLoad.EegTechnology);
        m_ElectrodesContext.LoadElectrodesOnBrain(m_Electrodes, sites, InitializeSite);
    }

    private void UpdateBrainMesh(List<AnatomicalSite> sites)
    {
        DestroyBuiltChildren();

        m_LeftHemiBrain = CreateChild("LeftHemi");
        m_RightHemiBrain = CreateChild("RightHemi");
        m_Electrodes = CreateChild("Electrodes");

        m_ElectrodesContext.LoadElectrodesOnBrain(m_Electrodes, sites, InitializeSite);
    }

    // Destroys what the previous build created, by reference. This used to be
    // Destroy(GameObject.Find("LeftHemi")) and so on, but Find skips inactive objects: hemispheres
    // hidden by the electrode-only model were never destroyed (their meshes leaked), and a
    // single-mesh model never destroyed the previous right hemisphere, which stayed on screen.
    private void DestroyBuiltChildren()
    {
        if (m_LeftHemiBrain != null) Destroy(m_LeftHemiBrain);
        if (m_RightHemiBrain != null) Destroy(m_RightHemiBrain);
        if (m_Electrodes != null) Destroy(m_Electrodes);
    }

    private GameObject CreateChild(string name, params System.Type[] components)
    {
        GameObject child = new GameObject(name, components);
        child.transform.SetParent(gameObject.transform, false);
        child.layer = gameObject.layer;
        return child;
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
