using BTV.Services.DatabaseService;
using SFB;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BTV.UI.PatientBaseManager
{
    public class FileMenu : Menu
    {
        [SerializeField]
        private Button m_NewDatabase = null;
        [SerializeField]
        private Button m_OpenDatabase = null;
        [SerializeField]
        private Button m_SaveDatabase = null;
        [SerializeField]
        private Button m_SaveDatabaseAs = null;
        [SerializeField]
        private Button m_Close = null;

        private void Start()
        {
            m_NewDatabase.onClick.AddListener(CreateNewDatabase);
            m_OpenDatabase.onClick.AddListener(OpenDatabase);
            m_SaveDatabase.onClick.AddListener(SaveDatabase);
            m_SaveDatabaseAs.onClick.AddListener(SaveDatabaseAs);
            m_Close.onClick.AddListener(CloseWindow);
        }

        private void OnDestroy()
        {
            m_NewDatabase.onClick.RemoveAllListeners();
            m_OpenDatabase.onClick.RemoveAllListeners();
            m_SaveDatabase.onClick.RemoveAllListeners();
            m_SaveDatabaseAs.onClick.RemoveAllListeners();
            m_Close.onClick.RemoveAllListeners();
        }

        private void CreateNewDatabase()
        {
            var extensionList = new[] { new ExtensionFilter("BrainTV Database Files", "dbtv2") };
#if UNITY_STANDALONE_OSX
            FileBrowser.GetSavedFileNameAsync((str) =>
            {
                FileMenuMessage message = new FileMenuMessage
                {
                    TaskToExecute = FileMenuMessage.Task.NewDatabase,
                    FilePath = str
                };
                Messenger.Default.Send(message, MessageContext.FileMenuMessage);
            }, extensionList, "Save Database To", DatabaseService.DefaultPath);
#else
            string filePath = FileBrowser.GetSavedFileName(extensionList, "Save Database To", DatabaseService.DefaultPath);
            FileMenuMessage message = new FileMenuMessage
            {
                TaskToExecute = FileMenuMessage.Task.NewDatabase,
                FilePath = filePath
            };
            Messenger.Default.Send(message, MessageContext.FileMenuMessage);
#endif
            Close();
        }

        private void OpenDatabase()
        {
#if UNITY_STANDALONE_OSX
            FileBrowser.GetExistingFileNameAsync((str) =>
            {
                FileMenuMessage message = new FileMenuMessage
                {
                    TaskToExecute = FileMenuMessage.Task.OpenDatabase,
                    FilePath = str
                };
                Messenger.Default.Send(message, MessageContext.FileMenuMessage);
            }, new string[] { "txt", "dbtv", "dbtv2" }, "Select a BrainTV Database File", DatabaseService.DefaultPath);
#else
            string filePath = FileBrowser.GetExistingFileName(new string[] { "txt", "dbtv", "dbtv2" }, "Select a BrainTV Database File", DatabaseService.DefaultPath);
            FileMenuMessage message = new FileMenuMessage
            {
                TaskToExecute = FileMenuMessage.Task.OpenDatabase,
                FilePath = filePath
            };
            Messenger.Default.Send(message, MessageContext.FileMenuMessage);
#endif
            Close();
        }

        private void SaveDatabase()
        {
            FileMenuMessage message = new FileMenuMessage
            {
                TaskToExecute = FileMenuMessage.Task.Save
            };
            Messenger.Default.Send(message, MessageContext.FileMenuMessage);
            Close();
        }

        private void SaveDatabaseAs()
        {
            FileMenuMessage message = new FileMenuMessage
            {
                TaskToExecute = FileMenuMessage.Task.SaveAs
            };
            Messenger.Default.Send(message, MessageContext.FileMenuMessage);
            Close();
        }

        private void CloseWindow()
        {
            FileMenuMessage message = new FileMenuMessage
            {
                TaskToExecute = FileMenuMessage.Task.Exit
            };
            Messenger.Default.Send(message, MessageContext.FileMenuMessage);
            Close();
        }
    }
}