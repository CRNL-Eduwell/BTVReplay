using UnityEngine;

class UiToVideoMessage
{
    // 0 : Update Gain
    // 1 : Update Offset
    // 2 : Toggle Audio Trace
    // 3 : Update Trace AudioFile
    public int TaskToExecute
    {
        get;
        set;
    }

    public float Gain
    {
        get;
        set;
    }

    public float Offset
    {
        get;
        set;
    }

    public bool IsTraceOn
    {
        get;
        set;
    }

    public int TraceID
    {
        get;
        set;
    }
}
