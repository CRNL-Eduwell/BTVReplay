using BTV.Services.DatabaseService;
using SFB;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace BTV.UI.MainWindow
{
    public class FileMenu : Menu
    {
        [SerializeField]
        private Button m_OpenSingleDataset = null;
        [SerializeField]
        private Button m_OpenDatabase = null;
        [SerializeField]
        private Button m_Close = null;

        private string[] _FileExtensions = new string[] { "TRC", "trc", "eeg", "vhdr", "edf" };

        private void Start()
        {
            m_OpenSingleDataset.onClick.AddListener(OpenSingleDataset);
            m_OpenDatabase.onClick.AddListener(OpenDatabase);
            m_Close.onClick.AddListener(Quit);
        }

        private void OnDestroy()
        {
            m_OpenSingleDataset.onClick.RemoveAllListeners();
            m_OpenDatabase.onClick.RemoveAllListeners();
            m_Close.onClick.RemoveAllListeners();
        }

        private void OpenSingleDataset()
        {
#if UNITY_STANDALONE_OSX
            FileBrowser.GetExistingFileNameAsync((str) =>
            {
                if (!string.IsNullOrEmpty(str))
                {
                    OpenDataset(str);
                }
            }, _FileExtensions);
#else
            string str = FileBrowser.GetExistingFileName(_FileExtensions);
            if (!string.IsNullOrEmpty(str))
            {
                OpenDataset(str);
            }
#endif
        }

        private void OpenDataset(string path)
        {
            Dictionary<string, IEegFileInfo> kvps = new Dictionary<string, IEegFileInfo>();
            FileInfo fileInfo = new FileInfo(path);
            if (fileInfo.Extension == ".TRC")
            {
                kvps.Add("Dataset", new MicromedFileInfo(path));
            }
            else if (fileInfo.Extension == ".eeg")
            {
                kvps.Add("Dataset", new ElanFileInfo(path));
            }
            else if (fileInfo.Extension == ".vhdr")
            {
                kvps.Add("Dataset", new BrainvisionFileInfo(path));
            }
            else if (fileInfo.Extension == ".edf")
            {
                kvps.Add("Dataset", new EdfFileInfo(path));
            }
            else
            {
                //Should never arriver at this point
                return;
            }

            //Create subject on the fly
            Experiment exp = new Experiment("Single Dataset", kvps, "");
            Subject s = new Subject(fileInfo.Name);
            s.Experiments.Add(exp);

            //Then send it to the loader
            LoadSubjectMessage message = new LoadSubjectMessage
            {
                subject = new Subject(s),
                label = "Single Dataset"
            };
            Messenger.Default.Send(message, MessageContext.LoadSubjectMessage);
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