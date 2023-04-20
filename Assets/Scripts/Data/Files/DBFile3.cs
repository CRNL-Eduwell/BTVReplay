using Assets.Scripts.Data.Factory;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using System.Linq;

public class DBFile3 : ISubjectsContext
{
    public List<Subject> Subjects { get; set; } = new List<Subject>();
    public string FilePath { get; set; } = "";

    public DBFile3()
    {

    }

    public DBFile3(string filePath)
    {
        FilePath = filePath;

        if (File.Exists(FilePath))
            Load(FilePath);
        else
            Debug.LogError("DBFile3 => Filepath : " + FilePath + " does not exist ");
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
            Debug.LogError("Error saving .dbtv2 file at " + FilePath);
            Debug.LogException(e);
        }
    }

    public static void ConvertOldDbFiles(string FilePath, List<OldSubject> subjects)
    {
        List<Subject> newSubjects = new List<Subject>();
        foreach (OldSubject patient in subjects)
        {
            Subject newSubject = new Subject
            {
                PatientName = patient.PatientName,
                AnatomicalSpaces = patient.AnatomicalSpaces.ToDictionary(entry => entry.Key, entry => new BrainDataContainer(entry.Value))
            };
            newSubject.Experiments.Add(new Experiment("Name", patient.Files, patient.Video));
            newSubjects.Add(newSubject);
        }

        Save(FilePath.Replace(".dbtv", ".dbtv2"), newSubjects);
    }
}
