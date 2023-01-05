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
                return m_Subject != null ? m_Subject.PatientName : "Subject not defined";
            }
        }

        public static string VideoPath
        {
            get
            {
                return (m_Subject != null && m_ExamIndex != -1) ? m_Subject.Experiments[m_ExamIndex].Video : "";
            }
        }

        private static Subject m_Subject = null;
        private static string m_ExamLabel = "";
        private static int m_ExamIndex = -1;

        public static void Reset()
        {
            m_Subject = null;
            m_ExamLabel = "";
            m_ExamIndex = -1;
        }

        public static void SetSubject(Subject subject, string examLabel)
        {
            m_Subject = subject;
            m_ExamLabel = examLabel;
            for (int i = 0; i < m_Subject.Experiments.Count; i++)
            {
                if (m_Subject.Experiments[i].Label == m_ExamLabel)
                {
                    m_ExamIndex = i;
                    break;
                }
            }
        }

        public static List<string> GetSubjectFileKeys()
        {
            List<string> keys = new List<string>();
            foreach (var item in m_Subject.Experiments[m_ExamIndex].Files)
            {
                string label = item.Equals(default(KeyValuePair<string, IEegFileInfo>)) ? "NO FILE" : item.Key;
                keys.Add(label);
            }
            return keys;
        }

        public static List<IEegFileInfo> GetSubjectFiles()
        {
            List<IEegFileInfo> files = new List<IEegFileInfo>();
            foreach (var item in m_Subject.Experiments[m_ExamIndex].Files)
            {
                files.Add(item.Value);
            }
            return files;
        }

        public static List<KeyValuePair<string, IEegFileInfo>> GetSubjectFilesAndDescription()
        {
            List<KeyValuePair<string, IEegFileInfo>> files = new List<KeyValuePair<string, IEegFileInfo>>();
            foreach (var item in m_Subject.Experiments[m_ExamIndex].Files)
            {
                files.Add(new KeyValuePair<string, IEegFileInfo>(item.Key, item.Value));
            }
            return files;
        }

        public static BrainDataContainer GetBrainDataContainer(string label)
        {
            m_Subject.AnatomicalSpaces.TryGetValue(label, out BrainDataContainer patContainer);
            return patContainer;
        }
    }
}