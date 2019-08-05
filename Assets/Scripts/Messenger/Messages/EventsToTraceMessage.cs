using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class EventsToTraceMessage
{    
    // 0 : Toggle Add Event
    // 1 : Toggle Show Event
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
}
