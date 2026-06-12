using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

class ReactionTimePointerMessage
{
    public enum Task
    {
        ShowAndUpdate = 0,
        Hide = 1,
    }

    public Task TaskToExecute
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

    public string ReactionTimeMs
    {
        get;
        set;
    }
}
