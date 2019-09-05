using System;
using System.Collections; //IEnumerator

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using CielaSpike;
using BTV.Services.EegFileService;
using BTV.Data.DataContainer;
using System.Linq;
using System.Collections.Generic;

public delegate void mediaLoadedEventHandler();
public delegate void initTrace();
public delegate void initVideo(string videoPath, int totalFileDuration);
public delegate void initPerf(bool init);

public class BTVMedia : MonoBehaviour
{
    public event mediaLoadedEventHandler mediaLoaded;
    public event initTrace loadTrace;
    public event initVideo loadVideo;
    public event initPerf loadPerf;
    
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
    public POS posFile = null;
    //public ELAN[] elanFiles = new ELAN[6];
    public PROV provFile = null;
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
            string bddFilePath = FileBrowser.getOpenFileName(new string[] { "txt" }, "Select a bdd file", Application.dataPath + @"/Config/PatientBase");
            if (bddFilePath != "")
            {
                pm.LoadList(false, bddFilePath);
                InstantiateDB();
            }
        });
        loadBUBase.onClick.AddListener(() => 
        {
            string bddFilePath = FileBrowser.getOpenFileName(new string[] { "txt" }, "Select a bdd backup file", Application.dataPath + @"/Config/PatientBase");
            if (bddFilePath != "")
            {
                pm.LoadList(true, bddFilePath);
                InstantiateDB();
            }
        });
        #endregion
    }

    void OnDestroy()
    {
        showAddPanel.onClick.RemoveAllListeners();
        addPatient.onClick.RemoveAllListeners();
        saveBase.onClick.RemoveAllListeners();
        loadBase.onClick.RemoveAllListeners();
        loadBUBase.onClick.RemoveAllListeners();

        //for (int i = 0; i < elanFiles.Length; i++)
        //{
        //    if (elanFiles[i] != null)
        //        elanFiles[i].Dispose();
        //}

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
            addPatientPanel.setPatientGUI(new Patient());
        }
        else
        {
            gameObject.SetActive(true);
        }
    }

    void addPatientToDB()
    {
        pm.addPat(addPatientPanel.getPatientGUI());
        if (patientContent.childCount == 0)
            InstantiateDB();
        else
            loadOnePatient(pm.currentPatients.Count - 1); 
    }

    void loadPatientGUI()
    {
        loadMedia(addPatientPanel.getPatientGUI());
    }

    void SaveDB()
    {
        if (patientContent.childCount > 0)
        {
            for (int i = 0; i < patientContent.childCount; i += 2)
            {
                PatientGUIManager guiPat = patientContent.transform.GetChild(i + 1).GetComponent<PatientGUIManager>();
                pm.currentPatients[i / 2] = guiPat.getPatientGUI();
            }

            string bddFilePath = FileBrowser.getSaveFileName(new string[] { "txt" }, "Save to a bdd File", Application.dataPath + @"/Config/PatientBase");
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

        for (int i = 0; i < pm.currentPatients.Count; i++)
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
        Text myName = null;
        Image myPic = null;
        Button loadMe = null;
        Button deleteMe = null;
        #endregion

        #region getObjectFromScene
        currentShowMe = patientBar.transform.GetChild(0).GetComponent<Button>();
        myPic = patientBar.transform.GetChild(0).GetComponent<Image>();
        myName = patientBar.transform.GetChild(1).GetComponent<Text>();
        myName.text = pm.currentPatients[idPat].patientName;
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
            loadMedia(pm.currentPatients[idPat]);
        });

        deleteMe.onClick.AddListener(() =>
        {
            ApplicationState.displayConfirmation("Deleting Patient", "Are You Sure You Want To Delete This Patient From The Base ?",
                () => { pm.removePatientAt(idPat); InstantiateDB(); },
                () => { });
        });
        #endregion

        patientDetails.GetComponent<PatientGUIManager>().setPatientGUI(pm.currentPatients[idPat]);
    }

    public void loadMedia(Patient myPat)
    {
        if (loaded == true)
            resetValue(myPat);
        else
            StartCoroutine(c_load(myPat));
    }

    IEnumerator c_load(Patient myPat)
    {
        ApplicationState.Patient = myPat;

        //When you start a coroutine there is an implicit jumpback
        //So we jump back to unity just in case
        yield return Ninja.JumpToUnity;
        yield return StartCoroutine(c_loadEEGFile(myPat));
        //ApplicationState.EegFiles = elanFiles;
        mediaLoaded();

        yield return Ninja.JumpToUnity;
        yield return StartCoroutine(c_loadVideo(myPat.video));

        //====
        bool ShouldLoadMniFirst = (myPat.hasMNI && !myPat.hasPAT) || (myPat.hasMNI && myPat.hasPAT);
        bool ShouldLoadPatFirst = !myPat.hasMNI && myPat.hasPAT;
        if (ShouldLoadMniFirst)
        {
            LoaderToBrainMessage message = new LoaderToBrainMessage
            {
                HasAnatomy = true,
                Anatomy = myPat.mni
            };
            Messenger.Default.Send(message, MessageContext.LoaderToBrain);
        }
        else if (ShouldLoadPatFirst)
        {
            LoaderToBrainMessage message = new LoaderToBrainMessage
            {
                HasAnatomy = true,
                Anatomy = myPat.pat
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
                Techno = myPat.mni.GetEegTech
            };
            Messenger.Default.Send(message, MessageContext.LoaderToBrain);
        }
        //====

        loadTrace();

        yield return Ninja.JumpToUnity;
        yield return StartCoroutine(c_loadPOSandPROV(myPat));

        yield return Ninja.JumpToUnity;
        loadingCircle.Close();
        gameObject.SetActive(false);
        loaded = true;
        Text PatientNameHeader = GameObject.Find("HeaderDisplay").transform.GetChild(0).GetComponent<Text>();
        PatientNameHeader.text = myPat.patientName;
        ApplicationState.init();
        yield return new WaitForSeconds(0.1f);
    }

    IEnumerator c_loadEEGFile(Patient myPat)
    {
        loadingCircle = (Instantiate(loadingCirclePrefab, Vector3.zero, Quaternion.identity, GameObject.Find("CircleWindow").transform) as GameObject).GetComponent<LoadingCircle>();
        loadingCircle.transform.localPosition = new Vector3(0, 0, 0);

        yield return Ninja.JumpToUnity;
        loadingCircle.Set(0, "Finding files");
        loadingCircle.Set(0.1f, "Loading File 1");
        yield return Ninja.JumpBack;
        yield return Process(myPat.smFiles[0], 0);
        yield return Ninja.JumpToUnity;

        loadingCircle.Set(0.2f, "Loading File 2");
        yield return Ninja.JumpBack;
        yield return Process(myPat.smFiles[1], 1);
        yield return Ninja.JumpToUnity;

        loadingCircle.Set(0.4f, "Loading File 3");
        yield return Ninja.JumpBack;
        yield return Process(myPat.smFiles[2], 2);
        yield return Ninja.JumpToUnity;

        loadingCircle.Set(0.6f, "Loading File 4");
        yield return Ninja.JumpBack;
        yield return Process(myPat.smFiles[3], 3);
        yield return Ninja.JumpToUnity;

        loadingCircle.Set(0.8f, "Loading File 5");
        yield return Ninja.JumpBack;
        yield return Process(myPat.smFiles[4], 4);
        yield return Ninja.JumpToUnity;

        loadingCircle.Set(1.0f, "Loading File 6");
        yield return Ninja.JumpBack;
        yield return Process(myPat.smFiles[5], 5);
        yield return Ninja.JumpToUnity;
    }

    YieldInstruction Process(string filePath, int FileID)
    {
        // I give my callback to the process
        // Async needed for another thread and not freezing/laging UI
        return this.StartCoroutineAsync(EegFileService.c_Load(filePath, Tools.CSharp.EEG.File.FileType.ELAN, FileID));
    }

    IEnumerator c_loadVideo(string videoPath)
    {
        //load video
        DataContainer container = EegFileService.ReturnFirstValidContainer();
        int totalDuration = container.NumberOfSample / container.Frequency.Value;
        loadVideo(videoPath, totalDuration);

        yield return null;
    }

    IEnumerator c_loadPOSandPROV(Patient myPat)
    {
        if (myPat.prov != "")
        {
            if (myPat.pos != "")
            {
                DataContainer container = EegFileService.ReturnFirstValidContainer();
                posFile = new POS(myPat.pos, container.Frequency.Value);
                posFile.readPosData();
            }

            if (posFile.FileTriggers.Count > 0)
            {
                provFile = new PROV(myPat.prov);
                if (provFile.changeCodeFilePath != "")
                    posFile.renameTrigger(provFile);

                posFile.calculateReactionTime(provFile);
            }
        }

        if (posFile != null && posFile.FileTriggers.Count > 0)
            loadPerf(true);
        else
            loadPerf(false);

        yield return new WaitForSeconds(1.0f);
        yield return null;
    }

    void resetValue(Patient myPat)
    {
        GameObject reloadGameObject = Instantiate(Resources.Load("Prefabs/Media-Reload", typeof(GameObject))) as GameObject;
        reloadGameObject.name = "ReloadMedia";

        ReloadMedia r = reloadGameObject.GetComponent<ReloadMedia>();

        r.lhemi_MNI = myPat.mni.lhemi;
        r.rhemi_MNI = myPat.mni.rhemi;
        r.pts_MNI = myPat.mni.pts;

        r.lhemi_PAT = myPat.pat.lhemi;
        r.rhemi_PAT = myPat.pat.rhemi;
        r.pts_PAT = myPat.pat.pts;
        r.atlas_PAT = myPat.pat.atlasCSV;

        r.sm0 = myPat.smFiles[0];
        r.sm250 = myPat.smFiles[1];
        r.sm500 = myPat.smFiles[2];
        r.sm1000 = myPat.smFiles[3];
        r.sm2500 = myPat.smFiles[4];
        r.sm5000 = myPat.smFiles[5];

        r.pos = myPat.pos;
        r.prov = myPat.prov;
        r.video = myPat.video;
        r.id = pm.idCurrentPatientLoaded;
        r.path = pm.pathFile;

        SceneManager.LoadScene("_main");
    }
}
