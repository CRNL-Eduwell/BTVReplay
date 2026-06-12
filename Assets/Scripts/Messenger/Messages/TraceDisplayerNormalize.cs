using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class TraceDisplayerNormalize
{
    public enum Task
    {
        Reset = 0,
        Normalize = 1,
    }

    public Task TaskToExecute { get; set; } = Task.Reset;
    public float BeginTimeBaseline { get; set; } = -1;
    public float EndTimeBaseline { get; set; } = -1;
}
