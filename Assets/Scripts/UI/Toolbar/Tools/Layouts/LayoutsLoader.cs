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

        protected override void OnInitialize()
        {
            m_LoadFile.onClick.AddListener(Load);
        }

        private void Load()
        {
            string directory = TracesService.GetOptionsFor(PatientSession, 0).FileHandle.Directory;

#if UNITY_STANDALONE_OSX
            FileBrowser.GetExistingFileNameAsync((str) =>
            {
                if (!string.IsNullOrEmpty(str))
                {
                    LoadFile.Invoke(str);
                }
            }, new string[] { "workspace" }, "Select a Workspace File", directory);
#else
            string filePath = FileBrowser.GetExistingFileName(new string[] { "workspace" }, "Select a Workspace File", directory);
            if(!string.IsNullOrEmpty(filePath))
                LoadFile.Invoke(filePath);
#endif
        }
    }
}
