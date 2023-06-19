using UnityEngine;

class MontageMessage
{
    // 0 : Update number of montage
    // 1 : Update selected montage
    public int TaskToExecute
    {
        get;
        set;
    }

    public int SelectedMontageID
    {
        get;
        set;
    }
}