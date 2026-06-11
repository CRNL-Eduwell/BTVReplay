using UnityEngine;

public class DbPreferences 
{
    /// <summary>
    /// Path to look for database files
    /// </summary>
    public string Path { get; set; } = "";

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="path">Default path to look for database files</param>
    public DbPreferences(string path = "")
    {
        Path = path == "" ? Application.dataPath + "/Config/PatientBase/" : path;
    }

    public DbPreferences(DbPreferences preferences)
    {
        Path = preferences.Path;
    }
}