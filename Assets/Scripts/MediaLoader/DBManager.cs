using System.IO;
using System.Collections.Generic;
using Assets.Scripts.Data.Factory;

public class DBManager
{
    public List<Subject> Subjects = new List<Subject>();
    public int idCurrentPatientLoaded = 0;
    public string pathFile { get; private set; }
    string pathBUFile { get { return pathFile.Replace(".txt", "BU.txt"); } }

    private ISubjectsContext m_fileContext = null;

    public void SaveList(string dbPath = "")
    {
        if(dbPath != "")
            pathFile = dbPath;

        bool dbFileExist = File.Exists(pathFile) == true;
        bool dbBUExist = File.Exists(pathBUFile) == true;

        if (dbFileExist && dbBUExist)
        {
            File.Copy(pathFile, pathBUFile, true);
            SubjectsFactory.SaveSubjects(pathFile, Subjects);
        }
        else
        {
            File.Create(pathFile).Dispose();
            File.Create(pathBUFile).Dispose();
            SubjectsFactory.SaveSubjects(pathFile, Subjects);
            SubjectsFactory.SaveSubjects(pathBUFile, Subjects);
        }
    }

    public void LoadList(bool backUp, string dbPath = "")
    {
        string fileToLoad = "";

        if (dbPath == "")
            fileToLoad = pathFile;
        else
            fileToLoad = pathFile = dbPath;

        if (backUp)
            fileToLoad = pathBUFile;

        m_fileContext = SubjectsFactory.GetSubjectsContext(fileToLoad);
        Subjects = new List<Subject>(m_fileContext.Subjects);
    }

    public void AddSubject(Subject subject)
    {
        Subjects.Add(new Subject(subject));
    }

    public void RemoveSubjectAt(int index)
    {
        if (Subjects.Count > 0)
        {
            Subjects.Remove(Subjects[index]);
        }
    }
}