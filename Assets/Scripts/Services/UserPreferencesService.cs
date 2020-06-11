using UnityEngine;
using UnityEditor;
using System.IO;
using Assets.Scripts.Data.Factory;

namespace BTV.Services.UserPreferencesService
{
    public static class UserPreferencesService
    {
        public static UserPreferences UserPreferences { get; set; } = null;

        public static string PATH = Path.Combine(Application.persistentDataPath, "Preferences.txt");

        public static void LoadPreferences()
        {
            if (File.Exists(PATH))
            {
                IUserPreferencesContext userPreferencesContext = UserPreferencesFactory.GetPreferencesContext(PATH);
                UserPreferences = new UserPreferences(userPreferencesContext.UserPreferences);
            }
            else
            {
                UserPreferences = new UserPreferences();
            }
        }

        public static void SavePreferences()
        {
            UserPreferencesFactory.SavePreferences(PATH, UserPreferences);
        }
    }
}

