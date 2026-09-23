using Assets.Scripts.Data.Factory;
using BTV.Data;
using BTV.Services.DatabaseService;
using NUnit.Framework;
using System.Collections.Specialized;
using System.IO;
using UnityEngine.TestTools;

/// <summary>
/// Edit-mode tests for DatabaseService covering the patient-DB edit bugs: in-place subject
/// rename corrupting the UI selection dictionaries, duplicate/invalid names, corrupt bases
/// entering the service, and database rename leaving stale files on disk.
/// </summary>
public class DatabaseServiceTests
{
    private string m_TestDir;

    [SetUp]
    public void SetUp()
    {
        m_TestDir = Path.Combine(Path.GetTempPath(), "BTVReplayTests_" + Path.GetRandomFileName().Replace(".", ""));
        Directory.CreateDirectory(m_TestDir);
        ClearDatabases();
    }

    [TearDown]
    public void TearDown()
    {
        ClearDatabases();
        if (Directory.Exists(m_TestDir)) Directory.Delete(m_TestDir, true);
    }

    private static void ClearDatabases()
    {
        // DatabaseService is static: leave no state behind for the next test.
        while (DatabaseService.Databases.Count > 0)
            DatabaseService.DeleteDatabase(DatabaseService.Databases[0]);
    }

    private SubjectRepository CreateDatabase(string name, params string[] patientNames)
    {
        string path = Path.Combine(m_TestDir, name + ".dbtv2");
        DatabaseService.CreateNewDatabase(path);
        SubjectRepository db = DatabaseService.Databases[DatabaseService.Databases.Count - 1];
        foreach (string patientName in patientNames)
            DatabaseService.AddSubjectToDatabase(db, new Subject(patientName));
        if (patientNames.Length > 0) db.Save();
        return db;
    }

    #region EditSubjectName
    [Test]
    public void EditSubjectName_ReplacesInstanceInsteadOfMutatingKey()
    {
        SubjectRepository db = CreateDatabase("base", "A");
        Subject original = db.Subjects[0];

        Assert.IsTrue(DatabaseService.EditSubjectName(db, original, "B"));

        Assert.AreEqual("A", original.PatientName,
            "the original instance must not be mutated: it is a dictionary key in the UI lists and its hash depends on the name");
        Assert.AreEqual(1, db.Subjects.Count);
        Assert.AreEqual("B", db.Subjects[0].PatientName);
        Assert.AreNotSame(original, db.Subjects[0]);
    }

    [Test]
    public void EditSubjectName_RaisesReplaceEvent()
    {
        SubjectRepository db = CreateDatabase("base", "A");
        NotifyCollectionChangedAction? received = null;
        ((INotifyCollectionChanged)db.Subjects).CollectionChanged += (s, e) => received = e.Action;

        DatabaseService.EditSubjectName(db, db.Subjects[0], "B");

        Assert.AreEqual(NotifyCollectionChangedAction.Replace, received,
            "a rename must surface as Replace so the UI lists re-key their selection dictionaries");
    }

    [Test]
    public void EditSubjectName_RefusesDuplicateOrEmptyName()
    {
        SubjectRepository db = CreateDatabase("base", "A", "B");
        Subject subject = db.Subjects[0];

        Assert.IsFalse(DatabaseService.EditSubjectName(db, subject, "B"), "name already used by another subject");
        Assert.IsFalse(DatabaseService.EditSubjectName(db, subject, "   "), "blank name");
        Assert.AreEqual("A", db.Subjects[0].PatientName);

        Assert.IsTrue(DatabaseService.EditSubjectName(db, subject, "A"), "renaming a subject to its own name is a no-op, not a conflict");
    }
    #endregion

    #region OpenDatabase
    [Test]
    public void OpenDatabase_IgnoresCorruptBase()
    {
        // The load failure logs errors on purpose; the runner would otherwise fail the test.
        LogAssert.ignoreFailingMessages = true;

        string path = Path.Combine(m_TestDir, "corrupt.dbtv2");
        File.WriteAllText(path, "{ this is not json");

        DatabaseService.OpenDatabase(path);

        Assert.AreEqual(0, DatabaseService.Databases.Count,
            "a base that failed to load must not enter the service: consumers assume Subjects is usable");
    }

    [Test]
    public void OpenDatabase_RefusesSamePathTwice()
    {
        LogAssert.ignoreFailingMessages = true;

        string path = Path.Combine(m_TestDir, "base.dbtv2");
        Assert.IsTrue(DBFile3.Save(path, new System.Collections.Generic.List<Subject> { new Subject("A") }));

        DatabaseService.OpenDatabase(path);
        DatabaseService.OpenDatabase(path);

        Assert.AreEqual(1, DatabaseService.Databases.Count, "opening the same base twice must not create a duplicate repository");
    }
    #endregion

