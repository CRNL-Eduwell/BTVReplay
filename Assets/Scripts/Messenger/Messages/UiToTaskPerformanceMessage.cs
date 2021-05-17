using UnityEngine;

class UiToTaskPerformanceMessage
{
    // 0 : Update Event Processing for given protocol
    // 1 : Update Window Size
    public int TaskToExecute
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