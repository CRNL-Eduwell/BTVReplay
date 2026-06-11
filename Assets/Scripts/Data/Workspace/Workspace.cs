using UnityEngine;

public class Workspace 
{
    public BrainParameters BrainParameters { get; set; } = null;
    public TraceParameters Trace1 { get; set; } = null;
    public TraceParameters Trace2 { get; set; } = null;

    public Workspace()
    {
        BrainParameters = new BrainParameters();
        Trace1 = new TraceParameters();
        Trace2 = new TraceParameters();
    }

    public Workspace(BrainParameters brain, TraceParameters trace1, TraceParameters trace2)
    {
        BrainParameters = new BrainParameters(brain);
        Trace1 = new TraceParameters(trace1);
        Trace2 = new TraceParameters(trace2);
    }
}