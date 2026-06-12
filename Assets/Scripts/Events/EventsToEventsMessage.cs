using System.Collections;
using System.Collections.Generic;
using BTV.Data;
using UnityEngine;

class EventsToEventsMessage
{
    public enum Task
    {
        MoveCursor = 0,
    }

    public Task TaskToExecute { get; set; }
    public float XPositionPercentage { get; set; }
    public float YPositionPercentage { get; set; }
    public BtvEvent BTVEvent { get; set; }
    public int ParentWindowIndex { get; set; }
}
