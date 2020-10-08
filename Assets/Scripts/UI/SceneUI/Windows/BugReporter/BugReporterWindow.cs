using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Tools.Unity
{
    public class BugReporterWindow : MonoBehaviour
    {
        #region Properties
        [SerializeField] Button m_Close = null;
        [SerializeField] InputField m_NameInputField;
        [SerializeField] InputField m_EmailInputField;
        [SerializeField] InputField m_DescriptionInputField;
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
                    ApplicationState.displayConfirmation("Empty description", "The description field is empty; we might not be able to help you properly.\nDo you still want to send the bug report without any description ?",
                        () =>
                        {
                            SendMail();
                            Destroy(gameObject);
                        },
                        () => { });
                }
                else
                {
                    SendMail();
                    Destroy(gameObject);
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                if (e is SmtpException)
                {
                    ApplicationState.displayMessage("NOK", "The report could not be sent", "Please check your internet connection and try again.");
                }
                else
                {
                    ApplicationState.displayMessage("NOK", e.Source, e.Message);
                }
                Destroy(gameObject);
            }
        }

        private void SendMail()
        {
            using (SmtpClient smtpServer = new SmtpClient("smtp-mail.outlook.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("btvreplayhelp@outlook.fr", "***REMOVED***") as ICredentialsByHost,
                EnableSsl = true
            })
            {
                ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
                using (MailMessage mail = new MailMessage())
                {
                    mail.From = new MailAddress("btvreplayhelp@outlook.fr", "Bug Reporter");
                    mail.To.Add("btvreplayhelp@outlook.fr");
                    mail.Subject = "BUGREPORT " + DateTime.Now.ToString("yyyy-MM-dd HH:mm");

                    StringBuilder bodyBuilder = new StringBuilder();
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
                    mail.Body = bodyBuilder.ToString();

                    string logFile = "";
                    switch (Application.platform)
                    {
                        case RuntimePlatform.OSXPlayer:
                            logFile = Path.Combine("~", "Library", "Logs", Application.companyName, Application.productName, "Player.log");
                            break;
                        case RuntimePlatform.WindowsPlayer:
                            logFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "..", "LocalLow", Application.companyName, Application.productName, "Player.log");
                            break;
                        case RuntimePlatform.LinuxPlayer:
                            logFile = Path.Combine("~", ".config", "unity3d", Application.companyName, Application.productName, "Player.log");
                            break;
                        default:
                            logFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "..", "LocalLow", Application.companyName, Application.productName, "Player.log");
                            break;
                    }
                    if (File.Exists(logFile))
                    {
                        string copiedLogFile = Path.Combine(Application.dataPath, "error_log.txt");
                        File.Copy(logFile, copiedLogFile, true);
                        using (Attachment log = new Attachment(copiedLogFile))
                        {
                            mail.Attachments.Add(log);
                            smtpServer.Send(mail);
                        }
                    }
                    else
                    {
                        smtpServer.Send(mail);
                    }

                    ApplicationState.displayMessage("INFO", "Bug report successfully sent.", "The issue will be adressed as soon as possible. If you've entered your contact information, we may contact you for further information concerning the bug you encountered.");
                }
            }
        }
    }
}