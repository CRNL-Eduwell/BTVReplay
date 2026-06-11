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
            throw new FileNotFoundException("DBFile3 => Filepath : " + FilePath + " does not exist ");
    }

    private void Load(string FilePath)
    {
        using (StreamReader streamReader = new StreamReader(FilePath))
        {
            Subjects = JsonConvert.DeserializeObject<List<Subject>>(streamReader.ReadToEnd(), new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto }) ?? new List<Subject>();
        }
    }

    public static bool Save(string FilePath, List<Subject> subjects)
    {
        try
        {
            string json = JsonConvert.SerializeObject(subjects, Formatting.Indented, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto, TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple });
            BrainTV.Tools.AtomicFile.WriteAllText(FilePath, json);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("Error saving .dbtv2 file at " + FilePath);
            Debug.LogException(e);
            return false;
        }
    }

    /// <summary>
    /// Converts v2 subjects and saves them to <paramref name="TargetFilePath"/> (must be a
    /// .dbtv2 path, distinct from the source file - the caller computes it explicitly so the
    /// source can never be overwritten).
    /// </summary>
    public static void ConvertOldDbFiles(string TargetFilePath, List<OldSubject> subjects)
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

        Save(TargetFilePath, newSubjects);
    }
}
