using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UI;
using UnityEngine;
using BTV.Services.EventsService;
using BTV.Services.EegFileService;
using System.Collections.Generic;
using BTV.Services.SubjectInfoService;

namespace BTV.UI.Module3D.Tools
{
    public delegate void EegFileIDUpdated(int UpdatedEegFileID);

    class EegSignalFileSwitcher : Tool
    {
        public event EegFileIDUpdated UpdateEegFileID;

        [SerializeField]
        private Dropdown m_FileDropDown = null;
        private bool m_InitInteractableDone = false;

        protected override void OnInitialize()
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
            if (EegFileService.IsFileIdValid(PatientSession, nextValue))
            {
                m_FileDropDown.value = nextValue;
            }
        }
        private void OnLoaderMessage(LoaderMessage message)
        {
            if (message.Task == LoaderMessage.LoaderTask.EegFilesReady && ReferenceEquals(PatientSession, message.PatientSession))
            {
                if (!m_InitInteractableDone)
                {
                    SetFileLabels();
                    SetFileInteractability();
                    m_FileDropDown.onValueChanged.AddListener((id) => { UpdateEegFileID?.Invoke(id); });
                }
            }
        }

        private void SetFileLabels()
        {
            List<string> keys = SubjectInfoService.GetSubjectFileKeys(PatientSession);

            m_FileDropDown.options.Clear();
            foreach (var item in keys)
            {
                m_FileDropDown.options.Add(new Dropdown.OptionData(item));
            }
        }

        private void SetFileInteractability()
        {
            var dropDownList = GetComponentInChildren<DropDownController>(true);
            for (int i = 0; i < BTV.Data.EegSlots.Count; i++)
            {
                if (!EegFileService.IsFileIdValid(PatientSession, i))
                {
                    dropDownList.indexesToDisable.Add(i);
                }
            }
            m_InitInteractableDone = true;
        }
    }
}
