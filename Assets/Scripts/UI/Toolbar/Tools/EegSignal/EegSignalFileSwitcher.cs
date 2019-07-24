using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UI;
using UnityEngine;

namespace BTV.UI.Module3D.Tools
{
    class EegSignalFileSwitcher : Tool
    {
        public event idFileChangedEventHandler idFileHasChanged;

        [SerializeField]
        private Dropdown m_FileDropDown = null;

        public override void Initialize()
        {
            LoadFileNames();
            m_FileDropDown.itemText.text = m_FileDropDown.options[m_FileDropDown.value].text;
            m_FileDropDown.onValueChanged.AddListener((id)=> { idFileHasChanged(id); });
        }

        //Load File Names and/or handle that are correct 
        //ie : exists and / or abble to be loaded in memory
        private void LoadFileNames()
        {

        }
    }
}
