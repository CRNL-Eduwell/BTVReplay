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

    #region operators
    public override bool Equals(object obj)
    {
        if (obj is Experiment experiment)
        {
            bool sameName = Label == experiment.Label;
            bool sameFiles = Files.All(k => experiment.Files.Contains(k)) && Files.Count == experiment.Files.Count;
            bool sameVideo = Video == experiment.Video;
            return sameName && sameFiles && sameVideo;
        }
        else
        {
            return false;
        }
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public static bool operator ==(Experiment a, Experiment b)
    {
        if (ReferenceEquals(a, b))
        {
            return true;
        }

        if (((object)a == null) || ((object)b == null))
        {
            return false;
        }

        return a.Equals(b);
    }

    public static bool operator !=(Experiment a, Experiment b)
    {
        return !(a == b);
    }
    #endregion
}
