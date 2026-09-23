using UnityEngine;
using System;
using System.IO;
using Assets.Scripts.Data.Factory;

namespace BTV.Services.UserPreferencesService
{
    public static class UserPreferencesService
    {
        public static UserPreferences UserPreferences { get; set; } = null;

        public static string PATH = Path.Combine(Application.persistentDataPath, "Preferences.txt");

        /// <summary>
        /// Why the preferences file at <see cref="PATH"/> could not be read, or null when it was
        /// read (or did not exist yet). While set, the app runs on default preferences and
        /// <see cref="SavePreferences"/> refuses to write, so the unreadable file - which holds the
        /// user's path roots - stays on disk for recovery instead of being overwritten.
        /// </summary>
        public static string LoadError { get; private set; } = null;

        public static void LoadPreferences()
        {
            LoadError = null;
            if (File.Exists(PATH))
            {
                try
                {
                    IUserPreferencesContext userPreferencesContext = UserPreferencesFactory.GetPreferencesContext(PATH);
                    UserPreferences = new UserPreferences(userPreferencesContext.UserPreferences);
                }
                catch (Exception e)
                {
                    LoadError = e.Message;
                    UserPreferences = new UserPreferences();
                    Debug.LogError("UserPreferencesService => could not read " + PATH + ", running on defaults and refusing to save over it: " + e);
                }
            }
            else
            {
                UserPreferences = new UserPreferences();
            }
        }

        /// <summary>
        /// Writes the preferences and returns whether they were saved; <paramref name="error"/>
        /// says why not, in words fit for a dialog.
        /// </summary>
        public static bool SavePreferences(out string error)
        {
            if (LoadError != null)
            {
                error = "The preferences file could not be read when the application started, so it was not overwritten.\n\n"
                    + PATH + "\n" + LoadError + "\n\nFix or delete that file, then restart the application.";
                return false;
            }
            try
            {
                UserPreferencesFactory.SavePreferences(PATH, UserPreferences);
                error = null;
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError("UserPreferencesService => could not save " + PATH + ": " + e);
                error = "The preferences could not be saved to " + PATH + ":\n" + e.Message;
                return false;
            }
        }
    }
}
