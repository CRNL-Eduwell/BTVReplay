using System.IO;
using System.Collections; //IEnumerator

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using SFB;
using BTV.Data;
using CielaSpike;
using BTV.Services.EegFileService;
using System.Linq;
using System.Collections.Generic;
using BTV.Services;

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
        LoadSubject(message.subject);
    }

    private void LoadSubject(Subject subject)
    {
        if (loaded == true)
            ResetValues(subject);
        else
            StartCoroutine(c_load(subject));
    }

    private IEnumerator c_load(Subject subject)
    {
        ApplicationState.Module3D.Patient = subject;

        yield return StartCoroutine(c_loadEEGFile(subject));
        LoaderMessage message = new LoaderMessage
        {
            Task = LoaderMessage.LoaderTask.MediaLoader
        };
        Messenger.Default.Send(message, MessageContext.LoaderMessage);

        yield return StartCoroutine(c_loadVideo(subject.Video));
        yield return StartCoroutine(c_LoadBrainAnatomy(subject));
        message = new LoaderMessage
        {
            Task = LoaderMessage.LoaderTask.LoadTrace
        };
        Messenger.Default.Send(message, MessageContext.LoaderMessage);

        //When everything is loaded we close the loading brain and media panel
        loadingCircle.Close();
        gameObject.SetActive(false);
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

    private IEnumerator c_loadEEGFile(Subject subject)
    {
        loadingCircle = (Instantiate(loadingCirclePrefab, Vector3.zero, Quaternion.identity, GameObject.Find("CircleWindow").transform) as GameObject).GetComponent<LoadingCircle>();
        loadingCircle.transform.localPosition = new Vector3(0, 0, 0);

        yield return Ninja.JumpToUnity;
        loadingCircle.Set(0, "Finding files");

        loadingCircle.Set(0.1f, "Loading File 1");
        yield return Ninja.JumpBack;
        yield return Process(subject, 0);
        yield return Ninja.JumpToUnity;

        loadingCircle.Set(0.2f, "Loading File 2");
        yield return Ninja.JumpBack;
        yield return Process(subject, 1);
        yield return Ninja.JumpToUnity;

        loadingCircle.Set(0.4f, "Loading File 3");
        yield return Ninja.JumpBack;
        yield return Process(subject, 2);
        yield return Ninja.JumpToUnity;

        loadingCircle.Set(0.6f, "Loading File 4");
        yield return Ninja.JumpBack;
        yield return Process(subject, 3);
        yield return Ninja.JumpToUnity;

        loadingCircle.Set(0.8f, "Loading File 5");
        yield return Ninja.JumpBack;
        yield return Process(subject, 4);
        yield return Ninja.JumpToUnity;

        loadingCircle.Set(1.0f, "Loading File 6");
        yield return Ninja.JumpBack;
        yield return Process(subject, 5);
        yield return Ninja.JumpToUnity;
    }

    private YieldInstruction Process(Subject subject, int FileID)
    {
        KeyValuePair<string, IEegFileInfo> kvp = subject.Files.ElementAtOrDefault(FileID);
        if (!kvp.Equals(default(KeyValuePair<string, IEegFileInfo>)))
        {
            // I give my callback to the process
            // Async needed for another thread and not freezing/laging UI
            return this.StartCoroutineAsync(EegFileService.c_Load(kvp.Value, FileID));
        }

        return null;
    }

    private IEnumerator c_loadVideo(string videoPath)
    {
        //load video
        BtvProgram container = EegFileService.ReturnFirstValidContainer();
        LoaderMessage message = new LoaderMessage
        {
            Task = LoaderMessage.LoaderTask.LoadVideo,
            VideoPath = videoPath,
            totalFileDuration = container.TotalDurationInSeconds
        };
        Messenger.Default.Send(message, MessageContext.LoaderMessage);

        yield return null;
    }

    private void ResetValues(Subject subject)
    {
        GameObject reloadGameObject = Instantiate(Resources.Load("Prefabs/Media-Reload", typeof(GameObject))) as GameObject;
        reloadGameObject.name = "ReloadMedia";

        ReloadMedia r = reloadGameObject.GetComponent<ReloadMedia>();
        r.SubjectToReload = new Subject(subject);
        r.Id = 0;// pm.idCurrentPatientLoaded;
        r.Path = "";// string.Copy(pm.pathFile);
        r.TriggerReload = true;
        SceneManager.LoadScene("_main");
    }
}
