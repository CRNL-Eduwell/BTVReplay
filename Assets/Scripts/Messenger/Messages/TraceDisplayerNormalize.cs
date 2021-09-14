using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class TraceDisplayerNormalize
{
    public int TaskToExecute { get; set; } = 0; //0 reset, 1 normalize
    public float BeginTimeBaseline { get; set; } = -1;
    public float EndTimeBaseline { get; set; } = -1;
}
