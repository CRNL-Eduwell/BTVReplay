using Assets.Scripts.Data.Factory;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using UnityEngine;

namespace BTV.Services.DatabaseService
{
    public static class DatabaseService
    {
        public static string DefaultPath { get { return Application.dataPath + "/Config/PatientBase/"; } }
        public static ObservableCollection<SubjectRepository> Databases { get; private set; } = new ObservableCollection<SubjectRepository>();

        public static void CreateNewDatabase(string filePath)
        {
            if (!string.IsNullOrEmpty(filePath))
            {
                UnityEngine.Debug.Log("Creating new db to " + filePath);
                Databases.Add(new SubjectRepository(filePath, null));
            }
        }

        public static void OpenDatabase(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                UnityEngine.Debug.LogError("Filepath " + filePath + " is null or empty");
                return;
            }
            FileInfo file = new FileInfo(filePath);
            if (!file.Exists)
            {
                UnityEngine.Debug.LogError("Filepath " + filePath + "does not exist");
                return;
            }

            UnityEngine.Debug.Log("Opening db file => " + filePath);
            Databases.Add(new SubjectRepository(filePath));
        }

        public static void UpdateDatabaseName(SubjectRepository element, string oldName, string newName)
        {
            if (Databases.Contains(element))
            {
                UnityEngine.Debug.Log("Update DB Name, contains element");
                int index = Databases.IndexOf(element);
                string filePath = Databases[index].FilePath;
                Databases[index].FilePath = filePath.Replace(oldName + ".dbtv", newName + ".dbtv");
            }
        }

        public static void DeleteDatabase(SubjectRepository element)
        {
            if (Databases.Contains(element))
            {
                UnityEngine.Debug.Log("Deleting DB, contains element");
                int index = Databases.IndexOf(element);
                Databases.RemoveAt(index);
            }
        }

        public static void EditSubjectName(SubjectRepository element, Subject subject, string newName)
        {
            if (Databases.Contains(element))
            {
                int index = Databases.IndexOf(element);
                if (Databases[index].Subjects.Contains(subject))
                {
                    UnityEngine.Debug.Log("Update Subject Name, contains element");
                    int subIndex = Databases[index].Subjects.IndexOf(subject);
                    Databases[index].Subjects[subIndex].PatientName = newName;
                }
            }
        }

        public static void AddSubjectToDatabase(SubjectRepository element)
        {
            if (Databases.Contains(element))
            {
                UnityEngine.Debug.Log("Add New Default Patient");
                int index = Databases.IndexOf(element);
                Subject subject = new Subject()
                {
                    PatientName = "Default Name"
                };
                Databases[index].Add(subject);
            }
        }

        public static void RemoveSubjectFromDatabase(SubjectRepository element, Subject subject)
        {
            if (Databases.Contains(element))
            {
                UnityEngine.Debug.Log("Remove subject , Repository found");
                int index = Databases.IndexOf(element);
                UnityEngine.Debug.Log("Remove subject , Trying to remove subject");
                Databases[index].Remove(subject);
            }
        }

        public static void UpdateSubjectFromDatabase(SubjectRepository element, Subject oldSubject, Subject newSubject)
        {
            if (Databases.Contains(element))
            {
                int index = Databases.IndexOf(element);
                if (index != -1)
                {
                    UnityEngine.Debug.Log("Replace subject , Repository found");
                    Databases[index].Update(oldSubject, newSubject);
                }
            }
        }

        public static void UpdateSubjectFromDatabase(int dbIndex, Subject oldSubject, Subject newSubject)
        {
            if (dbIndex != -1)
            {
                UnityEngine.Debug.Log("Replace subject in " + dbIndex + " , Repository found");
                Databases[dbIndex].Update(oldSubject, newSubject);
            }
        }
    }
}