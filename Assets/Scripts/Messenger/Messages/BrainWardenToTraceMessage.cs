using UnityEngine;

class BrainWardenToTraceMessage
{
    public enum Task
    {
        PlotClicked = 0,
    }

    public Task TaskToExecute
    {
        get;
        set;
    }

    public GameObject ClickedElectrode
    {
        get;
        set;
    }

}