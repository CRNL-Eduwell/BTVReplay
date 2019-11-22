using System.Collections.Generic;

public class Intra_Electrode
{
    public string Label
    {
        get;
        set;
    }

    public List<Intra_Plot> Plots
    {
        get;
        set;
    }

    public Intra_Electrode(string label)
    {
        Label = label;
        Plots = new List<Intra_Plot>();
    }
}