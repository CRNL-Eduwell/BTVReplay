using UnityEngine;

class MontageMessage
{
    public enum Task
    {
        UpdateMontageList = 0,
        SelectMontage = 1,
    }

    public Task TaskToExecute
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