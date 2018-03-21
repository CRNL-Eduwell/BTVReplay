using System;
using System.IO;
using System.Collections.Generic;

public enum mesh_Configuration { leftright, single };
public enum eeg_Technology { intra, scalp };

public class brain_anat
{
    public bool hasAnat
    {
        get
        {
            if (nbMesh == mesh_Configuration.leftright)
                return (lhemi != "" && rhemi != "" && pts != "");
            else if (nbMesh == mesh_Configuration.single)
                return (lhemi != "" && pts != "");
            else
                return false;
        }
    }

    public mesh_Configuration GetMeshNb
    {
        get
        {
            return nbMesh;
        }
    }

    public eeg_Technology GetEegTech
    {
        get
        {
            return eeg;
        }
    }

    public void setNbMesh(string textOpt)
    {
        if (textOpt == "Left/Right Mesh")
            nbMesh = mesh_Configuration.leftright;
        else if (textOpt == "Single Mesh")
            nbMesh = mesh_Configuration.single;
    }

    public string getMeshNb()
    {
        if (nbMesh == mesh_Configuration.leftright)
            return "Left/Right Mesh";
        else if (nbMesh == mesh_Configuration.single)
            return "Single Mesh";
        else
            return "ERROR";
    }

    public void setEegTech(string textOpt)
    {
        if (textOpt == "Intracranial EEG")
            eeg = eeg_Technology.intra;
        else if (textOpt == "Scalp EEG")
            eeg = eeg_Technology.scalp;
    }

    public string getEegTech()
    {
        if (eeg == eeg_Technology.intra)
            return "Intracranial EEG";
        else if (eeg == eeg_Technology.scalp)
            return "Scalp EEG";
        else
            return "ERROR";
    }

    public string lhemi = "";
    public string rhemi = "";
    public string pts = "";
    public string atlasCSV = "";
    mesh_Configuration nbMesh = mesh_Configuration.leftright;
    eeg_Technology eeg = eeg_Technology.intra;
};

public class Patient
{
    #region Public Properties & members
    public bool hasMNI
    {
        get { return mni.hasAnat; }
    }
    public bool hasPAT
    {
        get { return pat.hasAnat; }
    }
    public brain_anat mni;
    public brain_anat pat;
    public string[] smFiles = new string[6] { "", "", "", "", "", ""};
    public string pos = "";
    public string prov = "";
    public string video = "";
    public string patientName = "";
    #endregion

    #region Constructors
    public Patient()
    {
        mni = new brain_anat();
        pat = new brain_anat();
    }
    public Patient(Patient thisPat)
    {
        mni = thisPat.mni;
        pat = thisPat.pat;

        for (int i = 0; i < 6; i++)
        {
            smFiles[i] = thisPat.smFiles[i];
            if (smFiles[i] != "" && patientName == "")
                patientName = getPatientNameFromPath(smFiles[i]);
        }

        pos = thisPat.pos;
        prov = thisPat.prov;
        video = thisPat.video;
    }
    #endregion

    public void loadValue(string key, string[] data)
    {
        switch (key)
        {
            case "LH_MNI": if (data.Length > 1) mni.lhemi = data[1]; break;
            case "RH_MNI": if (data.Length > 1) mni.rhemi = data[1]; break;
            case "PTS_MNI": if (data.Length > 1) mni.pts = data[1]; break;
            case "MESH_MNI": if (data.Length > 1) mni.setNbMesh(data[1]); break;
            case "EEG_MNI": if (data.Length > 1) mni.setEegTech(data[1]); break;
            case "LH_PAT": if (data.Length > 1) pat.lhemi = data[1]; break;
            case "RH_PAT": if (data.Length > 1) pat.rhemi = data[1]; break;
            case "PTS_PAT": if (data.Length > 1) pat.pts = data[1]; break;
            case "ATLAS_PAT": if (data.Length > 1) pat.atlasCSV = data[1]; break;
            case "MESH_PAT": if (data.Length > 1) pat.setNbMesh(data[1]); break;
            case "EEG_PAT": if (data.Length > 1) pat.setEegTech(data[1]); break;
            case "SM0": if (data.Length > 1) smFiles[0] = data[1]; break;
            case "SM250": if (data.Length > 1) smFiles[1] = data[1]; break;
            case "SM500": if (data.Length > 1) smFiles[2] = data[1]; break;
            case "SM1000": if (data.Length > 1) smFiles[3] = data[1]; break;
            case "SM2500": if (data.Length > 1) smFiles[4] = data[1]; break;
            case "SM5000": if (data.Length > 1) smFiles[5] = data[1]; break;
            case "POS": if (data.Length > 1) pos = data[1]; break;
            case "PROV": if (data.Length > 1) prov = data[1]; break;
            case "VID": if (data.Length > 1) video = data[1]; break;
            default: UnityEngine.Debug.LogError("Problem with patients file"); break;
        }
    }

