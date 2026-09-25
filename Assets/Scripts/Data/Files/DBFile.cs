using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
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
            throw new FileNotFoundException("DBFile => Filepath : " + FilePath + " does not exist ");
    }

    private void Load(string FilePath)
    {
        if (Patients.Count > 0)
            Patients = new List<Patient>();

        string rawContent;
        using (StreamReader sr = new StreamReader(FilePath))
        {
            rawContent = sr.ReadToEnd();
        }

        string[] fileSplited = rawContent.Split(new string[] { "[----------]" }, StringSplitOptions.RemoveEmptyEntries);
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

        // A non-empty file that yields no patient is not a v1 database (e.g. a JSON file that
        // ended up with a .txt extension). Failing here prevents the conversion chain from
        // propagating an empty patient list over valid files.
        if (Patients.Count == 0 && rawContent.Trim().Length > 0)
            throw new FormatException("DBFile => " + FilePath + " is not a valid BrainTV v1 (.txt) database: no patient block found");
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
                if (data.Length == 1) patient.mni.LeftHemisphere = Application.dataPath + "/Config/Data/MNI/MNI_single_hight_Lhemi.tri";
                else if (data.Length > 1) patient.mni.LeftHemisphere = data[1];
                break;
            case "RH_MNI":
                if (data.Length == 1) patient.mni.RightHemisphere = Application.dataPath + "/Config/Data/MNI/MNI_single_hight_Rhemi.tri";
                else if (data.Length > 1) patient.mni.RightHemisphere = data[1];
                break;
            case "PTS_MNI": if (data.Length > 1) patient.mni.Pts = data[1]; break;
            case "MESH_MNI": if (data.Length > 1) patient.mni.SetMeshConfigurationFromString(data[1]); break;
            case "EEG_MNI": if (data.Length > 1) patient.mni.SetEegTechnologyFromString(data[1]); break;
            case "LH_PAT": if (data.Length > 1) patient.pat.LeftHemisphere = data[1]; break;
            case "RH_PAT": if (data.Length > 1) patient.pat.RightHemisphere = data[1]; break;
            case "PTS_PAT": if (data.Length > 1) patient.pat.Pts = data[1]; break;
            case "ATLAS_PAT": if (data.Length > 1) patient.pat.Atlas = data[1]; break;
            case "MESH_PAT": if (data.Length > 1) patient.pat.SetMeshConfigurationFromString(data[1]); break;
            case "EEG_PAT": if (data.Length > 1) patient.pat.SetEegTechnologyFromString(data[1]); break;
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
                sw.WriteLine("LH_MNI : " + patients[i].mni.LeftHemisphere);
                sw.WriteLine("RH_MNI : " + patients[i].mni.RightHemisphere);
                sw.WriteLine("PTS_MNI : " + patients[i].mni.Pts);
                sw.WriteLine("MESH_MNI : " + patients[i].mni.MeshConfiguration.GetDescription());
                sw.WriteLine("EEG_MNI : " + patients[i].mni.EegTechnology.GetDescription());
                sw.WriteLine("LH_PAT : " + patients[i].pat.LeftHemisphere);
                sw.WriteLine("RH_PAT : " + patients[i].pat.RightHemisphere);
                sw.WriteLine("PTS_PAT : " + patients[i].pat.Pts);
                sw.WriteLine("ATLAS_PAT : " + patients[i].pat.Atlas);
                sw.WriteLine("MESH_PAT : " + patients[i].pat.MeshConfiguration.GetDescription());
                sw.WriteLine("EEG_PAT : " + patients[i].pat.EegTechnology.GetDescription());
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
