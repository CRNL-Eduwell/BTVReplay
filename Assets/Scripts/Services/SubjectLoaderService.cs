using System.IO;
using System.Collections; //IEnumerator
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using SFB;
using BTV.Data;
using BTV.Services.EegFileService;
using System.Linq;
using System.Collections.Generic;
using BTV.Services;
using BTV.Services.AnatomicalDataService;
using BTV.Services.SubjectInfoService;

public class SubjectLoaderService : MonoBehaviour
{
    public bool loaded = false;

    #region UILoadingCircle
    [SerializeField] private GameObject loadingCirclePrefab = null;
    private LoadingCircle loadingCircle = null;
    #endregion

    private void Awake()
    {
        Messenger.Default.Register<LoadSubjectMessage>(this, OnLoadSubjectMessage, MessageContext.LoadSubjectMessage);    
    }

    private void OnDestroy()
    {
        Messenger.Default.Unregister(this, MessageContext.LoadSubjectMessage);
    }

    private void OnLoadSubjectMessage(LoadSubjectMessage message)
    {
        if (loaded == true)
            ResetValues(message.subject, message.label);
        else
            StartCoroutine(c_load(message.subject, message.label));
    }

    private IEnumerator c_load(Subject subject, string experimentName)
    {
        SubjectInfoService.SetSubject(subject, experimentName);
        
        yield return StartCoroutine(c_loadEEGFile(SubjectInfoService.GetSubjectFilesAndDescription()));
        TracesService.InitTraces();
        TimeFrequencyService.InitTraces();

        LoaderMessage message = new LoaderMessage
        {
            Task = LoaderMessage.LoaderTask.MediaLoader
        };
        Messenger.Default.Send(message, MessageContext.LoaderMessage);

        yield return StartCoroutine(c_loadVideo(SubjectInfoService.VideoPath));
        yield return StartCoroutine(c_LoadBrainAnatomy(subject));

        message = new LoaderMessage
        {
            Task = LoaderMessage.LoaderTask.LoadTrace
        };
        Messenger.Default.Send(message, MessageContext.LoaderMessage);

        //===============
        yield return new WaitForSeconds(0.1f);

        //kind of an ugly way to deactivate perf at launch time, see to do that by instantiating
        //the window only when needed 
        GameObject.Find("ButtonPerf").GetComponent<ExtendedToggle>().ForceStartValue(0);

        //When everything is loaded we close the loading brain and media panel
        loadingCircle.Close();
        loaded = true;
        Text PatientNameHeader = GameObject.Find("HeaderDisplay").transform.GetChild(0).GetComponent<Text>();
        PatientNameHeader.text = subject.PatientName;
        ApplicationState.init();
        yield return new WaitForSeconds(0.1f);
    }

    private IEnumerator c_LoadBrainAnatomy(Subject subject)
    {
        bool hasMniContainer = subject.AnatomicalSpaces.TryGetValue("MNI", out BrainDataContainer mniContainer);
        bool hasPatContainer = subject.AnatomicalSpaces.TryGetValue("PAT", out BrainDataContainer patContainer);

        if (hasMniContainer && mniContainer.Pts != "") yield return StartCoroutine(AnatomicalDataService.c_Load("MNI", mniContainer));
        if (hasPatContainer && patContainer.Pts != "") yield return StartCoroutine(AnatomicalDataService.c_Load("PAT", patContainer));

        //TODO
        //When there is no 3D model , we take the value of the mni dropdown for eegtech
        //if this is not filled this might be wrong, need to find another way to know
        //if it's intra or scalp
        yield return AnatomicalDataService.c_LoadDefaultElectrodes(mniContainer.EegTechnology);

        //TODO 
        //atlas is loaded in the service and now we'll need to link atlas info in visualisation part 
        yield return AnatomicalDataService.c_LoadAtlas(patContainer.Atlas);

        bool ShouldLoadMniFirst = false, ShouldLoadPatFirst = false;
        if (hasMniContainer && hasPatContainer)
        {
            ShouldLoadMniFirst = (mniContainer.HasAnat && !patContainer.HasAnat) || (mniContainer.HasAnat && patContainer.HasAnat);
            ShouldLoadPatFirst = !mniContainer.HasAnat && patContainer.HasAnat;
        }
        else if (hasMniContainer && !hasPatContainer)
        {
            ShouldLoadMniFirst = mniContainer.HasAnat;
            ShouldLoadPatFirst = false;
        }
        else if (!hasMniContainer && hasPatContainer)
        {
            ShouldLoadMniFirst = false;
            ShouldLoadPatFirst = patContainer.HasAnat;
        }
        else
        {
            ShouldLoadMniFirst = false;
            ShouldLoadPatFirst = false;
        }

        if (ShouldLoadMniFirst)
        {
            LoaderMessage message = new LoaderMessage
            {
                Task = LoaderMessage.LoaderTask.LoadBrain,
                HasAnatomy = true,
                Anatomy = mniContainer
            };
            Messenger.Default.Send(message, MessageContext.LoaderMessage);
        }
        else if (ShouldLoadPatFirst)
        {
            LoaderMessage message = new LoaderMessage
            {
                Task = LoaderMessage.LoaderTask.LoadBrain,
                HasAnatomy = true,
                Anatomy = patContainer
            };
            Messenger.Default.Send(message, MessageContext.LoaderMessage);
        }
        else
        {
            //TODO
            //When there is no 3D model , we take the value of the mni dropdown for eegtech
            //if this is not filled this might be wrong, need to find another way to know
            //if it's intra or scalp
            LoaderMessage message = new LoaderMessage
            {
                Task = LoaderMessage.LoaderTask.LoadBrain,
                HasAnatomy = false,
                Techno = EegTechnology.Intra
            };
            Messenger.Default.Send(message, MessageContext.LoaderMessage);
        }

        yield return null;
    }

