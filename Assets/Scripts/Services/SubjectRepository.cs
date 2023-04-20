using Assets.Scripts.Data.Factory;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace BTV.Services.DatabaseService
{
    public class SubjectRepository : ViewModelBase
    {
        public string FilePath
        {
            get
            {
                return m_FilePath;
            }
            set
            {
                if (m_FilePath != value)
                {
                    m_FilePath = value;
                    RaisePropertyChanged("FilePath");
                    ShortName = value;
                }
            }
        }
        public string ShortName
        {
            get
            {
                return m_ShortName;
            }
            private set
            {
                m_ShortName = value.Split(new string[] { "\\", "/" }, System.StringSplitOptions.None).Last().Replace(".dbtv2", "");
                RaisePropertyChanged("ShortName");
            }
        }
        public ReadOnlyObservableCollection<Subject> Subjects { get; private set; } = null;

        private string m_FilePath = "";
        private string m_ShortName = "";
        private ObservableCollection<Subject> m_Subjects = null;

        public SubjectRepository(string path)
        {
            FilePath = path;
            if (string.IsNullOrEmpty(FilePath))
            {
                UnityEngine.Debug.LogError("Filepath " + FilePath + " is null or empty");
                return;
            }
            FileInfo file = new FileInfo(FilePath);
            if (!file.Exists)
            {
                UnityEngine.Debug.LogError("Filepath " + FilePath + "does not exist");
                return;
            }

            ISubjectsContext fileContext = SubjectsFactory.GetSubjectsContext(file.FullName);
            m_Subjects = new ObservableCollection<Subject>(fileContext.Subjects);
            Subjects = new ReadOnlyObservableCollection<Subject>(m_Subjects);
            FilePath = fileContext.FilePath;
        }

        public SubjectRepository(string path = "", List<Subject> subjects = null)
        {
            FilePath = path;
            m_Subjects = subjects == null ? new ObservableCollection<Subject>() : new ObservableCollection<Subject>(subjects);
            Subjects = new ReadOnlyObservableCollection<Subject>(m_Subjects);
        }

        public void Add(Subject subject)
        {
            if (m_Subjects == null) throw new NullReferenceException("Subject Observable Collection is null");

            UnityEngine.Debug.Log("Subject Added to repository");
            m_Subjects.Add(subject);
        }

        public void Remove(Subject subject)
        {
            if (m_Subjects.Contains(subject))
            {
                UnityEngine.Debug.Log("Subject removed from repository");
                m_Subjects.Remove(subject);
            }
        }

        public void Update(Subject oldSubject, Subject newSubject)
        {
            if (m_Subjects.Contains(oldSubject))
            {
                UnityEngine.Debug.Log("Old Subject found in repository, tring to replace");

                int index = m_Subjects.IndexOf(oldSubject);
                if (index != -1)
                {
                    UnityEngine.Debug.Log("Replacing Subject in repository");
                    UnityEngine.Debug.Log("Index : " + index);
                    m_Subjects[index] = new Subject(newSubject);
                }
            }
        }

        public void Save(string path = "")
        {
            if (string.IsNullOrEmpty(path)) path = FilePath;

            string backupPath = path.Replace(".dbtv2", "BU.dbtv2");
            if (File.Exists(path) && File.Exists(backupPath))
            {
                File.Copy(path, backupPath, true);
                SubjectsFactory.SaveSubjects(path, m_Subjects.ToList());
            }
            else
            {
                File.Create(path).Dispose();
                SubjectsFactory.SaveSubjects(path, m_Subjects.ToList());
                //==
                File.Create(backupPath).Dispose();
                SubjectsFactory.SaveSubjects(backupPath, m_Subjects.ToList());
            }

            if (!string.IsNullOrEmpty(path)) FilePath = path;
        }

        #region operators
        public override bool Equals(object obj)
        {
            if (obj is SubjectRepository subject)
            {
                bool sameName = FilePath == subject.FilePath;
                bool sameSubjects = Subjects.All(k => subject.Subjects.Contains(k)) && Subjects.Count == subject.Subjects.Count;
                return sameName && sameSubjects;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(SubjectRepository a, SubjectRepository b)
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            return a.Equals(b);
        }
        public static bool operator !=(SubjectRepository a, SubjectRepository b)
        {
            return !(a == b);
        }
        #endregion
    }
}