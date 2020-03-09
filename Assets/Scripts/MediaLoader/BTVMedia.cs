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

public delegate void mediaLoadedEventHandler();
public delegate void initTrace();
public delegate void initVideo(string videoPath, int totalFileDuration);

public class BTVMedia : MonoBehaviour
{
    public event mediaLoadedEventHandler mediaLoaded;
    public event initTrace loadTrace;
    public event initVideo loadVideo;
    
    #region UILoadingCircle
    [SerializeField] GameObject loadingCirclePrefab = null;
    LoadingCircle loadingCircle = null;
    #endregion

    #region UIMembers
    private Button showAddPanel = null;
    private Image showAddPic = null;
    private PatientGUIManager addPatientPanel = null; // private Transform addPatientPanel = null;
    private Button addPatient = null;           
    //===
    private Transform patientContent = null;    /*||*/
    private GameObject patientTemplate = null;  /*||*/
    private GameObject patDetailTemplate = null;/*||*/
    private Button saveBase = null;             /*||*/     private Button loadBase = null;  /*||*/  private Button loadBUBase = null;
    //===
    #endregion

    #region members
    public DBManager pm = new DBManager();
    public bool loaded = false;
    #endregion

    void Awake()
    {
        patientTemplate = Resources.Load("Prefabs/Media-PatientX", typeof(GameObject)) as GameObject;
        patDetailTemplate = Resources.Load("Prefabs/Media-InfoPatient", typeof(GameObject)) as GameObject;

        #region getObjectFromScene
        showAddPanel = gameObject.transform.GetChild(0).GetChild(0).GetChild(1).GetComponent<Button>();
        showAddPic = showAddPanel.gameObject.GetComponent<Image>();
        addPatientPanel = gameObject.transform.GetChild(0).GetChild(1).GetChild(0).GetComponent<PatientGUIManager>();
        addPatient = gameObject.transform.GetChild(0).GetChild(1).GetChild(0).GetChild(3).GetChild(0).GetComponent<Button>(); // 0 1 0

        patientContent = gameObject.transform.GetChild(0).GetChild(1).GetChild(1).GetChild(1).GetChild(0).GetChild(0).GetChild(0);
        saveBase = gameObject.transform.GetChild(0).GetChild(1).GetChild(1).GetChild(2).GetChild(0).GetComponent<Button>();
        loadBase = gameObject.transform.GetChild(0).GetChild(1).GetChild(1).GetChild(2).GetChild(1).GetComponent<Button>();
        loadBUBase = gameObject.transform.GetChild(0).GetChild(1).GetChild(1).GetChild(2).GetChild(2).GetComponent<Button>();
        #endregion

        #region addListener
        showAddPanel.onClick.AddListener(() => 
        {
            addPatientPanel.transform.gameObject.SetActive(!addPatientPanel.transform.gameObject.activeSelf);
            if (addPatientPanel.transform.gameObject.activeSelf)
            {
                showAddPic.transform.Rotate(new Vector3(0, 0, -90));
            }
            else
            {
                showAddPic.transform.Rotate(new Vector3(0, 0, 90));
            }
        });
        addPatient.onClick.AddListener(() => { addPatientToDB(); });
        saveBase.onClick.AddListener(() => { SaveDB(); });
        loadBase.onClick.AddListener(() => 
        {
            string bddFilePath = FileBrowser.GetExistingFileName(new string[] { "txt", "dbtv" }, "Select a bdd file", Application.dataPath + @"/Config/PatientBase");
            if (bddFilePath != "")
            {
                pm.LoadList(false, bddFilePath);
                InstantiateDB();
            }
        });
        loadBUBase.onClick.AddListener(() => 
        {
            string bddFilePath = FileBrowser.GetExistingFileName(new string[] { "txt", "dbtv" }, "Select a bdd backup file", Application.dataPath + @"/Config/PatientBase");
            if (bddFilePath != "")
            {
                pm.LoadList(true, bddFilePath);
                InstantiateDB();
            }
        });
        #endregion

        //TODO : 
        //ugly patch to prevent an undefined reference if we click on show panel
        //twice at the begining without having shown the panel to add patient
        showAddPanel.onClick.Invoke();
        showAddPanel.onClick.Invoke();
    }

