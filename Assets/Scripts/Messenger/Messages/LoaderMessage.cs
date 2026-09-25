using BTV.Services;
using UnityEngine;

class LoaderMessage 
{
    // EegFilesReady: the EEG files are loaded and the montage slots filled. Listeners that need
    // the file list used to wait for LoadBrain instead, which only worked because of ordering.
    public enum LoaderTask { None, MediaLoader, LoadVideo, LoadBrain, LoadTrace, EegFilesReady };

    public LoaderTask Task { get; set; } = LoaderTask.None;
    public Session PatientSession { get; set; }
    public bool HasAnatomy { get; set; } = false;
    public BrainDataContainer Anatomy { get; set; } = null;
    public EegTechnology Techno { get; set; } = EegTechnology.Intra;
    public string VideoPath { get; set; } = "";
    public int totalFileDuration { get; set; } = -1;
}
