using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Patient
{
    #region members
    public string lhemi_MNI = "";
    public string rhemi_MNI = "";
    public string pts_MNI = "";

    public string lhemi_PAT = "";
    public string rhemi_PAT = "";
    public string pts_PAT = "";

    public string sm0 = "";
    public string sm250 = "";
    public string sm500 = "";
    public string sm1000 = "";
    public string sm2500 = "";
    public string sm5000 = "";

    public string pos = "";
    public string prov = "";
    public string video = "";

    public string patientName = "";
    #endregion

    public Patient()
    {

    }

    public Patient(Patient thisPat)
    {
        lhemi_MNI = thisPat.lhemi_MNI;
        rhemi_MNI = thisPat.rhemi_MNI;
        pts_MNI = thisPat.pts_MNI;

        lhemi_PAT = thisPat.lhemi_PAT;
        rhemi_PAT = thisPat.rhemi_PAT;
        pts_PAT = thisPat.pts_PAT;

        sm0 = thisPat.sm0;
        sm250 = thisPat.sm250;
        sm500 = thisPat.sm500;
        sm1000 = thisPat.sm1000;
        sm2500 = thisPat.sm2500;
        sm5000 = thisPat.sm5000;

        pos = thisPat.pos;
        prov = thisPat.prov;
        video = thisPat.video;
    }

    public void loadValue(int val, string[] data)
    {
        switch (val)
        {
            case 0: if (data.Length > 1) lhemi_MNI = data[1]; break;
            case 1: if (data.Length > 1) rhemi_MNI = data[1]; break;
            case 2: if (data.Length > 1) pts_MNI = data[1]; break;
            case 3: if (data.Length > 1) lhemi_PAT = data[1]; break;
            case 4: if (data.Length > 1) rhemi_PAT = data[1]; break;
            case 5: if (data.Length > 1) pts_PAT = data[1]; break;
            case 6: if (data.Length > 1) sm0 = data[1]; break;
            case 7: if (data.Length > 1) sm250 = data[1]; break;
            case 8: if (data.Length > 1) sm500 = data[1]; break;
            case 9: if (data.Length > 1) sm1000 = data[1]; break;
            case 10: if (data.Length > 1) sm2500 = data[1]; break;
            case 11: if (data.Length > 1) sm5000 = data[1]; break;
            case 12: if (data.Length > 1) pos = data[1]; break;
            case 13: if (data.Length > 1) prov = data[1]; break;
            case 14: if (data.Length > 1) video = data[1]; break;
            default: Debug.LogError("Problem with patients file"); break;
        }
    }

    public void CopyValues(Patient thisPat)
    {
        lhemi_MNI = thisPat.lhemi_MNI;
        rhemi_MNI = thisPat.rhemi_MNI;
        pts_MNI = thisPat.pts_MNI;

        lhemi_PAT = thisPat.lhemi_PAT;
        rhemi_PAT = thisPat.rhemi_PAT;
        pts_PAT = thisPat.pts_PAT;

        sm0 = thisPat.sm0;
        sm250 = thisPat.sm250;
        sm500 = thisPat.sm500;
        sm1000 = thisPat.sm1000;
        sm2500 = thisPat.sm2500;
        sm5000 = thisPat.sm5000;

        pos = thisPat.pos;
        prov = thisPat.prov;
        video = thisPat.video;
    }
}

public class PatientManager
{
    public List<Patient> currentPatients = new List<Patient>();
    public int idCurrentPatientLoaded = 0;
    string pathFile { get { return Application.dataPath + @"/Config/PatientBase/PatientReplay.txt"; } } 
    string pathBUFile { get { return Application.dataPath + @"/Config/PatientBase/PatientReplayBU.txt"; } } 

