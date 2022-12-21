using BTV.Data;
using System;
using System.Collections.Generic;
using System.IO;

namespace Assets.Scripts.Data.Factory
{
    public class SubjectsFactory
    {
        public static ISubjectsContext GetEmptyContext()
        {
            return new DBFile3();
        }

        public static ISubjectsContext GetSubjectsContext(string FilePath)
        {
            FileInfo fileInfo = new FileInfo(FilePath);
            switch (fileInfo.Extension)
            {
                case ".txt":
                    {
                        DBFile file = new DBFile(FilePath);
                        DBFile2.ConvertOldDbFiles(FilePath, file.Patients);
                        DBFile2 file2 = new DBFile2(FilePath.Replace(".txt", ".dbtv"));
                        DBFile3.ConvertOldDbFiles(FilePath, file2.Subjects);
                        return new DBFile3(FilePath.Replace(".dbtv", ".dbtv2"));
                    }
                case ".dbtv":
                    {
                        DBFile2 file = new DBFile2(FilePath);
                        DBFile3.ConvertOldDbFiles(FilePath, file.Subjects);
                        return new DBFile3(FilePath.Replace(".dbtv", ".dbtv2"));
                    }
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
                    UnityEngine.Debug.Log("Old btv .txt file not supported anymore, please save to .dbtv2 file");
                    break;
                case ".dbtv":
                    UnityEngine.Debug.Log("Old btv .dbtv file not supported anymore, please save to .dbtv2 file");
                    break;
                case ".dbtv2":
                    DBFile3.Save(FilePath, subjects);
                    break;
                default:
                    throw new ArgumentException("SubjectsFactory.SavePatients : file extension not supported => " + fileInfo.Extension); ;
            }
        }
    }
}
