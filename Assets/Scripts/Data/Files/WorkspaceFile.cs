using UnityEngine;
using System.IO;
using Newtonsoft.Json;
using System;
using Assets.Scripts.Data.Factory;
using System.Collections.Generic;

public class WorkspaceFile : IWorkspaceContext
{
    public Workspace Workspace { get; set; } = new Workspace();
    public string FilePath { get; set; } = "";

    public WorkspaceFile()
    {

    }

    public WorkspaceFile(string path)
    {
        FilePath = path;
        if (File.Exists(FilePath))
        {
            Load(path);
        }
        else
        {
            Debug.LogError("WorkspaceFile => Filepath : " + FilePath + " does not exist ");
        }
    }

    private int Load(string FilePath)
    {
        try
        {
            using (StreamReader streamReader = new StreamReader(FilePath))
            {
                Workspace = JsonConvert.DeserializeObject<Workspace>(streamReader.ReadToEnd(), BtvJson.ReadSettings);
            }

            return 0;
        }
        catch (Exception e)
        {
            Debug.LogError("WorkspaceFile => could not read " + FilePath + ": " + e.Message);
            Workspace = new Workspace();
            return -1;
        }
    }

    public static void Save(string FilePath, Workspace workspace)
    {
        try
        {
            // Serialize before touching the file, then swap it in: a failure can no longer leave
            // the previous workspace truncated to nothing.
            string json = JsonConvert.SerializeObject(workspace, Formatting.Indented, BtvJson.WriteSettings);
            BrainTV.Tools.AtomicFile.WriteAllText(FilePath, json);
        }
        catch (Exception e)
        {
            Debug.LogError("Error saving Workspace file at " + FilePath);
            Debug.LogException(e);
        }
    }
}