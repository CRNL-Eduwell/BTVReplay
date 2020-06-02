using Assets.Scripts.Data.Factory;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

public class DBFile2 : ISubjectsContext
{
    public List<Subject> Subjects { get; set; } = new List<Subject>();
    public string FilePath { get; set; } = "";

    public DBFile2()
    {

    }

    public DBFile2(string filePath)
    {
        FilePath = filePath;

        if (File.Exists(FilePath))
            Load(FilePath);
        else
            Debug.LogError("DBFile2 => Filepath : " + FilePath + " does not exist ");
    }

    private int Load(string FilePath)
    {
        try
        {
            using (StreamReader streamReader = new StreamReader(FilePath))
            {
                Subjects = JsonConvert.DeserializeObject<List<Subject>>(streamReader.ReadToEnd(), new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto });
            }

            return 0;
        }
        catch (Exception e)
        {
            Console.WriteLine("The patient database file could not be read:");
            Console.WriteLine(e.Message);
            Subjects = new List<Subject>();
            return -1;
        }
    }

    public static void Save(string FilePath, List<Subject> subjects)
    {
        try
        {
            using (StreamWriter streamWriter = new StreamWriter(FilePath))
            {
                string json = JsonConvert.SerializeObject(subjects, Formatting.Indented, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto, TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple });
                streamWriter.Write(json);
                streamWriter.Close();
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error saving .dbtv file at " + FilePath);
            Debug.LogException(e);
        }
    }

    public static void ConvertOldDbFiles(string FilePath, List<Patient> patients)
    {
        string[] oldKeys = new string[6] {"sm0", "sm250", "sm500", "sm1000", "sm2500", "sm5000" };
        List<Subject> subjects = new List<Subject>();
        foreach (Patient patient in patients)
        {
            Subject subject = new Subject();
            subject.PatientName = patient.PatientName;

            BrainDataContainer mni = new BrainDataContainer
            {
                LeftHemisphere = patient.mni.LeftHemisphere,
                RightHemisphere = patient.mni.RightHemisphere,
                Transformation = Application.dataPath + "/Config/Data/MNI/transfo_mni.trm",
                Pts = patient.mni.Pts,
                Atlas = patient.mni.Atlas,
                MeshConfiguration = patient.mni.MeshConfiguration
            };
            subject.AnatomicalSpaces.Add("MNI", mni);

            BrainDataContainer pat = new BrainDataContainer
            {
                LeftHemisphere = patient.pat.LeftHemisphere,
                RightHemisphere = patient.pat.RightHemisphere,
                Transformation = "",
                Pts = patient.pat.Pts,
                Atlas = patient.pat.Atlas,
                MeshConfiguration = patient.pat.MeshConfiguration
            };
            subject.AnatomicalSpaces.Add("PAT", pat);

            for (int i = 0; i < patient.smFiles.Length; i++)
            {
                string path = patient.smFiles[i];
                if (!string.IsNullOrEmpty(path))
                {
                    FileInfo fileInfo = new FileInfo(path);
                    if (fileInfo.Extension == ".TRC")
                    {
                        subject.Files.Add(oldKeys[i], new MicromedFileInfo(fileInfo.FullName));
                    }
                    else if (fileInfo.Extension == ".eeg")
                    {
                        subject.Files.Add(oldKeys[i], new ElanFileInfo(fileInfo.FullName));
                    }
                    else if (fileInfo.Extension == ".vhdr")
                    {
                        subject.Files.Add(oldKeys[i], new BrainvisionFileInfo(fileInfo.FullName));
                    }
                    else if (fileInfo.Extension == ".edf")
                    {
                        subject.Files.Add(oldKeys[i], new EdfFileInfo(fileInfo.FullName));
                    }
                }
            }

            subject.Video = patient.video;

            subjects.Add(subject);
        }

        Save(FilePath.Replace(".txt", ".dbtv"), subjects);
    }
}
