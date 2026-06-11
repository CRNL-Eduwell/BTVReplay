using UnityEngine;
using System;
using System.IO;

namespace Assets.Scripts.Data.Factory
{
    public class UserPreferencesFactory
    {
        public static IUserPreferencesContext GetPreferencesContext(string FilePath)
        {
            FileInfo fileInfo = new FileInfo(FilePath);
            switch (fileInfo.Extension)
            {
                case ".txt":
                    return new UserPreferencesFile(FilePath);
                default:
                    throw new ArgumentException("UserPreferencesFactory.GetPreferencesContext : file extension not supported => " + fileInfo.Extension); ;
            }
        }

        public static void SavePreferences(string FilePath, UserPreferences preferences)
        {
            FileInfo fileInfo = new FileInfo(FilePath);
            switch (fileInfo.Extension)
            {
                case ".txt":
                    UserPreferencesFile.Save(FilePath, preferences);
                    break;
                default:
                    throw new ArgumentException("UserPreferencesFactory.SavePatients : file extension not supported => " + fileInfo.Extension); ;
            }
        }
    }
}