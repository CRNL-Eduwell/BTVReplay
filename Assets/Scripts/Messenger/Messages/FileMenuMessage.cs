using UnityEngine;

class FileMenuMessage
{
    // 0 : NewDB
    // 1 : OpenDB
    // 2 : Save
    // 3 : SaveAs
    public int TaskToExecute
    {
        get;
        set;
    }

    public string FilePath
    {
        get;
        set;
    }
}