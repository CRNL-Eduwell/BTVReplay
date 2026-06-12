using UnityEngine;
using System.IO;
using Newtonsoft.Json;
using System;
using Assets.Scripts.Data.Factory;

public class UserPreferencesFile : IUserPreferencesContext
{
    public UserPreferences UserPreferences { get; set; } = new UserPreferences();
    public string FilePath { get; set; } = "";

    public UserPreferencesFile()
    {

    }

    public UserPreferencesFile(string path)
    {
        FilePath = path;
        if (File.Exists(FilePath))
        {
            Load(path);
        }
        else
        {
            Debug.LogError("UserPreferencesFile => Filepath : " + FilePath + " does not exist ");
        }
    }

    private int Load(string FilePath)
    {
        try
        {
            using (StreamReader streamReader = new StreamReader(FilePath))
            {
                UserPreferences = JsonConvert.DeserializeObject<UserPreferences>(streamReader.ReadToEnd(), BtvJson.ReadSettings);
            }

            return 0;
        }
        catch (Exception e)
        {
            Console.WriteLine("The User Preferences file could not be read:");
            Console.WriteLine(e.Message);
            UserPreferences = new UserPreferences();
            return -1;
        }
    }

    public static void Save(string FilePath, UserPreferences preferences)
    {
        try
        {
            using (StreamWriter streamWriter = new StreamWriter(FilePath))
            {
                string json = JsonConvert.SerializeObject(preferences, Formatting.Indented, BtvJson.WriteSettings);
                streamWriter.Write(json);
                streamWriter.Close();
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error saving User Preferences file at " + FilePath);
            Debug.LogException(e);
        }
    }
}