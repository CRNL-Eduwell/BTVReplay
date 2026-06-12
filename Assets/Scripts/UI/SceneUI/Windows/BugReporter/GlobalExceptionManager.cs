using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tools.Unity
{
    public class GlobalExceptionManager : MonoBehaviour // Maybe FIXME : integrate this to the windows system
    {
        #region Private Methods
        private void OnEnable()
        {
            Application.logMessageReceived += HandleException;
        }
        private void OnDisable()
        {
            Application.logMessageReceived -= HandleException;
        }
        private void HandleException(string condition, string stackTrace, LogType type)
        {
            if (type == LogType.Exception)
            {
                OpenBugReporter();
            }
        }
        #endregion

        #region Public Methods
        public void OpenBugReporter()
        {
            ShowWindowMessage message = new ShowWindowMessage
            {
                WindowName = "BugReporterWindow"
            };
            Messenger.Default.Send(message, MessageContext.ShowWindowMessage);
        }
        #endregion
    }
}