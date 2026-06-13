using System.Collections.Generic;
using UnityEngine;

public class DbPreferences
{
    /// <summary>
    /// Path to look for database files
    /// </summary>
    public string Path { get; set; } = "";

    /// <summary>
    /// Machine-specific named roots used to make patient-base paths portable: a path stored as
    /// "${NAME}/..." resolves against the root named NAME on this machine. The same names must
    /// be configured (pointing wherever is right locally) on every machine sharing the bases.
    /// </summary>
    public List<PathRoot> PathRoots { get; set; } = new List<PathRoot>();

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
        PathRoots = preferences.PathRoots.ConvertAll(r => new PathRoot(r));
    }
}
