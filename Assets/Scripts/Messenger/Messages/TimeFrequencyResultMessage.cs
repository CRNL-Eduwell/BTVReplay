using System.Collections;
using System.Collections.Generic;
using BTV.Data;
using UnityEngine;

class TimeFrequencyResultMessage
{
    public BtvEvent EventOfInterest { get; set; }

    public int TraceIndex { get; set; }

    public TimeFrequencyDataStructure TFDataStructure { get; set; }
}
