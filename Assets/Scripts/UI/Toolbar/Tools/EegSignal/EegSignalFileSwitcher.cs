using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UI;
using UnityEngine;
using BTV.Services.EventsService;
using BTV.Services.EegFileService;
using System.Collections.Generic;

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
            Messenger.Default.Register<LoaderMessage>(this, OnLoaderMessage, MessageContext.LoaderMessage);
        }

        private void OnDestroy()
        {
            Messenger.Default.Unregister(this, MessageContext.LoaderMessage);
        }

        //= -1 previous +1 next 0 nothing
        public void ChangeFile(int direction)
        {
            int currentValue = m_FileDropDown.value;
            int nextValue = currentValue + direction;
            if (EegFileService.IsFileIdValid(nextValue))
            {
                m_FileDropDown.value = nextValue;
            }
        }
        private void OnLoaderMessage(LoaderMessage message)
        {
            if (message.Task == LoaderMessage.LoaderTask.LoadBrain)
            {
                if (!m_InitInteractableDone)
                {
                    SetFileLabels();
                    SetFileInteractability();
                    m_FileDropDown.onValueChanged.AddListener((id) => { UpdateEegFileID(id); });
                }
            }
        }

        //TODO : At one point create something of a Subject info service that returns info
        //like the labels of the files and informations for which you don't need to have 
        //access to the data structures
        private void SetFileLabels()
        {
            Subject subject = ApplicationState.Module3D.Patient;

            m_FileDropDown.options.Clear();
            foreach (var item in subject.Files)
            {
                string label = item.Equals(default(KeyValuePair<string, IEegFileInfo>)) ? "NO FILE" : item.Key;
                m_FileDropDown.options.Add(new Dropdown.OptionData(label));
            }
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
