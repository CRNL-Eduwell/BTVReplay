using UnityEngine;

class UiToTraceMessage
{
    public enum Task
    {
        UpdateGain = 0,
        UpdateOffset = 1,
        ToggleGrid = 2,
        UpdateWindowSize = 3,
        ToggleSonification = 4,
        UpdateSonificationSound = 5,
        UpdateColor = 6,
        UpdateFile = 7,
    }

    public Task TaskToExecute
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

    public int FileID
    {
        get;
        set;
    }
}