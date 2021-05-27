using BTV.Services.DatabaseService;
using SFB;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BTV.UI.MainWindow
{
    public class ToolsMenu : Menu
    {
        [SerializeField]
        private Button m_OpenUserPreferences = null;

        private void Start()
        {
            m_OpenUserPreferences.onClick.AddListener(OpenUserPreferences);
        }

        private void OnDestroy()
        {
            m_OpenUserPreferences.onClick.RemoveAllListeners();
        }

        private void OpenUserPreferences()
        {
            ShowWindowMessage message = new ShowWindowMessage
            {
                TaskToExecute = 0,
                WindowName = "GeneralOptionsPreferences"
            };
            Messenger.Default.Send(message, MessageContext.ShowWindowMessage);
            Close();
        }
    }
}