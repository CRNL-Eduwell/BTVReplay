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

    /// <summary>
    /// The destination of a move or copy. The repository itself, not its name: two open
    /// databases can have the same file name in different folders.
    /// </summary>
    public BTV.Services.DatabaseService.SubjectRepository DestinationDatabase
    {
        get;
        set;
    }
}