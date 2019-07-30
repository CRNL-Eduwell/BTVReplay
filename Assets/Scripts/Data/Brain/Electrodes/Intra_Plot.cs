using UnityEngine;

public class Intra_Plot : EEG_Plot
{
    /// <summary>
    /// Label of the intracerebral plot according to the EEG File
    /// </summary>
    public new string Label
    {
        get;
        set;
    }
    /// <summary>
    /// Label of the intracerebral plot according to the Atlas File
    /// They use a 0 in front of numbers smaller than 10
    /// </summary>
    public string AtlasLabel
    {
        get
        {
            return Parent + IdToStr(Id);
        }
    }
    public string Parent
    {
        get;
        set;
    }
    public int Id
    {
        get;
        set;
    }
    public MarsAtlas_plot Atlas
    {
        get;
        set;
    }

    public Intra_Plot(Intra_Plot plotToCopy) : base(plotToCopy.Label, plotToCopy.Coordinates)
    {
        Label = plotToCopy.Label;
        Parent = plotToCopy.Parent;
        Id = plotToCopy.Id;
        Atlas = plotToCopy.Atlas;
    }

    public Intra_Plot(string label, int id, Vector3 coordinates) : base("", coordinates)
    {
        Parent = label;
        Id = id;
        Label = label + id;
        base.Label = Label;
    }

    private string IdToStr(int id)
    {
        if (id == -1)
            return "";
        else if (id < 10)
            return "0" + id;
        else
            return id.ToString();
    }
}