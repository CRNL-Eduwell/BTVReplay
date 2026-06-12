using BTV.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class EventsModificationMessage
{
    public enum Task
    {
        AddEvent = 0,
        ModifyEvent = 1,
        DeleteEvent = 2,
        EditEvent = 3,
        ComputeCorrelation = 4,
        ComputeCorrelation2D = 5,
    }

    public Task TaskToExecute
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

    public int ParentWindowIndex
    {
        get;
        set;
    }
}
