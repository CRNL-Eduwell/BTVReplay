using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Assets.Scripts.Data.Factory;
using UnityEngine;

public class DBFile : IPatientsContext
{
    public List<Patient> Patients { get; set; } = new List<Patient>();
    public string FilePath { get; private set; }

    public DBFile(string filePath)
    {
        FilePath = filePath;

        if (File.Exists(FilePath))
            Load(FilePath);
        else
            Debug.LogError("PosFile => Filepath : " + FilePath + " does not exist ");
    }

    private int Load(string FilePath)
    {
        try
        {
            if (Patients.Count > 0)
                Patients = new List<Patient>();

            using (StreamReader sr = new StreamReader(FilePath))
            {
                string[] fileSplited = sr.ReadToEnd().Split(new string[] { "[----------]" }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < fileSplited.Length - 1; i++)   // -1 because of last line jump
                {
                    Patient currentPat = new Patient();
                    string[] currentPatSplit = fileSplited[i].Split(new string[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                    for (int j = 0; j < currentPatSplit.Length; j++)
                    {
                        string[] splitPath = currentPatSplit[j].Split(new string[] { " : " }, StringSplitOptions.RemoveEmptyEntries);
                        loadValue(currentPat, splitPath);
                    }

                    Patients.Add(currentPat);
                }

                sr.Close();
                return 0;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("The patient database file could not be read:");
            Console.WriteLine(e.Message);
            Patients = new List<Patient>();
            return -1;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="patient"></param>
    /// <param name="data">Raw data : Id 0 is key Id 1 is value </param>
    private void loadValue(Patient patient, string[] data)
    {
        switch (data[0])
        {
            case "LH_MNI":
                if (data.Length == 1) patient.mni.lhemi = Application.dataPath + "/Config/Data/MNI/MNI_single_hight_Lhemi.tri";
                else if (data.Length > 1) patient.mni.lhemi = data[1];
                break;
            case "RH_MNI":
                if (data.Length == 1) patient.mni.rhemi = Application.dataPath + "/Config/Data/MNI/MNI_single_hight_Rhemi.tri";
                else if (data.Length > 1) patient.mni.rhemi = data[1];
                break;
            case "PTS_MNI": if (data.Length > 1) patient.mni.pts = data[1]; break;
            case "MESH_MNI": if (data.Length > 1) patient.mni.setNbMesh(data[1]); break;
            case "EEG_MNI": if (data.Length > 1) patient.mni.setEegTech(data[1]); break;
            case "LH_PAT": if (data.Length > 1) patient.pat.lhemi = data[1]; break;
            case "RH_PAT": if (data.Length > 1) patient.pat.rhemi = data[1]; break;
            case "PTS_PAT": if (data.Length > 1) patient.pat.pts = data[1]; break;
            case "ATLAS_PAT": if (data.Length > 1) patient.pat.atlasCSV = data[1]; break;
            case "MESH_PAT": if (data.Length > 1) patient.pat.setNbMesh(data[1]); break;
            case "EEG_PAT": if (data.Length > 1) patient.pat.setEegTech(data[1]); break;
            case "SM0": if (data.Length > 1) patient.smFiles[0] = data[1]; break;
            case "SM250": if (data.Length > 1) patient.smFiles[1] = data[1]; break;
            case "SM500": if (data.Length > 1) patient.smFiles[2] = data[1]; break;
            case "SM1000": if (data.Length > 1) patient.smFiles[3] = data[1]; break;
            case "SM2500": if (data.Length > 1) patient.smFiles[4] = data[1]; break;
            case "SM5000": if (data.Length > 1) patient.smFiles[5] = data[1]; break;
            case "POS": if (data.Length > 1) patient.pos = data[1]; break;
            case "PROV": if (data.Length > 1) patient.prov = data[1]; break;
            case "VID": if (data.Length > 1) patient.video = data[1]; break;
            default: UnityEngine.Debug.LogError("Problem with patients file"); break;
        }
    }

    public static void Save(string FilePath, List<Patient> patients)
    {
        using (StreamWriter sw = new StreamWriter(FilePath))
        {
            for (int i = 0; i < patients.Count; i++)
            {
                sw.WriteLine("LH_MNI : " + patients[i].mni.lhemi);
                sw.WriteLine("RH_MNI : " + patients[i].mni.rhemi);
                sw.WriteLine("PTS_MNI : " + patients[i].mni.pts);
                sw.WriteLine("MESH_MNI : " + patients[i].mni.getMeshNb());
                sw.WriteLine("EEG_MNI : " + patients[i].mni.getEegTech());
                sw.WriteLine("LH_PAT : " + patients[i].pat.lhemi);
                sw.WriteLine("RH_PAT : " + patients[i].pat.rhemi);
                sw.WriteLine("PTS_PAT : " + patients[i].pat.pts);
                sw.WriteLine("ATLAS_PAT : " + patients[i].pat.atlasCSV);
                sw.WriteLine("MESH_PAT : " + patients[i].pat.getMeshNb());
                sw.WriteLine("EEG_PAT : " + patients[i].pat.getEegTech());
                sw.WriteLine("SM0 : " + patients[i].smFiles[0]);
                sw.WriteLine("SM250 : " + patients[i].smFiles[1]);
                sw.WriteLine("SM500 : " + patients[i].smFiles[2]);
                sw.WriteLine("SM1000 : " + patients[i].smFiles[3]);
                sw.WriteLine("SM2500 : " + patients[i].smFiles[4]);
                sw.WriteLine("SM5000 : " + patients[i].smFiles[5]);
                sw.WriteLine("POS : " + patients[i].pos);
                sw.WriteLine("PROV : " + patients[i].prov);
                sw.WriteLine("VID : " + patients[i].video);
                sw.WriteLine("[----------]");
            }
            sw.Close();
        }
    }
}
