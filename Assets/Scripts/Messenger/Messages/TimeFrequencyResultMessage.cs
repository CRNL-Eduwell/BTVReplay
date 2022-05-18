using System.Collections;
using System.Collections.Generic;
using BTV.Data;
using UnityEngine;

class TimeFrequencyResultMessage
{
    public float[][] TFData { get; set; }

    public BtvEvent EventOfInterest { get; set; }

    public int TraceIndex { get; set; }
}
