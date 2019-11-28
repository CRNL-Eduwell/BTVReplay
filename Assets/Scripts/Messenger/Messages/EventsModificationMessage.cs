using BTV.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class EventsModificationMessage
{
    // 0 : Add Event
    // 1 : Modify Event
    // 2 : Delete Event
    // 3 : Edit Event
    // 4 : Calculate Plot Correlation
    // 5 : Calculate All Plots Correlation
    public int TaskToExecute
    {
        get;
        set;
    }

    public BtvEvent Event
    {
        get;
        set;
    }

    public BtvEvent EventMemory
    {
        get;
        set;
    }
}
