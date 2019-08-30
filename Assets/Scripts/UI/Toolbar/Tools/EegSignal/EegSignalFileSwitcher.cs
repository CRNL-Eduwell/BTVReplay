using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UI;
using UnityEngine;

namespace BTV.UI.Module3D.Tools
{
    public delegate void idFileChangedEventHandler(int UpdatedEegFileID);

    class EegSignalFileSwitcher : Tool
    {
        public event idFileChangedEventHandler idFileHasChanged;

        [SerializeField]
        private Dropdown m_FileDropDown = null;

        public override void Initialize()
        {
            //LoadFileNames();
            //m_FileDropDown.itemText.text = m_FileDropDown.options[m_FileDropDown.value].text;

            m_FileDropDown.options.Clear();
            m_FileDropDown.onValueChanged.AddListener((id)=> 
            {
                idFileHasChanged(id);
            });
        }

        //TODO : Need to be called when loading a patient with a list of valid file
        //       because rigth now it's just loaded stupidely without knowing what is behind
        //
        //Load File Names and/or handle that are correct 
        //ie : exists and / or abble to be loaded in memory
        private void LoadFileNames()
        {
            m_FileDropDown.options.Clear();
            for (int i = 0; i < 6; i++)
            {
                m_FileDropDown.options.Add(new Dropdown.OptionData("File " + i));
            }
        }
    }
}
