using UnityEngine;

class EditMenuMessage
{
    public enum Task
    {
        RenameDatabase = 0,
        CloseDatabase = 1,
        RenameSubject = 2,
        AddSubject = 3,
        DeleteSubject = 4,
        MoveSubjects = 5,
        CopySubjects = 6,
    }

    public Task TaskToExecute
    {
        get;
        set;
    }

    public string DestinationDatabase
    {
        get;
        set;
    }
}