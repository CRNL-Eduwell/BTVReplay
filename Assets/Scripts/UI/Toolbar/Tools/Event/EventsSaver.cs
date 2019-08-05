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
                string filePath = FileBrowser.getSaveFileName(new string[] { "pos" }, "Save Event File", ApplicationState.CurrentSelectedFile.fileFolder);
                SaveFile.Invoke(filePath);
            });
        }
    }
}
