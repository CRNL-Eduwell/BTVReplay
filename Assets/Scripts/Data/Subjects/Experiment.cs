using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Experiment
{
    public string Label { get; set; } = ""; 
    public Dictionary<string, IEegFileInfo> Files { get; set; } = new Dictionary<string, IEegFileInfo>();
    public string Video { get; set; } = "";

    public Experiment()
    {

    }

    public Experiment(string label, Dictionary<string, IEegFileInfo> files, string video)
    {
        Label = label;
        Files = files.ToDictionary(entry => entry.Key, entry => (IEegFileInfo)entry.Value.Clone());
        Video = video;
    }

    public Experiment(Experiment experiment)
    {
        Label = experiment.Label;
        Files = experiment.Files.ToDictionary(entry => entry.Key, entry => (IEegFileInfo)entry.Value.Clone());
        Video = experiment.Video;
    }
}
