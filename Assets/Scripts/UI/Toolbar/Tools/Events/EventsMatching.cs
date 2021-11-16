using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.IO;

namespace BTV.UI.Module3D.Tools
{
    class EventsMatching : Tool
    {
        public GenericEvent<string> LoadFile = new GenericEvent<string>();

        [SerializeField]
        private Button m_LoadFile = null;

        [SerializeField]
        private Text _FileShortName = null;

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
                {
                    _FileShortName.text = Path.GetFileName(str) + " loaded";
                    LoadFile.Invoke(str);
                }
            }, new string[] { "match" }, "Select a code matching file");
#else
            string filePath = FileBrowser.GetExistingFileName(new string[] { "match" }, "Select a code matching file");
            if (!string.IsNullOrEmpty(filePath))
            {
                _FileShortName.text = Path.GetFileName(filePath) + " loaded";
                LoadFile.Invoke(filePath);
            }
#endif
        }
    }
}
