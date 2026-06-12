using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneralPreferences
{
    /// <summary>
    /// Default Path to export Screenshots
    /// </summary>
    public string ExportPath { get; set; } = "";

    /// <summary>
    /// Path to look for Vlc Dependency
    /// </summary>
    public string VlcPath { get; set; } = "";

    /// <summary>
    /// Machine-specific named roots used to make patient-base paths portable: a path stored as
    /// "${NAME}/..." resolves against the root named NAME on this machine. The same names must
    /// be configured (pointing wherever is right locally) on every machine sharing the bases.
    /// </summary>
    public List<PathRoot> PathRoots { get; set; } = new List<PathRoot>();

    public GeneralPreferences()
    {

    }

    public GeneralPreferences(GeneralPreferences preferences)
    {
        ExportPath = preferences.ExportPath;
        VlcPath = preferences.VlcPath;
        PathRoots = preferences.PathRoots.ConvertAll(r => new PathRoot(r));
    }
}
