using UnityEngine;
using UnityEditor;
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
                JsonSerializerSettings serializerSettings = new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.Auto,
                    TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                    Converters = new List<JsonConverter>() { new ColorConverter() } //need to add a color converter, otherwise not interpreted and triggers exceptions
                };

                Workspace = JsonConvert.DeserializeObject<Workspace>(streamReader.ReadToEnd(), serializerSettings);
            }

            return 0;
        }
        catch (Exception e)
        {
            Console.WriteLine("The Workspace file could not be read:");
            Console.WriteLine(e.Message);
            Workspace = new Workspace();
            return -1;
        }
    }

    public static void Save(string FilePath, Workspace workspace)
    {
        try
        {
            using (StreamWriter streamWriter = new StreamWriter(FilePath))
            {
                JsonSerializerSettings serializerSettings = new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.Auto,
                    TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                    Converters = new List<JsonConverter>() { new ColorConverter() } //need to add a color converter, otherwise not interpreted and triggers exceptions
                };

                string json = JsonConvert.SerializeObject(workspace, Formatting.Indented, serializerSettings );
                streamWriter.Write(json);
                streamWriter.Close();
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error saving Workspace file at " + FilePath);
            Debug.LogException(e);
        }
    }
}