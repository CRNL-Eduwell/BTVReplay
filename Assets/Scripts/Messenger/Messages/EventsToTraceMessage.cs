using BTV.Data;

class EventsToTraceMessage
{    
    // 0 : Toggle Add Event
    // 1 : Toggle Show Event
    // 2 : Edit Event
    // 3 : Add Event
    // 4 : Remove Event
    // 5 : Display Event

    public int TaskToExecute
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
