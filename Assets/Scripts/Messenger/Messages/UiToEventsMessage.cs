using UnityEngine;

class UiToEventsMessage
{
    // 0 : Load File
    // 1 : Save File
    // 2 : Toggle Add Events
    // 3 : Toggle Show Events
    // 4 : Delete Events
    // 5 : Load Matching Code File
    public int TaskToExecute
    {
        get;
        set;
    }

    public string FilePathToLoad
    {
        get;
        set;
    }

    public string FilePathToSave
    {
        get;
        set;
    }

    public bool IsAddEventsOn
    {
        get;
        set;
    }

    public bool IsShowEventsOn
    {
        get;
        set;
    }
}