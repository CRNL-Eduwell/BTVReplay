using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace BTV.UI.Module3D.Tools
{
    class LayoutsLoader : Tool
    {
        public GenericEvent<string> LoadFile = new GenericEvent<string>();

        [SerializeField]
        private Button m_LoadFile = null;

        public override void Initialize()
        {
            m_LoadFile.onClick.AddListener(Load);
        }

        private void Load()
        {
            string filePath = FileBrowser.GetExistingFileName(new string[] { "workspace" }, "Select a Workspace File", ApplicationState.Module3D.Window1.TraceEeg.FileHandle.Directory);
            if(filePath != "")
                LoadFile.Invoke(filePath);
        }
    }
}
