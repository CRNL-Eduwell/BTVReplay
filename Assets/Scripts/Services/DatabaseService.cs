using Assets.Scripts.Data.Factory;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using UnityEngine;
using BTV.Services.UserPreferencesService;

namespace BTV.Services.DatabaseService
{
    /// <summary>What <see cref="DatabaseService.OpenDatabase"/> did with the file it was given.</summary>
    public enum OpenDatabaseResult
    {
        Opened,
        /// <summary>No path: the file dialog was cancelled.</summary>
        NoPath,
        FileNotFound,
        /// <summary>The file exists but could not be read as a database; it was left untouched.</summary>
        LoadFailed,
        AlreadyOpen,
    }

    public static class DatabaseService
    {
        public static string DefaultPath { get { return UserPreferencesService.UserPreferencesService.UserPreferences.DatabasePreferences.Path; } }
        public static ObservableCollection<SubjectRepository> Databases { get; private set; } = new ObservableCollection<SubjectRepository>();

        /// <summary>
        /// Creates and registers an empty database at <paramref name="filePath"/>. Returns false, and
        /// registers nothing, when the file cannot be written: it used to register the repository
        /// anyway, so the base looked created while nothing existed on disk.
        /// </summary>
        public static bool CreateNewDatabase(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return false;

            BtvLog.Log("Creating new db to " + filePath);
            SubjectRepository db = new SubjectRepository(filePath, null);
            if (!db.Save()) return false;
            Databases.Add(db);
            return true;
        }

        /// <summary>
        /// Opens the database at <paramref name="filePath"/> and registers it. The result says why
        /// nothing was registered, so the caller can tell the user: every refusal used to be a
        /// console line only, and the manager looked as if the click had done nothing.
        /// </summary>
        public static OpenDatabaseResult OpenDatabase(string filePath)
        {
            return OpenDatabase(filePath, out _);
        }

        /// <param name="loadError">The reader's message when the result is <see cref="OpenDatabaseResult.LoadFailed"/>, null otherwise.</param>
        public static OpenDatabaseResult OpenDatabase(string filePath, out string loadError)
        {
            loadError = null;
            if (string.IsNullOrEmpty(filePath))
            {
                BtvLog.Log("OpenDatabase : Filepath is null or empty");
                return OpenDatabaseResult.NoPath;
            }
            FileInfo file = new FileInfo(filePath);
            if (!file.Exists)
            {
                UnityEngine.Debug.LogError("OpenDatabase : Filepath " + filePath + " does not exist");
                return OpenDatabaseResult.FileNotFound;
            }

            BtvLog.Log("OpenDatabase => " + filePath);
            SubjectRepository repository = new SubjectRepository(filePath);
            if (repository.Subjects == null)
            {
                // Load failed (the repository already logged why). Don't add it: every
                // consumer assumes Subjects is usable, and a failed load must never be
                // mistaken for an empty database.
                loadError = repository.LoadError;
                return OpenDatabaseResult.LoadFailed;
            }
            // Compare against the post-load path so re-opening a migrated .txt is caught too.
            if (Databases.Any(db => db.FilePath == repository.FilePath))
            {
                Debug.LogWarning("OpenDatabase : " + repository.FilePath + " is already open");
                return OpenDatabaseResult.AlreadyOpen;
            }
            Databases.Add(repository);
            return OpenDatabaseResult.Opened;
        }

        /// <summary>
        /// The text of the dialog for an <see cref="OpenDatabase"/> result, or null when there is
        /// nothing to tell (the base opened, or the file dialog was cancelled).
        /// </summary>
        public static string DescribeOpenFailure(OpenDatabaseResult result, string filePath, string loadError = null)
        {
            switch (result)
            {
                case OpenDatabaseResult.FileNotFound:
                    return "The file does not exist:\n" + filePath;
                case OpenDatabaseResult.LoadFailed:
                    return "The file could not be read as a patient database. It was left untouched:\n" + filePath
                        + (string.IsNullOrEmpty(loadError) ? "" : "\n\n" + loadError);
                case OpenDatabaseResult.AlreadyOpen:
                    return "This database is already open:\n" + filePath;
                default:
                    return null;
            }
        }

        /// <summary>
        /// The name to show for an open database: its short name, followed by its folder when
        /// another open database has the same short name (two files called alike in different
        /// folders), and by the full folder path when the folder names are alike too.
        /// </summary>
        public static string DisplayName(SubjectRepository database)
        {
            var sameName = Databases.Where(db => !ReferenceEquals(db, database) && db.ShortName == database.ShortName).ToList();
            if (sameName.Count == 0) return database.ShortName;

            string directory = Path.GetDirectoryName(database.FilePath) ?? "";
            string folder = Path.GetFileName(directory);
            bool folderIsUnique = !string.IsNullOrEmpty(folder)
                && sameName.All(db => Path.GetFileName(Path.GetDirectoryName(db.FilePath) ?? "") != folder);
            return database.ShortName + " (" + (folderIsUnique ? folder : directory) + ")";
        }

