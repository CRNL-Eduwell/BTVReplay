using UnityEngine;

class FileMenuMessage
{
    public enum Task
    {
        NewDatabase = 0,
        OpenDatabase = 1,
        Save = 2,
        SaveAs = 3,
        Exit = 4,
    }

    public Task TaskToExecute
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