    private IEnumerator c_loadEEGFile(List<KeyValuePair<string, IEegFileInfo>> eegfiles)
    {
        loadingCircle = (Instantiate(loadingCirclePrefab, Vector3.zero, Quaternion.identity, GameObject.Find("CircleWindow").transform) as GameObject).GetComponent<LoadingCircle>();
        loadingCircle.transform.localPosition = new Vector3(0, 0, 0);

        loadingCircle.Set(0, "Finding files");

        for (int i = 0; i < eegfiles.Count; i++)
        {
            loadingCircle.Set(0.1f + ((0.9f / eegfiles.Count) * i), "Loading File " + (i+1));
            Task loadTask = LoadFile(eegfiles[i], i);
            if (loadTask != null)
            {
                yield return new WaitUntil(() => loadTask.IsCompleted);
                if (loadTask.IsFaulted)
                {
                    // A failed file must not abort the others; surface it and keep loading.
                    UnityEngine.Debug.LogError("Could not load EEG file " + (i + 1) + " (" + eegfiles[i].Key + ").");
                    UnityEngine.Debug.LogException(loadTask.Exception.GetBaseException());
                }
            }
        }
        loadingCircle.Set(1f, "Files have been loaded");
    }

    private Task LoadFile(KeyValuePair<string, IEegFileInfo> kvp, int FileID)
    {
        if (!kvp.Equals(default(KeyValuePair<string, IEegFileInfo>)))
        {
            // The native read runs on a worker so the UI does not freeze; the montage slot is
            // assigned back on the main thread inside LoadAsync.
            return EegFileService.LoadAsync(kvp.Value, FileID, kvp.Key);
        }

        return null;
    }

    private IEnumerator c_loadVideo(string videoPath)
    {
        //load video
        BtvProgram container = EegFileService.ReturnFirstValidContainer();
        if (container == null)
            UnityEngine.Debug.LogWarning("c_loadVideo: no valid EEG container loaded; the video will use its own duration instead of an EEG-based one.");

        LoaderMessage message = new LoaderMessage
        {
            Task = LoaderMessage.LoaderTask.LoadVideo,
            VideoPath = videoPath,
            totalFileDuration = container != null ? container.TotalDurationInMilliseconds : -1
        };
        Messenger.Default.Send(message, MessageContext.LoaderMessage);

        yield return null;
    }

    private void ResetValues(Subject subject, string experimentName)
    {
        GameObject reloadGameObject = Instantiate(Resources.Load("Prefabs/Media-Reload", typeof(GameObject))) as GameObject;
        reloadGameObject.name = "ReloadMedia";

        ReloadMedia r = reloadGameObject.GetComponent<ReloadMedia>();
        r.SubjectToReload = new Subject(subject);
        r.ExperimentName = experimentName;
        r.TriggerReload = true;

        // LoadScene completes later in the frame. Stop the outgoing player now so its final
        // Update cannot broadcast ticks after the patient session has been replaced.
        CustomVideoPlayer outgoingVideoPlayer = FindAnyObjectByType<CustomVideoPlayer>();
        if (outgoingVideoPlayer != null)
            outgoingVideoPlayer.enabled = false;

        // Start the new patient lifetime before the replacement scene's Awake methods run.
        // Components created by that scene can then safely subscribe to session-scoped events.
        Session.ReplaceCurrent();
        SceneManager.LoadScene("_main");
    }
}
