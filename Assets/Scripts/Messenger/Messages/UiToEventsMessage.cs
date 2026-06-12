using UnityEngine;

class UiToEventsMessage
{
    public enum Task
    {
        LoadEventsFile = 0,
        SaveEventsFile = 1,
        ToggleAddEvents = 2,
        ToggleShowEvents = 3,
        DeleteSelectedEvents = 4,
        LoadCodeMatchingFile = 5,
    }

    public Task TaskToExecute
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