    public static string getPatientNameFromPath(string path)
    {
        string[] namesplit = path.Split(new string[] { @"\", "/" }, StringSplitOptions.RemoveEmptyEntries);
        return namesplit[namesplit.Length - 2];
    }
}

public class DBManager
{
    public List<Patient> currentPatients = new List<Patient>();
    public int idCurrentPatientLoaded = 0;
    public string pathFile { get; private set; }
    string pathBUFile { get { return pathFile.Replace(".txt", "BU.txt"); } }

    public void SaveList(string dbPath = "")
    {
        if(dbPath != "")
            pathFile = dbPath;

        bool dbFileExist = File.Exists(pathFile) == true;
        bool dbBUExist = File.Exists(pathBUFile) == true;

        if (dbFileExist && dbBUExist)
        {
            File.Copy(pathFile, pathBUFile, true);
            saveDBTxtFile(pathFile);
        }
        else
        {
            File.Create(pathFile).Dispose();
            File.Create(pathBUFile).Dispose();
            saveDBTxtFile(pathFile);
            saveDBTxtFile(pathBUFile);
        }
    }

    void saveDBTxtFile(string pathFile)
    {
        using (StreamWriter sw = new StreamWriter(pathFile))
        {
            for (int i = 0; i < currentPatients.Count; i++)
            {
                sw.WriteLine("LH_MNI : " + currentPatients[i].mni.lhemi);
                sw.WriteLine("RH_MNI : " + currentPatients[i].mni.rhemi);
                sw.WriteLine("PTS_MNI : " + currentPatients[i].mni.pts);
                sw.WriteLine("MESH_MNI : " + currentPatients[i].mni.getMeshNb());
                sw.WriteLine("EEG_MNI : " + currentPatients[i].mni.getEegTech());
                sw.WriteLine("LH_PAT : " + currentPatients[i].pat.lhemi);
                sw.WriteLine("RH_PAT : " + currentPatients[i].pat.rhemi);
                sw.WriteLine("PTS_PAT : " + currentPatients[i].pat.pts);
                sw.WriteLine("ATLAS_PAT : " + currentPatients[i].pat.atlasCSV);
                sw.WriteLine("MESH_PAT : " + currentPatients[i].pat.getMeshNb());
                sw.WriteLine("EEG_PAT : " + currentPatients[i].pat.getEegTech());
                sw.WriteLine("SM0 : " + currentPatients[i].smFiles[0]);
                sw.WriteLine("SM250 : " + currentPatients[i].smFiles[1]);
                sw.WriteLine("SM500 : " + currentPatients[i].smFiles[2]);
                sw.WriteLine("SM1000 : " + currentPatients[i].smFiles[3]);
                sw.WriteLine("SM2500 : " + currentPatients[i].smFiles[4]);
                sw.WriteLine("SM5000 : " + currentPatients[i].smFiles[5]);
                sw.WriteLine("POS : " + currentPatients[i].pos);
                sw.WriteLine("PROV : " + currentPatients[i].prov);
                sw.WriteLine("VID : " + currentPatients[i].video);
                sw.WriteLine("[----------]");
            }
            sw.Close();
        }
    }

    public void LoadList(bool backUp, string dbPath = "")
    {
        List<int> indexToLook = new List<int> { 11, 12, 13, 14, 15, 16 };
        bool nameFound = false;
        string fileToLoad = "";

        if (dbPath == "")
            fileToLoad = pathFile;
        else
            fileToLoad = pathFile = dbPath;

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
                            currentPat.patientName = Patient.getPatientNameFromPath(splitPath[1]);
                            nameFound = true;
                        }
                        //currentPat.loadValue(j, splitPath);
                        currentPat.loadValue(splitPath[0], splitPath);
                    }

                    currentPatients.Add(currentPat);
                    nameFound = false;
                }
                sr.Close();
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