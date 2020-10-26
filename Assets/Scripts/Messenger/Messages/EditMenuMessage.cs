using UnityEngine;

class EditMenuMessage
{
    // 0 : RenameDatabase
    // 1 : CloseDatabase
    // 2 : RenameSubject
    // 3 : AddSubject
    // 4 : DeleteSubject
    // 5 : MoveSubjects
    // 6 : CopySubjects
    public int TaskToExecute
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