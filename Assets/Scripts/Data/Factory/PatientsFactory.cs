using BTV.Data;
using System;
using System.Collections.Generic;
using System.IO;

namespace Assets.Scripts.Data.Factory
{
    public class PatientsFactory
    {
        public static IPatientsContext GetPatientsContext(string FilePath)
        {
            FileInfo fileInfo = new FileInfo(FilePath);
            switch (fileInfo.Extension)
            {
                case ".txt":
                    return new DBFile(FilePath);
                default:
                    throw new ArgumentException("PatientsFactory.GetPatientsContext : file extension not supported => " + fileInfo.Extension); ;
            }
        }

        public static void SavePatients(string FilePath, List<Patient> Patients)
        {
            FileInfo fileInfo = new FileInfo(FilePath);
            switch (fileInfo.Extension)
            {
                case ".txt":
                    DBFile.Save(FilePath, Patients);
                    break;
                default:
                    throw new ArgumentException("PatientsFactory.SavePatients : file extension not supported => " + fileInfo.Extension); ;
            }
        }
    }
}