    public void SaveList()
    {
        File.Copy(pathFile, pathBUFile, true);

        try
        {
            using (StreamWriter sw = new StreamWriter(pathFile))
            {
                for (int i = 0; i < currentPatients.Count; i++)
                {
                    sw.WriteLine("LH_MNI : " + currentPatients[i].lhemi_MNI);
                    sw.WriteLine("RH_MNI : " + currentPatients[i].rhemi_MNI);
                    sw.WriteLine("PTS_MNI : " + currentPatients[i].pts_MNI);
                    sw.WriteLine("LH_PAT : " + currentPatients[i].lhemi_MNI);
                    sw.WriteLine("RH_PAT : " + currentPatients[i].rhemi_MNI);
                    sw.WriteLine("PTS_PAT : " + currentPatients[i].pts_MNI);
                    sw.WriteLine("SM0 : " + currentPatients[i].sm0);
                    sw.WriteLine("SM250 : " + currentPatients[i].sm250);
                    sw.WriteLine("SM500 : " + currentPatients[i].sm500);
                    sw.WriteLine("SM1000 : " + currentPatients[i].sm1000);
                    sw.WriteLine("SM2500 : " + currentPatients[i].sm2500);
                    sw.WriteLine("SM5000 : " + currentPatients[i].sm5000);
                    sw.WriteLine("POS : " + currentPatients[i].pos);
                    sw.WriteLine("PROV : " + currentPatients[i].prov);
                    sw.WriteLine("VID : " + currentPatients[i].video);
                    sw.WriteLine("[----------]");
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Error writing file", e.ToString());
        }
    }

    public void LoadList(bool backUp)
    {
        List<int> indexToLook = new List<int>{ 6,7,8,9,10,11 };
        bool nameFound = false;

        string fileToLoad = pathFile;

        try
        {
            if (currentPatients.Count > 0)
                currentPatients = new List<Patient>();

            if (backUp)
                fileToLoad = pathBUFile;

            using (StreamReader sr = new StreamReader(fileToLoad))
            {
                string[] fileSplited = sr.ReadToEnd().Split(new string[] { "[----------]" }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < fileSplited.Length - 1; i++)   // -1 because of last line jump
                {
                    Patient currentPat = new Patient();
                    string[] currentPatSplit = fileSplited[i].Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                    for (int j = 0; j < currentPatSplit.Length; j++)
                    {
                        string[] splitPath = currentPatSplit[j].Split(new string[] { " : " }, StringSplitOptions.RemoveEmptyEntries);
                        if (indexToLook.IndexOf(j) != -1 && splitPath.Length > 1 && splitPath[1] != "" && nameFound == false)
                        {
                            string[] namesplit = splitPath[1].Split(new string[] { @"\", "/" }, StringSplitOptions.RemoveEmptyEntries);
                            currentPat.patientName = namesplit[namesplit.Length - 2];
                            nameFound = true;
                        }
                        currentPat.loadValue(j, splitPath);
                    }

                    currentPatients.Add(currentPat);
                    nameFound = false;
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Error Reading file", e.ToString());
        }
    }

    public void addPat(Patient thisPatient)
    {
        currentPatients.Add(new Patient(thisPatient));
    }

    public void removePatientAt(int index)
    {
        if (currentPatients.Count > 0)
        {
            currentPatients.Remove(currentPatients[index]);
        }
    }
}

public class BTVMedia_New : MonoBehaviour
{
    #region UIMembers
    private Button hidePanel = null;
    private Button addPatient = null;           /*||*/     private Button loadThisPat = null;
    //===
    private Transform patientContent = null;    /*||*/
    private GameObject patientTemplate = null;  /*||*/
    private GameObject patDetailTemplate = null;/*||*/
    private Button saveBase = null;             /*||*/     private Button loadBase = null;  /*||*/  private Button loadBUBase = null;
    //===
    #endregion

    #region members
    public PatientManager pm = new PatientManager();
    public POSFile1 posF1 = null;
    public ELAN e0 = null;
    public ELAN e250 = null;
    public ELAN e500 = null;
    public ELAN e1000 = null;
    public ELAN e2500 = null;
    public ELAN e5000 = null;
    public provFile provF = null;

    public bool loaded = false;
    public bool perfOk = false;
    #endregion

    void Awake()
    {
        patientTemplate = Resources.Load("Prefabs/LoadMedia/PatientRPZ/PatientX", typeof(GameObject)) as GameObject;
        patDetailTemplate = Resources.Load("Prefabs/LoadMedia/PatientRPZ/InfoPatient", typeof(GameObject)) as GameObject;

        #region getObjectFromScene
        hidePanel = gameObject.transform.GetChild(0).GetChild(1).GetComponent<Button>();

        addPatient = gameObject.transform.GetChild(1).GetChild(3).GetChild(0).GetComponent<Button>();
        loadThisPat = gameObject.transform.GetChild(1).GetChild(3).GetChild(1).GetComponent<Button>();

        patientContent = gameObject.transform.GetChild(1).GetChild(4).GetChild(1).GetChild(0).GetChild(0).GetChild(0);
        saveBase = gameObject.transform.GetChild(1).GetChild(4).GetChild(2).GetComponent<Button>();
        loadBase = gameObject.transform.GetChild(1).GetChild(4).GetChild(3).GetComponent<Button>();
        loadBUBase = gameObject.transform.GetChild(1).GetChild(4).GetChild(4).GetComponent<Button>();
        #endregion

        #region addListener
        hidePanel.onClick.AddListener(() => { gameObject.SetActive(false); setPatientGUI(gameObject.transform.GetChild(1).gameObject, new Patient());});
        addPatient.onClick.AddListener(() => { addPatientToDB(); });
        loadThisPat.onClick.AddListener(() => { loadPatientGUI(); });
        saveBase.onClick.AddListener(() => { SaveDB(); });
        loadBase.onClick.AddListener(() => { pm.LoadList(false); InstantiateDB(); });
        loadBUBase.onClick.AddListener(() => { pm.LoadList(true); InstantiateDB(); });
        #endregion
    }

    void OnDestroy()
    {
        hidePanel.onClick.RemoveAllListeners();
        addPatient.onClick.RemoveAllListeners();
        loadThisPat.onClick.RemoveAllListeners();
        saveBase.onClick.RemoveAllListeners();
        loadBase.onClick.RemoveAllListeners();
        loadBUBase.onClick.RemoveAllListeners();
        if(e0 != null)
            e0.Dispose();
        if (e250 != null)
            e250.Dispose();
        if (e500 != null)
            e500.Dispose();
        if (e1000 != null)
            e1000.Dispose();
        if (e2500 != null)
            e2500.Dispose();
        if (e5000 != null)
            e5000.Dispose();

        for (int i = 0; i < patientContent.childCount; i+=2)
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

    void addPatientToDB()
    {
        pm.addPat(getPatientGUI(gameObject.transform.GetChild(1).gameObject));
        pm.SaveList();
        pm.LoadList(false);
        InstantiateDB();
    }

    public void loadPatientGUI()
    {
        loadMedia(getPatientGUI(gameObject.transform.GetChild(1).gameObject));
    }

    void SaveDB()
    {
        for (int i = 0; i < patientContent.childCount; i+=2)
        {
            Patient currentPat = getPatientGUI(patientContent.transform.GetChild(i + 1).gameObject);
            pm.currentPatients[i / 2] = currentPat;
        }

        pm.SaveList();
        pm.LoadList(false);
        InstantiateDB();
    }

    public void InstantiateDB()
    {
        if (patientContent.childCount > 0)
        {
            for (int i = 0; i < patientContent.childCount; i++)
                Destroy(patientContent.GetChild(i).gameObject);
        }

        for (int i = 0; i < pm.currentPatients.Count; i++)
        {
            GameObject currentPat = Instantiate(patientTemplate);
            GameObject currentDetails = Instantiate(patDetailTemplate);
            currentPat.name = "pat" + i;
            currentPat.transform.SetParent(patientContent);
            currentPat.transform.localScale = new Vector3(1, 1, 1);
            currentDetails.name = "patDetails" + i;
            currentDetails.transform.SetParent(patientContent);
            currentDetails.transform.localScale = new Vector3(1, 1, 1);
            currentDetails.SetActive(true);
            currentDetails.SetActive(false);

            loadDataOnePatient(currentPat, currentDetails, i);
        }
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
            pm.removePatientAt(idPat);
            pm.SaveList();
            InstantiateDB();
        });
        #endregion

        setPatientGUI(patientDetails, pm.currentPatients[idPat]);
    }

    void setPatientGUI(GameObject patientDetails , Patient myPat)
    {
        patientDetails.transform.GetChild(0).GetChild(0).GetChild(1).GetComponent<browseButton>().inputfield.text = myPat.lhemi_MNI;
        patientDetails.transform.GetChild(0).GetChild(0).GetChild(2).GetComponent<browseButton>().inputfield.text = myPat.rhemi_MNI;
        patientDetails.transform.GetChild(0).GetChild(0).GetChild(3).GetComponent<browseButton>().inputfield.text = myPat.pts_MNI;

        patientDetails.transform.GetChild(0).GetChild(1).GetChild(1).GetComponent<browseButton>().inputfield.text = myPat.lhemi_PAT;
        patientDetails.transform.GetChild(0).GetChild(1).GetChild(2).GetComponent<browseButton>().inputfield.text = myPat.rhemi_PAT;
        patientDetails.transform.GetChild(0).GetChild(1).GetChild(3).GetComponent<browseButton>().inputfield.text = myPat.pts_PAT;

        patientDetails.transform.GetChild(1).GetChild(1).GetComponent<browseButton>().inputfield.text = myPat.sm0;
        patientDetails.transform.GetChild(1).GetChild(2).GetComponent<browseButton>().inputfield.text = myPat.sm250;
        patientDetails.transform.GetChild(1).GetChild(3).GetComponent<browseButton>().inputfield.text = myPat.sm500;
        patientDetails.transform.GetChild(1).GetChild(4).GetComponent<browseButton>().inputfield.text = myPat.sm1000;
        patientDetails.transform.GetChild(1).GetChild(5).GetComponent<browseButton>().inputfield.text = myPat.sm2500;
        patientDetails.transform.GetChild(1).GetChild(6).GetComponent<browseButton>().inputfield.text = myPat.sm5000;

        patientDetails.transform.GetChild(2).GetChild(1).GetComponent<browseButton>().inputfield.text = myPat.pos;
        patientDetails.transform.GetChild(2).GetChild(2).GetComponent<browseButton>().inputfield.text = myPat.prov;
        patientDetails.transform.GetChild(2).GetChild(4).GetComponent<browseButton>().inputfield.text = myPat.video;
    }

    public Patient getPatientGUI(GameObject rootUI)
    {
        Patient myPat = new Patient();

        myPat.lhemi_MNI = rootUI.transform.GetChild(0).GetChild(0).GetChild(1).GetComponent<browseButton>().inputfield.text;
        myPat.rhemi_MNI = rootUI.transform.GetChild(0).GetChild(0).GetChild(2).GetComponent<browseButton>().inputfield.text;
        myPat.pts_MNI = rootUI.transform.GetChild(0).GetChild(0).GetChild(3).GetComponent<browseButton>().inputfield.text;

        myPat.lhemi_PAT = rootUI.transform.GetChild(0).GetChild(1).GetChild(1).GetComponent<browseButton>().inputfield.text;
        myPat.rhemi_PAT = rootUI.transform.GetChild(0).GetChild(1).GetChild(2).GetComponent<browseButton>().inputfield.text;
        myPat.pts_PAT = rootUI.transform.GetChild(0).GetChild(1).GetChild(3).GetComponent<browseButton>().inputfield.text;

        myPat.sm0 = rootUI.transform.GetChild(1).GetChild(1).GetComponent<browseButton>().inputfield.text;
        myPat.sm250 = rootUI.transform.GetChild(1).GetChild(2).GetComponent<browseButton>().inputfield.text;
        myPat.sm500 = rootUI.transform.GetChild(1).GetChild(3).GetComponent<browseButton>().inputfield.text;
        myPat.sm1000 = rootUI.transform.GetChild(1).GetChild(4).GetComponent<browseButton>().inputfield.text;
        myPat.sm2500 = rootUI.transform.GetChild(1).GetChild(5).GetComponent<browseButton>().inputfield.text;
        myPat.sm5000 = rootUI.transform.GetChild(1).GetChild(6).GetComponent<browseButton>().inputfield.text;

        myPat.pos = rootUI.transform.GetChild(2).GetChild(1).GetComponent<browseButton>().inputfield.text;
        myPat.prov = rootUI.transform.GetChild(2).GetChild(2).GetComponent<browseButton>().inputfield.text;
        myPat.video = rootUI.transform.GetChild(2).GetChild(4).GetComponent<browseButton>().inputfield.text;

        return myPat;
    }

    public void loadMedia(Patient myPat)
    {
        if (loaded == true)
        {
           resetValue(myPat);
        }
        else
        {
            loadStuff(myPat);
        }
    }

    void loadStuff(Patient myPat)
    {
        e0 = checkObject(myPat.sm0);
        e250 = checkObject(myPat.sm250);
        e500 = checkObject(myPat.sm500);
        e1000 = checkObject(myPat.sm1000);
        e2500 = checkObject(myPat.sm2500);
        e5000 = checkObject(myPat.sm5000);

        if (myPat.prov != "")
        {
            if (myPat.pos != "")
            {
                posF1 = new POSFile1(myPat.pos);
                posF1.readPosData();
            }

            provF = new provFile(myPat.prov);
            if (provF.changeCodeFilePath != "")
            {
                try
                {
                    posF1.renameTrigger(provF);
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                    perfOk = false;
                }
            }
            posF1.calculateReactionTime(provF, getSamplingFreqFromFiles());
        }

        showMe(false);
        loaded = true;

        if (posF1 != null)
        {
            perfOk = true;
        }
    }

    void resetValue(Patient myPat)
    {
        GameObject cheat = Instantiate(Resources.Load("Prefabs/ReloadCheat_New", typeof(GameObject))) as GameObject;
        cheat.name = "ReloadCheat_New";

        Reload_New r = cheat.GetComponent<Reload_New>();

        r.lhemi_MNI = myPat.lhemi_MNI;
        r.rhemi_MNI = myPat.rhemi_MNI;
        r.pts_MNI = myPat.pts_MNI;

        r.lhemi_PAT = myPat.lhemi_PAT;
        r.rhemi_PAT = myPat.rhemi_PAT;
        r.pts_PAT = myPat.pts_PAT;

        r.sm0 = myPat.sm0;
        r.sm250 = myPat.sm250;
        r.sm500 = myPat.sm500;
        r.sm1000 = myPat.sm1000;
        r.sm2500 = myPat.sm2500;
        r.sm5000 = myPat.sm5000;

        r.pos = myPat.pos;
        r.prov = myPat.prov;
        r.video = myPat.video;
        r.id = pm.idCurrentPatientLoaded;

        //if (e0 != null)
        //    e0.Dispose();
        //if (e250 != null)
        //    e250.Dispose();
        //if (e500 != null)
        //    e500.Dispose();
        //if (e1000 != null)
        //    e1000.Dispose();
        //if (e2500 != null)
        //    e2500.Dispose();
        //if (e5000 != null)
        //    e5000.Dispose();

        SceneManager.LoadScene("_main");
    }

    ELAN checkObject(string currentElement)
    {
        if (currentElement != "")
        {
            ELAN elan = new ELAN(currentElement);
            elan.readElanFile();
            return elan;
        }
        else
        {
            return null;
        }
    }

    public float getSamplingFreqFromFiles()
    {
        if (e0 != null)
        {
            return e0.sampFreq;
        }
        else if (e250 != null)
        {
            return e250.sampFreq;
        }
        else if (e500 != null)
        {
            return e500.sampFreq;
        }
        else if (e1000 != null)
        {
            return e1000.sampFreq;
        }
        else if (e2500 != null)
        {
            return e2500.sampFreq;
        }
        else if (e5000 != null)
        {
            return e5000.sampFreq;
        }
        else
        {
            return -1;
        }
    }

    public void showMe(bool yes)
    {
        gameObject.SetActive(yes);
    }
}
