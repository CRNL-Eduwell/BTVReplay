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
    class LayoutsSaver : Tool
    {
        public GenericEvent<string> SaveFile = new GenericEvent<string>();

        [SerializeField]
        private Button m_SaveFile = null;

        private ExtensionFilter[] m_ExtensionList = { new ExtensionFilter("Workspace File", "workspace") };

        public override void Initialize()
        {
            m_SaveFile.onClick.AddListener(() =>
            {
                string filePath = FileBrowser.GetSavedFileName(m_ExtensionList, "Save Workspace File", ApplicationState.Module3D.Window1.TraceEeg.FileHandle.Directory);
                if (filePath != "")
                    SaveFile.Invoke(filePath);
            });
        }
    }
}
