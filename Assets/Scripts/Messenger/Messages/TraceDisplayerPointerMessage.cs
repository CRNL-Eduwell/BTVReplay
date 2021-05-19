using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

class TraceDisplayerPointerMessage
{
    // 0 : Show and Update Pointer
    // 1 : Hide
    public int TaskToExecute
    {
        get;
        set;
    }

    public Vector3 PointerPosition
    {
        get;
        set;
    }

    public bool ShowPointer
    {
        get;
        set;
    }

    public string Code
    {
        get;
        set;
    }

    public string DurationTimeMs
    {
        get;
        set;
    }
}
