using System.Collections;
using System.Collections.Generic;
using BTV.Services.EegFileService;
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

        public override void Initialize()
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
            if (message.Task == LoaderMessage.LoaderTask.LoadBrain)
            {
                if (!m_InitInteractableDone)
                {
                    SetFileLabels();
                    SetFileInteractability();
                }
            }
        }

        //TODO : At one point create something of a Subject info service that returns info
        //like the labels of the files and informations for which you don't need to have 
        //access to the data structures
        private void SetFileLabels()
        {
            Subject subject = ApplicationState.Module3D.Patient;

            m_EegFiles.options.Clear();
            foreach (var item in subject.Files)
            {
                string label = item.Equals(default(KeyValuePair<string, IEegFileInfo>)) ? "NO FILE" : item.Key;
                m_EegFiles.options.Add(new Dropdown.OptionData(label));
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

        private void Load()
        {
            Data.BtvProgram program = EegFileService.ChangeContainerHandle(null, m_EegFiles.value);
            if (program != null)
            {
                List<Data.BtvEvent> evs = new List<Data.BtvEvent>(program.Events);
                LoadEvents.Invoke(evs);
            }
        }
    }
}