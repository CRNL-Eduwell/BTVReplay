using UnityEngine;

class UiToTraceMessage
{
    // 0 : Update Gain
    // 1 : Update Offset
    // 2 : Update Grid Toggle
    // 3 : Update Window Size
    // 4 : Toggle Sonification
    // 5 : Update Sonification Sound
    // 6 : Update Color Picker
    // 7 : Update File Switcher
    public int TaskToExecute
    {
        get;
        set;
    }

    public int TraceID
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

    public bool IsGridOn
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

    public bool IsSonificationOn
    {
        get;
        set;
    }

    public int NewSonificationId
    {
        get;
        set;
    }

    public Color Color
    {
        get;
        set;
    }
}