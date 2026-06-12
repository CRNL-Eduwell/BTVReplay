using UnityEngine;
using UnityEngine.UI;

namespace BTV.UI.PatientBaseManager
{
    public class OptionMenu : Menu
    {
        [SerializeField]
        private Button m_UserPreferences = null;

        private void Start()
        {
            m_UserPreferences.onClick.AddListener(OpenUserPreferencesWindow);
        }

        private void OnDestroy()
        {
            m_UserPreferences.onClick.RemoveAllListeners();
        }

        private void OpenUserPreferencesWindow()
        {
            ShowWindowMessage message = new ShowWindowMessage
            {
                WindowName = "DBUserPreferences"
            };
            Messenger.Default.Send(message, MessageContext.ShowWindowMessage);
            Close();
        }
    }
}