using System;
using System.Collections.Generic;

namespace BTV.Services.SubjectInfoService
{
    public static class SubjectInfoService
    {
        public static string SubjectName
        {
            get
            {
                return Session.Current.Subject != null ? Session.Current.Subject.PatientName : "Subject not defined";
            }
        }

        public static string VideoPath
        {
            get
            {
                Session session = Session.Current;
                return (session.Subject != null && session.ExamIndex != -1) ? session.Subject.Experiments[session.ExamIndex].Video : "";
            }
        }

        public static void Reset()
        {
            Session session = Session.Current;
            session.Subject = null;
            session.ExamLabel = "";
            session.ExamIndex = -1;
        }

        public static void SetSubject(Subject subject, string examLabel)
        {
            Session session = Session.Current;
            session.Subject = subject;
            session.ExamLabel = examLabel;
            session.ExamIndex = -1;
            for (int i = 0; i < session.Subject.Experiments.Count; i++)
            {
                if (session.Subject.Experiments[i].Label == session.ExamLabel)
                {
                    session.ExamIndex = i;
                    break;
                }
            }
        }

        public static List<string> GetSubjectFileKeys()
        {
            Session session = Session.Current;
            List<string> keys = new List<string>();
            foreach (var item in session.Subject.Experiments[session.ExamIndex].Files)
            {
                string label = item.Equals(default(KeyValuePair<string, IEegFileInfo>)) ? "NO FILE" : item.Key;
                keys.Add(label);
            }
            return keys;
        }

        public static List<IEegFileInfo> GetSubjectFiles()
        {
            Session session = Session.Current;
            List<IEegFileInfo> files = new List<IEegFileInfo>();
            foreach (var item in session.Subject.Experiments[session.ExamIndex].Files)
            {
                files.Add(item.Value);
            }
            return files;
        }

        public static List<KeyValuePair<string, IEegFileInfo>> GetSubjectFilesAndDescription()
        {
            Session session = Session.Current;
            List<KeyValuePair<string, IEegFileInfo>> files = new List<KeyValuePair<string, IEegFileInfo>>();
            foreach (var item in session.Subject.Experiments[session.ExamIndex].Files)
            {
                files.Add(new KeyValuePair<string, IEegFileInfo>(item.Key, item.Value));
            }
            return files;
        }

        public static BrainDataContainer GetBrainDataContainer(string label)
        {
            Session.Current.Subject.AnatomicalSpaces.TryGetValue(label, out BrainDataContainer patContainer);
            return patContainer;
        }
    }
}
