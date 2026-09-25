using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFB;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace BTV.UI.Module3D.Tools
{
    class EventsSaver : Tool
    {
        public GenericEvent<string> SaveFile = new GenericEvent<string>();

        [SerializeField]
        private Button m_SaveFile = null;

        private ExtensionFilter[] m_ExtensionList = { new ExtensionFilter("BrainTV Event File", "btv"), new ExtensionFilter("Elan Event File", "pos") };

        protected override void OnInitialize()
        {
            m_SaveFile.onClick.AddListener(Save);
        }

        private void Save()
        {
            string directory = TracesService.GetOptionsFor(PatientSession, 0).FileHandle.Directory;

#if UNITY_STANDALONE_OSX
            FileBrowser.GetSavedFileNameAsync((str) =>
            {
                if (!string.IsNullOrEmpty(str))
                    SaveFile.Invoke(str);
            }, m_ExtensionList, "Save Event File", directory);
#else
                string filePath = FileBrowser.GetSavedFileName(m_ExtensionList, "Save Event File", directory);
                if (!string.IsNullOrEmpty(filePath))
                    SaveFile.Invoke(filePath);
#endif
        }
    }
}
