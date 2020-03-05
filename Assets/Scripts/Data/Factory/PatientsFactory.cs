using BTV.Data;
using System;
using System.Collections.Generic;
using System.IO;

namespace Assets.Scripts.Data.Factory
{
    public class SubjectsFactory
    {
        public static ISubjectsContext GetSubjectsContext(string FilePath)
        {
            FileInfo fileInfo = new FileInfo(FilePath);
            switch (fileInfo.Extension)
            {
                case ".txt":
                    DBFile file = new DBFile(FilePath);
                    DBFile2.ConvertOldDbFiles(FilePath, file.Patients);
                    return new DBFile2(FilePath.Replace(".txt", ".dbtv"));
                case ".dbtv":
                    return new DBFile2(FilePath);
                default:
                    throw new ArgumentException("SubjectsFactory.GetSubjectsContext : file extension not supported => " + fileInfo.Extension); ;
            }
        }

        public static void SaveSubjects(string FilePath, List<Subject> subjects)
        {
            FileInfo fileInfo = new FileInfo(FilePath);
            switch (fileInfo.Extension)
            {
                case ".txt":
                    UnityEngine.Debug.Log("Old btv .txt file not supported anymore, please save to .dbtv file");
                    break;
                case ".dbtv":
                    DBFile2.Save(FilePath, subjects);
                    break;
                default:
                    throw new ArgumentException("SubjectsFactory.SavePatients : file extension not supported => " + fileInfo.Extension); ;
            }
        }
    }
}
