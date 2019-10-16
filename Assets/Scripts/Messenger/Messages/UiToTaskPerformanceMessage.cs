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

    public PROV NewProtocol
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