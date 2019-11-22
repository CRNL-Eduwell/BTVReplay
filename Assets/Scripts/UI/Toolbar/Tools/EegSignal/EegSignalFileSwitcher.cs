using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UI;
using UnityEngine;
using BTV.Services.EventsService;
using BTV.Services.EegFileService;

namespace BTV.UI.Module3D.Tools
{
    public delegate void EegFileIDUpdated(int UpdatedEegFileID);

    class EegSignalFileSwitcher : Tool
    {
        public event EegFileIDUpdated UpdateEegFileID;

        [SerializeField]
        private Dropdown m_FileDropDown = null;
        private bool m_InitInteractableDone = false;

        public override void Initialize()
        {
            LoadFileNames();
            Messenger.Default.Register<LoaderToBrainMessage>(this, OnBrainLoaderMessage, MessageContext.LoaderToBrain);
            m_FileDropDown.onValueChanged.AddListener((id) => { UpdateEegFileID(id); });
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

        //Ugly way to have a message from the loading phase , at this point when we are
        //loading the brain it means the eeg data is already loaded and we can request informations
        //
        //TODO : Later implement some nice messages at the loading step to get this information
        private void OnBrainLoaderMessage(LoaderToBrainMessage message)
        {
            if(!m_InitInteractableDone)
                SetFileInteractability();
        }

        private void SetFileInteractability()
        {
            var dropDownList = GetComponentInChildren<DropDownController>(true);
            for (int i = 0; i < 6; i++)
            {
                if (!EegFileService.IsFileIdValid(i))
                {
                    dropDownList.indexesToDisable.Add(i);
                }
            }
            m_InitInteractableDone = true;
        }
    }
}
