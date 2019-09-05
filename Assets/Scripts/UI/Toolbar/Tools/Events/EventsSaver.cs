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
    class EventsSaver : Tool
    {
        public GenericEvent<string> SaveFile = new GenericEvent<string>();

        [SerializeField]
        private Button m_SaveFile = null;

        public override void Initialize()
        {
            m_SaveFile.onClick.AddListener(() =>
            {                
                //Need to change Appliction State file folder with the folder of the project , not just the eeg ? 
                string filePath = FileBrowser.getSaveFileName(new string[] { "pos" }, "Save Event File", ApplicationState.Window1.TraceEeg.FileHandle.Directory);
                SaveFile.Invoke(filePath);
            });
        }
    }
}
