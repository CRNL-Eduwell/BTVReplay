using UnityEngine;

class EditMenuMessage
{
    // 0 : EditDBName
    // 1 : DeleteDB
    // 2 : EditSubName
    // 3 : AddSubject
    // 4 : DeleteSubject
    public int TaskToExecute
    {
        get;
        set;
    }
}