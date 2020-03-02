using System.IO;
using System.Collections.Generic;
using Assets.Scripts.Data.Factory;

public class DBManager
{
    public List<Patient> currentPatients = new List<Patient>();
    public int idCurrentPatientLoaded = 0;
    public string pathFile { get; private set; }
    string pathBUFile { get { return pathFile.Replace(".txt", "BU.txt"); } }

    private IPatientsContext m_fileContext = null;

    public void SaveList(string dbPath = "")
    {
        if(dbPath != "")
            pathFile = dbPath;

        bool dbFileExist = File.Exists(pathFile) == true;
        bool dbBUExist = File.Exists(pathBUFile) == true;

        if (dbFileExist && dbBUExist)
        {
            File.Copy(pathFile, pathBUFile, true);
            PatientsFactory.SavePatients(pathFile, currentPatients);
        }
        else
        {
            File.Create(pathFile).Dispose();
            File.Create(pathBUFile).Dispose();
            PatientsFactory.SavePatients(pathFile, currentPatients);
            PatientsFactory.SavePatients(pathBUFile, currentPatients);
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

        m_fileContext = PatientsFactory.GetPatientsContext(fileToLoad);
        currentPatients = new List<Patient>(m_fileContext.Patients);
    }

    public void addPat(Patient thisPatient)
    {
        currentPatients.Add(new Patient(thisPatient));
    }

    public void removePatientAt(int index)
    {
        if (currentPatients.Count > 0)
        {
            currentPatients.Remove(currentPatients[index]);
        }
    }
}