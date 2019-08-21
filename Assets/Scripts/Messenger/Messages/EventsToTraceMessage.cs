using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class EventsToTraceMessage
{    
    // 0 : Toggle Add Event
    // 1 : Toggle Show Event
    // 2 : Edit Event
    // 3 : Add Event
    // 4 : Remove Event

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

    public TraceEvent Event
    {
        get;
        set;
    }

    public int EventIndex
    {
        get;
        set;
    }
}
