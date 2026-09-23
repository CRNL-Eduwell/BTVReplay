using UnityEngine;
using System.IO;
using Newtonsoft.Json;
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

    // Throws when the file cannot be read or holds no preferences. The old version substituted
    // a default UserPreferences (empty PathRoots) and reported only through Console.WriteLine,
    // so the next save from the options window silently overwrote the real file.
    private void Load(string FilePath)
    {
        using (StreamReader streamReader = new StreamReader(FilePath))
        {
            UserPreferences = JsonConvert.DeserializeObject<UserPreferences>(streamReader.ReadToEnd(), BtvJson.ReadSettings)
                ?? throw new InvalidDataException("The preferences file is empty.");
        }
    }

    // Throws on failure; the caller decides how to report it. Serialized first and swapped in
    // atomically, so a failed save cannot truncate the previous file.
    public static void Save(string FilePath, UserPreferences preferences)
    {
        string json = JsonConvert.SerializeObject(preferences, Formatting.Indented, BtvJson.WriteSettings);
        BrainTV.Tools.AtomicFile.WriteAllText(FilePath, json);
    }
}
