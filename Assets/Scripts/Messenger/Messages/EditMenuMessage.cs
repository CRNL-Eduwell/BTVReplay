using UnityEngine;

class EditMenuMessage
{
    // 0 : EditDBName
    // 1 : DeleteDB
    // 2 : EditSubName
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