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
                        // Conversion targets are computed from the extension, never by string
                        // replacement on the source path: a Replace(".dbtv", ".dbtv2") applied
                        // to a .txt path silently no-ops and used to overwrite the source file.
                        string dbtvPath = Path.ChangeExtension(FilePath, ".dbtv");
                        string dbtv2Path = Path.ChangeExtension(FilePath, ".dbtv2");
                        DBFile file = new DBFile(FilePath);
                        DBFile2.ConvertOldDbFiles(dbtvPath, file.Patients);
                        DBFile2 file2 = new DBFile2(dbtvPath);
                        DBFile3.ConvertOldDbFiles(dbtv2Path, file2.Subjects);
                        return new DBFile3(dbtv2Path);
                    }
                case ".dbtv":
                    {
                        string dbtv2Path = Path.ChangeExtension(FilePath, ".dbtv2");
                        DBFile2 file = new DBFile2(FilePath);
                        DBFile3.ConvertOldDbFiles(dbtv2Path, file.Subjects);
                        return new DBFile3(dbtv2Path);
                    }
                case ".dbtv2":
                    {
                        return new DBFile3(FilePath);
                    }
                default:
                    throw new ArgumentException("SubjectsFactory.GetSubjectsContext : file extension not supported => " + fileInfo.Extension); ;
            }
        }

        public static bool SaveSubjects(string FilePath, List<Subject> subjects)
        {
            FileInfo fileInfo = new FileInfo(FilePath);
            switch (fileInfo.Extension)
            {
                case ".txt":
                    UnityEngine.Debug.LogError("Old btv .txt file not supported anymore, please save to .dbtv2 file");
                    return false;
                case ".dbtv":
                    UnityEngine.Debug.LogError("Old btv .dbtv file not supported anymore, please save to .dbtv2 file");
                    return false;
                case ".dbtv2":
                    return DBFile3.Save(FilePath, subjects);
                default:
                    throw new ArgumentException("SubjectsFactory.SavePatients : file extension not supported => " + fileInfo.Extension);
            }
        }
    }
}
