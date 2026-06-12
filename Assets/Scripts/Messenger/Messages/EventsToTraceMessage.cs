using BTV.Data;

class EventsToTraceMessage
{
    public enum Task
    {
        ToggleAddEvents = 0,
        ToggleShowEvents = 1,
        EditEvent = 2,
        AddEventToTrace = 3,
        RemoveEventFromTrace = 4,
        DisplayEvent = 5,
    }

    public Task TaskToExecute
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

    public BtvEvent Event
    {
        get;
        set;
    }

    public int EventIndex
    {
        get;
        set;
    }

    public int ParentWindowIndex
    {
        get;
        set;
    }
}
