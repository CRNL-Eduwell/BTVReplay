using UnityEngine;

class UiToVideoMessage
{
    public enum Task
    {
        UpdateGain = 0,
        UpdateOffset = 1,
        ToggleAudioTrace = 2,
        UpdateAudioFile = 3,
    }

    public Task TaskToExecute
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