    #region UpdateDatabaseName
    [Test]
    public void UpdateDatabaseName_RenamesFilesOnDisk()
    {
        SubjectRepository db = CreateDatabase("oldname", "A");
        string oldPath = db.FilePath;

        Assert.IsTrue(DatabaseService.UpdateDatabaseName(db, "newname"));

        Assert.AreEqual("newname", db.ShortName);
        Assert.IsTrue(File.Exists(Path.Combine(m_TestDir, "newname.dbtv2")), "main file should be moved");
        Assert.IsTrue(File.Exists(Path.Combine(m_TestDir, "newnameBU.dbtv2")), "backup file should be moved");
        Assert.IsFalse(File.Exists(oldPath), "the old file must not survive the rename");
        Assert.IsFalse(File.Exists(Path.Combine(m_TestDir, "oldnameBU.dbtv2")));
        Assert.AreEqual(1, new DBFile3(db.FilePath).Subjects.Count, "the renamed base must still load");
    }

    [Test]
    public void UpdateDatabaseName_RefusesInvalidOrConflictingNames()
    {
        SubjectRepository alpha = CreateDatabase("alpha", "A");
        CreateDatabase("beta", "B");

        Assert.IsFalse(DatabaseService.UpdateDatabaseName(alpha, "beta"), "another open base already uses that name");
        Assert.IsFalse(DatabaseService.UpdateDatabaseName(alpha, "   "), "blank name");
        Assert.IsFalse(DatabaseService.UpdateDatabaseName(alpha, "in/valid"), "invalid filename characters");

        File.WriteAllText(Path.Combine(m_TestDir, "ondisk.dbtv2"), "unrelated");
        Assert.IsFalse(DatabaseService.UpdateDatabaseName(alpha, "ondisk"), "must never clobber an existing file on disk");

        Assert.AreEqual("alpha", alpha.ShortName);
        Assert.IsTrue(File.Exists(Path.Combine(m_TestDir, "alpha.dbtv2")), "a refused rename must leave the files untouched");
        Assert.IsTrue(File.Exists(Path.Combine(m_TestDir, "alphaBU.dbtv2")));
    }
    #endregion

    #region AddSubjectToDatabase
    [Test]
    public void CopyThenRename_DoesNotAffectTheOtherDatabase()
    {
        // Regression: copy used to insert the same Subject instance into both databases and
        // rename mutated it in place, so renaming the copy renamed the original too.
        SubjectRepository source = CreateDatabase("source", "Patient");
        SubjectRepository destination = CreateDatabase("destination");
        Subject original = source.Subjects[0];

        // CopySubjectsToDatabase copies with a deep copy, mirrored here.
        Assert.IsTrue(DatabaseService.AddSubjectToDatabase(destination, new Subject(original)));

        Assert.IsTrue(DatabaseService.EditSubjectName(destination, destination.Subjects[0], "Renamed"));
        Assert.AreEqual("Patient", source.Subjects[0].PatientName, "renaming the copy must not rename the original");
        Assert.AreEqual("Renamed", destination.Subjects[0].PatientName);

        Assert.IsTrue(DatabaseService.EditSubjectName(source, source.Subjects[0], "Renamed2"));
        Assert.AreEqual("Renamed", destination.Subjects[0].PatientName, "renaming the original must not rename the copy");
    }

    [Test]
    public void AddSubjectToDatabase_RefusesValueEqualDuplicate()
    {
        SubjectRepository db = CreateDatabase("base");
        Subject subject = new Subject("A");

        Assert.IsTrue(DatabaseService.AddSubjectToDatabase(db, subject));
        Assert.IsFalse(DatabaseService.AddSubjectToDatabase(db, new Subject(subject)),
            "a value-equal copy must be refused (this is what makes copying to the same base a no-op)");
        Assert.AreEqual(1, db.Subjects.Count);
    }
    #endregion

    [Test]
    public void CreateNewDatabase_UnwritablePath_ReturnsFalseAndRegistersNothing()
    {
        string path = Path.Combine(m_TestDir, "missing-folder", "new.dbtv2");
        LogAssert.Expect(UnityEngine.LogType.Error, new System.Text.RegularExpressions.Regex("Error saving .dbtv2 file"));
        LogAssert.Expect(UnityEngine.LogType.Exception, new System.Text.RegularExpressions.Regex("DirectoryNotFoundException"));

        Assert.IsFalse(DatabaseService.CreateNewDatabase(path));

        Assert.AreEqual(0, DatabaseService.Databases.Count, "a base that could not be written must not appear as created");
    }
}
