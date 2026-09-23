using System;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Tools.Unity
{
    public class BugReporterWindow : MonoBehaviour
    {
        #region Properties
        [SerializeField] Button m_Close = null;
        [SerializeField] InputField m_NameInputField = null;
        [SerializeField] InputField m_EmailInputField = null;
        [SerializeField] InputField m_DescriptionInputField = null;
        [SerializeField] Button m_Submit = null;
        [SerializeField] Button m_Cancel = null;
        #endregion

        private void Start()
        {
            m_Close.onClick.AddListener(() => { Destroy(gameObject); });
            m_Submit.onClick.AddListener(OK);
            m_Cancel.onClick.AddListener(() => { Destroy(gameObject); });
        }

        private void OnDestroy()
        {
            m_Close.onClick.RemoveAllListeners();
            m_Submit.onClick.RemoveAllListeners();
            m_Cancel.onClick.RemoveAllListeners();
        }

        private void OK()
        {
            try
            {
                if (string.IsNullOrEmpty(m_DescriptionInputField.text))
                {
                    ApplicationState.displayConfirmation("Empty description", "The description field is empty; we might not be able to help you properly.\nDo you still want to copy the bug report without any description ?",
                        () =>
                        {
                            CopyReportToClipboard();
                            Destroy(gameObject);
                        },
                        () => { });
                }
                else
                {
                    CopyReportToClipboard();
                    Destroy(gameObject);
                }
            }
            catch (Exception e)
            {
                // Not LogException: that would reopen the bug reporter from inside itself.
                BtvLog.Handled("Bug report could not be prepared", e);
                ApplicationState.displayMessage(e.Source, "NOK", e.Message);
                Destroy(gameObject);
            }
        }

        private void CopyReportToClipboard()
        {
            StringBuilder bodyBuilder = new StringBuilder();
            bodyBuilder.AppendLine("BUGREPORT " + DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
            bodyBuilder.AppendFormat
            (
                "{0} {1} {2} {3}\n{4}, {5}, {6}x {7}\n{8}x{9} {10}dpi FullScreen {11}, {12}, {13} vmem: {14} Max Texture: {15}\n",
                SystemInfo.deviceModel,
                SystemInfo.deviceName,
                SystemInfo.deviceType,
                SystemInfo.deviceUniqueIdentifier,

                SystemInfo.operatingSystem,
                SystemInfo.systemMemorySize,
                SystemInfo.processorCount,
                SystemInfo.processorType,

                Screen.currentResolution.width,
                Screen.currentResolution.height,
                Screen.dpi,
                Screen.fullScreen,
                SystemInfo.graphicsDeviceName,
                SystemInfo.graphicsDeviceVendor,
                SystemInfo.graphicsMemorySize,
                SystemInfo.maxTextureSize
            );
            bodyBuilder.AppendLine(" ");
            bodyBuilder.AppendLine(m_NameInputField.text);
            bodyBuilder.AppendLine(m_EmailInputField.text);
            bodyBuilder.AppendLine(" ");
            bodyBuilder.AppendLine(m_DescriptionInputField.text);
            bodyBuilder.AppendLine(" ");
            bodyBuilder.AppendLine("Log file: " + GetLogFilePath());

            GUIUtility.systemCopyBuffer = bodyBuilder.ToString();

            ApplicationState.displayMessage("Bug report copied to clipboard.", "INFO", "The report has been copied to your clipboard. Paste it into an email or an issue, and attach the log file if possible:\n" + GetLogFilePath());
        }

        private static string GetLogFilePath()
        {
            switch (Application.platform)
            {
                case RuntimePlatform.OSXPlayer:
                    return Path.Combine("~", "Library", "Logs", Application.companyName, Application.productName, "Player.log");
                case RuntimePlatform.LinuxPlayer:
                    return Path.Combine("~", ".config", "unity3d", Application.companyName, Application.productName, "Player.log");
                default:
                    return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "..", "LocalLow", Application.companyName, Application.productName, "Player.log");
            }
        }
    }
}
