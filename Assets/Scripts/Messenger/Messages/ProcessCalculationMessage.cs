using System.Collections;
using System.Collections.Generic;
using BTV.Data;
using UnityEngine;

class ProcessCalculationMessage
{
    public Calculations Task { get; set; }

    public BtvEvent BaselineEvent { get; set; }

    public BtvEvent EventOfInterest { get; set; }

    public int TraceIndex { get; set; }
}
