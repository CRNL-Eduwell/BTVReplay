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
            Subjects = JsonConvert.DeserializeObject<List<Subject>>(streamReader.ReadToEnd(), BtvJson.ReadSettings) ?? new List<Subject>();
        }
        // Portable "${NAME}/..." paths only exist in the file; in memory everything is absolute.
        SubjectPathPortability.ExpandTokens(Subjects);
    }

    public static bool Save(string FilePath, List<Subject> subjects)
    {
        try
        {
            // Serialize a tokenized copy: paths under the configured roots become portable
            // "${NAME}/..." entries on disk while the in-memory subjects stay absolute.
            List<Subject> portableSubjects = SubjectPathPortability.TokenizedCopy(subjects);
            string json = JsonConvert.SerializeObject(portableSubjects, Formatting.Indented, BtvJson.WriteSettings);
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
