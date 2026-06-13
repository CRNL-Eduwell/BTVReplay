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

    public GeneralPreferences()
    {

    }

    public GeneralPreferences(GeneralPreferences preferences)
    {
        ExportPath = preferences.ExportPath;
        VlcPath = preferences.VlcPath;
    }
}
