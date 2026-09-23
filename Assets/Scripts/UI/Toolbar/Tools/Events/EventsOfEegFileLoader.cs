using System.Collections;
using System.Collections.Generic;
using BTV.Services.EegFileService;
using BTV.Services.SubjectInfoService;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace BTV.UI.Module3D.Tools
{
    public class EventsOfEegFileLoader : Tool
    {
        public GenericEvent<List<Data.BtvEvent>> LoadEvents = new GenericEvent<List<Data.BtvEvent>>();

        [SerializeField]
        private Dropdown m_EegFiles = null;

        [SerializeField]
        private Button m_LoadFile = null;

        private bool m_InitInteractableDone = false;

        protected override void OnInitialize()
        {
            Messenger.Default.Register<LoaderMessage>(this, OnLoaderMessage, MessageContext.LoaderMessage);


            m_LoadFile.onClick.AddListener(Load);
        }

        private void OnDestroy()
        {
            Messenger.Default.Unregister(this, MessageContext.LoaderMessage);
        }

        private void OnLoaderMessage(LoaderMessage message)
        {
            if (message.Task == LoaderMessage.LoaderTask.EegFilesReady && ReferenceEquals(PatientSession, message.PatientSession))
            {
                if (!m_InitInteractableDone)
                {
                    SetFileLabels();
                    SetFileInteractability();
                }
            }
        }

        private void SetFileLabels()
        {
            List<string> keys = SubjectInfoService.GetSubjectFileKeys(PatientSession);

            m_EegFiles.options.Clear();
            foreach (var item in keys)
            {
                m_EegFiles.options.Add(new Dropdown.OptionData(item));
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

        private void Load()
        {
            Data.BtvProgram program = EegFileService.ChangeContainerHandle(PatientSession, null, m_EegFiles.value);
            if (program != null)
            {
                List<Data.BtvEvent> evs = new List<Data.BtvEvent>(program.Events);
                LoadEvents.Invoke(evs);
            }
        }
    }
}
