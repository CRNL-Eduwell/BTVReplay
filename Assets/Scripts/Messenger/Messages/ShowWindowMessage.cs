using UnityEngine;
using System.Collections;

class ShowWindowMessage
{
    // The message only ever shows a window: WindowsManager spawns the named prefab if no
    // instance exists. The old TaskToExecute field (0 show / 1 hide) was never read and
    // hiding was never implemented, so it was removed.
    public string WindowName
    {
        get;
        set;
    }
}