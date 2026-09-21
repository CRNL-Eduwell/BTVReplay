using BTV.Services;
using UnityEngine;

class LoaderMessage 
{
    public enum LoaderTask { None, MediaLoader, LoadVideo, LoadBrain, LoadTrace};

    public LoaderTask Task { get; set; } = LoaderTask.None;
    public Session PatientSession { get; set; }
    public bool HasAnatomy { get; set; } = false;
    public BrainDataContainer Anatomy { get; set; } = null;
    public EegTechnology Techno { get; set; } = EegTechnology.Intra;
    public string VideoPath { get; set; } = "";
    public int totalFileDuration { get; set; } = -1;
}
