using UnityEngine;

class UiToTaskPerformanceMessage
{
    public enum Task
    {
        ProcessProtocol = 0,
        UpdateTimeWindow = 1,
    }

    public Task TaskToExecute
    {
        get;
        set;
    }

    public Protocol NewProtocol
    {
        get;
        set;
    }

    //in seconds
    public int TimeWindow
    {
        get;
        set;
    }
}