        public static bool UpdateDatabaseName(SubjectRepository element, string newName)
        {
            if (!Databases.Contains(element)) return false;
            if (string.IsNullOrWhiteSpace(newName) || newName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) return false;

            int index = Databases.IndexOf(element);
            string oldPath = Databases[index].FilePath;
            if (!oldPath.EndsWith(".dbtv2")) return false;
            if (Databases[index].ShortName == newName) return true;
            if (Databases.Any(db => db.ShortName == newName)) return false;

            string newPath = Path.Combine(Path.GetDirectoryName(oldPath), newName + ".dbtv2");
            if (File.Exists(newPath)) return false; // never clobber another base on disk

            // Rename the on-disk files too, otherwise the old base survives and reloads stale
            // data later. The main file moves first: the backup used to move first, so a failed
            // main move left the backup orphaned under the new name.
            string oldBackup = oldPath.Substring(0, oldPath.Length - ".dbtv2".Length) + "BU.dbtv2";
            string newBackup = newPath.Substring(0, newPath.Length - ".dbtv2".Length) + "BU.dbtv2";
            try
            {
                if (File.Exists(oldPath)) File.Move(oldPath, newPath);
            }
            catch (System.Exception e)
            {
                Debug.LogError("UpdateDatabaseName : could not rename " + oldPath + " to " + newPath);
                Debug.LogException(e);
                return false;
            }
            try
            {
                if (File.Exists(oldBackup) && !File.Exists(newBackup)) File.Move(oldBackup, newBackup);
            }
            catch (System.Exception e)
            {
                // The base itself is renamed; the backup is secondary and is rewritten on the next save.
                Debug.LogWarning("UpdateDatabaseName : renamed the base but could not move its backup " + oldBackup + ": " + e.Message);
            }
            BtvLog.Log("Update DB Name, contains element");
            Databases[index].FilePath = newPath;
            return true;
        }

        public static void DeleteDatabase(SubjectRepository element)
        {
            if (Databases.Contains(element))
            {
                BtvLog.Log("Deleting DB, contains element");
                int index = Databases.IndexOf(element);
                Databases.RemoveAt(index);
            }
        }

        public static bool EditSubjectName(SubjectRepository element, Subject subject, string newName)
        {
            if (string.IsNullOrWhiteSpace(newName)) return false;
            if (!Databases.Contains(element)) return false;

            int index = Databases.IndexOf(element);
            if (!Databases[index].Subjects.Contains(subject)) return false;
            if (Databases[index].Subjects.Any(s => s.PatientName == newName && !ReferenceEquals(s, subject))) return false;

            BtvLog.Log("Update Subject Name, contains element");
            // Replace instead of renaming in place: Subject instances are dictionary keys in
            // the UI lists and their hash depends on PatientName — mutating it strands the
            // entry and the next list refresh throws KeyNotFoundException. Going through
            // Update also raises CollectionChanged(Replace) so the lists re-key properly.
            Subject renamed = new Subject(subject) { PatientName = newName };
            Databases[index].Update(subject, renamed);
            return true;
        }

        public static void AddSubjectToDatabase(SubjectRepository element)
        {
            if (Databases.Contains(element))
            {
                BtvLog.Log("Add New Default Patient");
                int index = Databases.IndexOf(element);

                string name = "DefaultName";
                int count = 0;
                while (true)
                {
                    string temp = string.Format("{0}({1})", name, ++count);
                    if (Databases[index].Subjects.FirstOrDefault(x => x.PatientName == temp) == default)
                    {
                        name = temp;
                        break;
                    }
                }
                Subject subject = new Subject(name);
                Databases[index].Add(subject);
            }
        }

        public static bool AddSubjectToDatabase(SubjectRepository element, Subject subject)
        {
            if (Databases.Contains(element))
            {
                int index = Databases.IndexOf(element);
                if (!Databases[index].Subjects.Contains(subject))
                {
                    BtvLog.Log("Add Already existing Patient");
                    Databases[index].Add(subject);
                    return true;
                }
            }
            return false;
        }

        public static void RemoveSubjectFromDatabase(SubjectRepository element, Subject subject)
        {
            if (Databases.Contains(element))
            {
                BtvLog.Log("Remove subject , Repository found");
                int index = Databases.IndexOf(element);
                BtvLog.Log("Remove subject , Trying to remove subject");
                Databases[index].Remove(subject);
            }
        }

        public static void UpdateSubjectFromDatabase(SubjectRepository element, Subject oldSubject, Subject newSubject)
        {
            if (Databases.Contains(element))
            {
                int index = Databases.IndexOf(element);
                if (index != -1)
                {
                    BtvLog.Log("Replace subject , Repository found");
                    Databases[index].Update(oldSubject, newSubject);
                }
            }
        }

        public static void UpdateSubjectFromDatabase(int dbIndex, Subject oldSubject, Subject newSubject)
        {
            if (dbIndex != -1)
            {
                BtvLog.Log("Replace subject in " + dbIndex + " , Repository found");
                Databases[dbIndex].Update(oldSubject, newSubject);
            }
        }
    }
}