    void OnDestroy()
    {
        showAddPanel.onClick.RemoveAllListeners();
        addPatient.onClick.RemoveAllListeners();
        saveBase.onClick.RemoveAllListeners();
        loadBase.onClick.RemoveAllListeners();
        loadBUBase.onClick.RemoveAllListeners();

        for (int i = 0; i < patientContent.childCount; i += 2)
        {
            Button currentShowMe = patientContent.GetChild(i).GetChild(0).GetComponent<Button>();
            Button loadMe = patientContent.GetChild(i).GetChild(2).GetComponent<Button>();
            Button deleteMe = patientContent.GetChild(i).GetChild(3).GetComponent<Button>();
            currentShowMe.onClick.RemoveAllListeners();
            loadMe.onClick.RemoveAllListeners();
            deleteMe.onClick.RemoveAllListeners();
        }
        for (int i = patientContent.childCount - 1; i >= 0; i--)
        {
            Destroy(patientContent.GetChild(i).gameObject);
        }
    }

    public void showMe()
    {
        if (gameObject.activeSelf)
        {
            gameObject.SetActive(false);
            addPatientPanel.SetSubjectToGUI(new Subject());
        }
        else
        {
            gameObject.SetActive(true);
        }
    }

    void addPatientToDB()
    {
        pm.AddSubject(addPatientPanel.GetSubjectsFromGUI());
        if (patientContent.childCount == 0)
            InstantiateDB();
        else
            loadOnePatient(pm.Subjects.Count - 1); 
    }

    void loadPatientGUI()
    {
        loadMedia(addPatientPanel.GetSubjectsFromGUI());
    }

    void SaveDB()
    {
        if (patientContent.childCount > 0)
        {
            for (int i = 0; i < patientContent.childCount; i += 2)
            {
                PatientGUIManager guiPat = patientContent.transform.GetChild(i + 1).GetComponent<PatientGUIManager>();
                pm.Subjects[i / 2] = guiPat.GetSubjectsFromGUI();
            }

            var extensionList = new[] { new ExtensionFilter("BrainTV BDD File", "dbtv")};
            string bddFilePath = FileBrowser.GetSavedFileName(extensionList, "Save to a bdd File", Application.dataPath + @"/Config/PatientBase");
            if (bddFilePath != "")
            {
                pm.SaveList(bddFilePath);
                pm.LoadList(false, bddFilePath);
                InstantiateDB();
            }
        }
    }

    void InstantiateDB()
    {
        if (patientContent.childCount > 0)
        {
            for (int i = 0; i < patientContent.childCount; i++)
                Destroy(patientContent.GetChild(i).gameObject);
        }

        for (int i = 0; i < pm.Subjects.Count; i++)
            loadOnePatient(i);
    }

    void loadOnePatient(int currentID)
    {
        GameObject currentPat = Instantiate(patientTemplate);
        GameObject currentDetails = Instantiate(patDetailTemplate);
        currentPat.name = "pat" + currentID;
        currentPat.transform.SetParent(patientContent);
        currentPat.transform.localScale = new Vector3(1, 1, 1);
        currentDetails.name = "patDetails" + currentID;
        currentDetails.transform.SetParent(patientContent);
        currentDetails.transform.localScale = new Vector3(1, 1, 1);
        currentDetails.SetActive(true);
        currentDetails.SetActive(false);

        loadDataOnePatient(currentPat, currentDetails, currentID);
    }

    void loadDataOnePatient(GameObject patientBar, GameObject patientDetails, int idPat)
    {
        #region UIMembers
        Button currentShowMe = null;
        InputField myName = null;
        Image myPic = null;
        Button loadMe = null;
        Button deleteMe = null;
        #endregion

        #region getObjectFromScene
        currentShowMe = patientBar.transform.GetChild(0).GetComponent<Button>();
        myPic = patientBar.transform.GetChild(0).GetComponent<Image>();
        myName = patientBar.transform.GetChild(1).GetComponent<InputField>();
        myName.text = pm.Subjects[idPat].PatientName;
        loadMe = patientBar.transform.GetChild(2).GetComponent<Button>();
        deleteMe = patientBar.transform.GetChild(3).GetComponent<Button>();
        #endregion

        #region listeners
        currentShowMe.onClick.AddListener(() =>
        {
            patientDetails.SetActive(!patientDetails.activeSelf);
            if (patientDetails.activeSelf)
            {
                myPic.transform.Rotate(new Vector3(0, 0, -90));
            }
            else
            {
                myPic.transform.Rotate(new Vector3(0, 0, 90));
            }
        });

        loadMe.onClick.AddListener(() =>
        {
            pm.idCurrentPatientLoaded = idPat;
            loadMedia(pm.Subjects[idPat]);
        });

        deleteMe.onClick.AddListener(() =>
        {
            ApplicationState.displayConfirmation("Deleting Patient", "Are You Sure You Want To Delete This Patient From The Base ?",
                () => { pm.RemoveSubjectAt(idPat); InstantiateDB(); },
                () => { });
        });
        #endregion

        patientDetails.GetComponent<PatientGUIManager>().SetSubjectToGUI(pm.Subjects[idPat]);
    }

