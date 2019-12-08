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

        public override void Initialize()
        {
            m_SaveFile.onClick.AddListener(() =>
            {
                string filePath = FileBrowser.GetSavedFileName(m_ExtensionList, "Save Event File", ApplicationState.Module3D.Window1.TraceEeg.FileHandle.Directory);
                if (filePath != "")
                    SaveFile.Invoke(filePath);
            });
        }
    }
}
