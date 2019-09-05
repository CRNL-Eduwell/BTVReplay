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
            m_LoadFile.onClick.AddListener(() =>
            {
                //Need to change Appliction State file folder with the folder of the project , not just the eeg ? 
                string filePath = FileBrowser.getOpenFileName(new string[] { "btv", "pos" }, "Select an Event File", ApplicationState.Window1.TraceEeg.FileHandle.Directory);
                LoadFile.Invoke(filePath);
            });
        }
    }
}
