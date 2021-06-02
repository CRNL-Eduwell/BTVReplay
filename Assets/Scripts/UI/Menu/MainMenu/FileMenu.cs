using BTV.Services.DatabaseService;
using SFB;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BTV.UI.MainWindow
{
    public class FileMenu : Menu
    {
        [SerializeField]
        private Button m_OpenDatabase = null;
        [SerializeField]
        private Button m_Close = null;

        private void Start()
        {
            m_OpenDatabase.onClick.AddListener(OpenDatabase);
            m_Close.onClick.AddListener(Quit);
        }

        private void OnDestroy()
        {
            m_OpenDatabase.onClick.RemoveAllListeners();
            m_Close.onClick.RemoveAllListeners();
        }

        private void OpenDatabase()
        {
            ShowWindowMessage message = new ShowWindowMessage
            {
                TaskToExecute = 0,
                WindowName = "SubjectDatabase"
            };
            Messenger.Default.Send(message, MessageContext.ShowWindowMessage);
            Close();
        }

        private void Quit()
        {
            ApplicationState.displayConfirmation("Quit BTVReplay?", "Are you sure you want to quit BTVReplay? Make sure all your data is saved.", () => { Application.Quit(); }, () => { });
        }
    }
}