    public void loadMedia(Subject subject)
    {
        if (loaded == true)
            resetValue(subject);
        else
            StartCoroutine(c_load(subject));
    }

    IEnumerator c_load(Subject subject)
    {
        ApplicationState.Module3D.Patient = subject;

        yield return StartCoroutine(c_loadEEGFile(subject));
        mediaLoaded();
        yield return StartCoroutine(c_loadVideo(subject.Video));
        yield return StartCoroutine(c_LoadBrainAnatomy(subject));
        loadTrace();

        //When everything is loaded we close the loading brain and media panel
        loadingCircle.Close();
        gameObject.SetActive(false);
        loaded = true;
        Text PatientNameHeader = GameObject.Find("HeaderDisplay").transform.GetChild(0).GetComponent<Text>();
        PatientNameHeader.text = subject.PatientName;
        ApplicationState.init();
        yield return new WaitForSeconds(0.1f);
    }

    IEnumerator c_LoadBrainAnatomy(Subject subject)
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
            LoaderToBrainMessage message = new LoaderToBrainMessage
            {
                HasAnatomy = true,
                Anatomy = mniContainer
            };
            Messenger.Default.Send(message, MessageContext.LoaderToBrain);
        }
        else if (ShouldLoadPatFirst)
        {
            LoaderToBrainMessage message = new LoaderToBrainMessage
            {
                HasAnatomy = true,
                Anatomy = patContainer
            };
            Messenger.Default.Send(message, MessageContext.LoaderToBrain);
        }
        else
        {
            //TODO
            //When there is no 3D model , we take the value of the mni dropdown for eegtech
            //if this is not filled this might be wrong, need to find another way to know
            //if it's intra or scalp
            LoaderToBrainMessage message = new LoaderToBrainMessage
            {
                HasAnatomy = false,
                Techno = EegTechnology.Intra
            };
            Messenger.Default.Send(message, MessageContext.LoaderToBrain);
        }

        yield return null;
    }

    IEnumerator c_loadEEGFile(Subject subject)
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

    YieldInstruction Process(Subject subject, int FileID)
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

    IEnumerator c_loadVideo(string videoPath)
    {
        //load video
        BtvProgram container = EegFileService.ReturnFirstValidContainer();
        loadVideo(videoPath, container.TotalDurationInSeconds);

        yield return null;
    }

    void resetValue(Subject subject)
    {
        GameObject reloadGameObject = Instantiate(Resources.Load("Prefabs/Media-Reload", typeof(GameObject))) as GameObject;
        reloadGameObject.name = "ReloadMedia";

        ReloadMedia r = reloadGameObject.GetComponent<ReloadMedia>();

        bool hasMniContainer = subject.AnatomicalSpaces.TryGetValue("MNI", out BrainDataContainer mniContainer);
        bool hasPatContainer = subject.AnatomicalSpaces.TryGetValue("PAT", out BrainDataContainer patContainer);

        r.lhemi_MNI = mniContainer.LeftHemisphere;
        r.rhemi_MNI = mniContainer.RightHemisphere;
        r.pts_MNI = mniContainer.Pts;

        r.lhemi_PAT = patContainer.LeftHemisphere;
        r.rhemi_PAT = patContainer.RightHemisphere;
        r.pts_PAT = patContainer.Pts;
        r.atlas_PAT = patContainer.Atlas;

        //r.sm0 = subject.smFiles[0];
        //r.sm250 = subject.smFiles[1];
        //r.sm500 = subject.smFiles[2];
        //r.sm1000 = subject.smFiles[3];
        //r.sm2500 = subject.smFiles[4];
        //r.sm5000 = subject.smFiles[5];

        r.video = subject.Video;
        r.id = pm.idCurrentPatientLoaded;
        r.path = pm.pathFile;

        SceneManager.LoadScene("_main");
    }
}
