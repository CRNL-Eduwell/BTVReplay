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
    class EventsLoader : Tool
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
#if UNITY_STANDALONE_OSX
            FileBrowser.GetExistingFileNameAsync((str) =>
            {
                if (!string.IsNullOrEmpty(str))
                    LoadFile.Invoke(str);
            }, new string[] { "btv", "pos" }, "Select an Event File", ApplicationState.Module3D.Window1.TraceEeg.FileHandle.Directory);
#else
            //Need to change Appliction State file folder with the folder of the project , not just the eeg ? 
            string filePath = FileBrowser.GetExistingFileName(new string[] { "btv", "pos" }, "Select an Event File", ApplicationState.Module3D.Window1.TraceEeg.FileHandle.Directory);
            if (!string.IsNullOrEmpty(filePath))
                LoadFile.Invoke(filePath);
#endif
        }
    }
}
